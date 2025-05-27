using System.Runtime.InteropServices.JavaScript;
using WizardIslandRestApi.Game.Spells.Debuffs;

namespace WizardIslandRestApi.Game.Spells.Ultimates
{
    public class BloodWorm : Spell
    {
        public override string Name { get { return "Blood worm"; } }
        public const int WormPartCost = 10;
        private const int _minWormPartCount = 3; // shoots, you get for free
        private const int _ticksBetweenWormCreation = 1 * Game._updatesPerSecond;
        private const int _wormLifetime = (int)(30 * Game._updatesPerSecond);
        private const float _circlingRadius = 7.5f;

        private float _damage = 5;
        private float _knockback = 1.75f;

        private int _shots = 0;
        private WaitToDoSomethingEntity? _entityToCreateWorm = null;
        private BloodWormEntity? _tail = null;

        private enum BloodWormStates { Ready, CreatingWorm, ShootoutWorm }
        public override bool CanBeReplaced { get { return _currentState == BloodWormStates.Ready; } protected set { } }
        public BloodWorm(Player player) : base(player)
        {
            Type = SpellType.Ultimate;
        }

        public override int CooldownMax { get; protected set; } = 45 * Game._updatesPerSecond;

        BloodWormStates _currentState = BloodWormStates.Ready;


        public override void OnCast(Vector2 pos, Vector2 mousePos)
        {
            switch (_currentState)
            {
                case BloodWormStates.Ready:
                    _currentState = BloodWormStates.CreatingWorm;
                    _shots = 0;
                    for (int i = 0; i < _minWormPartCount; i++)
                        CreateWormPart(pos, mousePos);
                    _entityToCreateWorm = new WaitToDoSomethingEntity(0, () =>
                    {
                        _entityToCreateWorm.TicksToWait = _ticksBetweenWormCreation;
                        // check the player has the health required
                        if (MyPlayer.Stats.Health <= WormPartCost)
                        {
                            OnCast(pos, mousePos);
                            return;
                        }
                        MyPlayer.Stats.Health -= WormPartCost; // we don't want to scale it with damage multipliers, in this case
                                                                // actually create the worm part
                        CreateWormPart(pos, mousePos);
                    });
                    GetCurrentGame().Entities.Add(_entityToCreateWorm);
                    break;
                case BloodWormStates.CreatingWorm:
                    _currentState = BloodWormStates.ShootoutWorm;
                    _entityToCreateWorm.ActionAfterWaiting = () => { _entityToCreateWorm = null; };
                    break;
                case BloodWormStates.ShootoutWorm:
                    ShootWorm(mousePos);
                    if (_shots <= 0)
                        ForceGoOnCooldown();
                    break;
            }
            
        }
        private void CreateWormPart(Vector2 pos, Vector2 target)
        {
            _shots++;
            //var dir = (target - pos).Normalized();
            BloodWormEntity temp = new BloodWormEntity(MyPlayer, pos + (target - pos).Normalized() * _circlingRadius * .5f, _tail, this)
            {
                TicksUntillDeletingMax = _wormLifetime,
                Damage = _damage,
                Knockback = _knockback,
                Size = 2,
                Color = "130, 0, 200",
                CirclingRadius = _circlingRadius * (1 + (float)_shots * .15f),
                Speed = (100.0f / Game._updatesPerSecond),
            };
            if (_tail != null)
            {
                _tail.Child = temp;
                _tail.EntityId = _tail.Parent == null ? "BloodWormHead" : "BloodWormBody";
            }
            _tail = temp;
            _tail.EntityId = _tail.Parent == null ? "BloodWormHead" : "BloodWormTail";
            GetCurrentGame().Entities.Add(temp);
        }
        private void ShootWorm(Vector2 target)
        {
            if (_tail.StartAttacking(target))
                _shots--;
        }
        public void ForceGoOnCooldown()
        {
            if (_currentState == BloodWormStates.Ready)
                return;
            GoOnCooldown();
            _currentState = BloodWormStates.Ready;
            _tail = null;
        }

        public override void FullReset()
        {
            _currentState = BloodWormStates.Ready;
            _tail = null;
            base.FullReset();
        }
    }
    public class BloodWormEntity : Entity
    {
        private int _ticksUntillDeleting;
        private int _ticksUntillDeletingMax;
        private Vector2 _dir;
        private Vector2 _target;
        private BloodWorm _spell;
        private List<Player> _hitPlayers = new List<Player>();
        // head stuff
        private float _circlingAngle = 0;
        private float _circlingRadius;
        public float CirclingRadius { get { return _circlingRadius; } set { _circlingRadius = value; if (Parent != null) Parent.CirclingRadius = value; } }

        public int TicksUntillDeletingMax { get { return _ticksUntillDeletingMax; } set { _ticksUntillDeletingMax = value; _ticksUntillDeleting = value; } }
        public float Damage { get; set; }
        public float Knockback { get; set; }
        public float Speed { get; set; }
        public float RotationSpeed { get { return Speed / 20; } }
        public enum BloodWormEntityState { Dorment, Attacking, Retreating }
        private Player _owner;
        public BloodWormEntity? Parent { get; private set; }
        public BloodWormEntity? Child { get; set; }
        public BloodWormEntityState CurrentState { get; private set; } = BloodWormEntityState.Retreating;
        
        public BloodWormEntity(Player owner, Vector2 startPos, BloodWormEntity? parent, BloodWorm spell) : base(owner, startPos)
        {
            _owner = owner;
            Parent = parent;
            _spell = spell;
            Height = EntityHeight.Ground;
        }
        public override bool Update()
        {
            _ticksUntillDeleting--;
            if (Parent != null)
            {
                _dir = (Parent.Pos - Pos).Normalized();
                ForwardAngle = HomingBoltEntity.GetAngleFromDirection(_dir);
                Pos = Parent.Pos - _dir * (Size + Parent.Size);
                return false;
            }
            // head movement
            switch (CurrentState)
            {
                case BloodWormEntityState.Dorment:
                    _circlingAngle += Speed / CirclingRadius;
                    Pos = MyCollider.Owner.Pos + new Vector2( MathF.Cos(_circlingAngle), MathF.Sin(_circlingAngle) ) * CirclingRadius;
                    ForwardAngle = _circlingAngle + MathF.PI / 2;
                    break;
                case BloodWormEntityState.Attacking:
                    Pos += _dir * Speed;
                    if (_dir.Dot(_target - Pos) < 0)
                        StopAttacking();
                    break;
                case BloodWormEntityState.Retreating:
                    var targetDir = MyCollider.Owner.Pos - Pos;
                    if (targetDir.LengthSqr() < CirclingRadius * CirclingRadius)
                    {
                        CurrentState = BloodWormEntityState.Dorment;
                        _circlingAngle = HomingBoltEntity.GetAngleFromDirection(targetDir*-1);
                        break;
                    }
                    var targetAngle = HomingBoltEntity.GetAngleFromDirection(targetDir);
                    float diff = HomingBoltEntity.DeltaAngle(_circlingAngle, targetAngle);
                    _circlingAngle += Math.Sign(diff) * Math.Min(Math.Abs(diff), RotationSpeed);
                    ForwardAngle = _circlingAngle;
                    // move
                    _dir = new Vector2(MathF.Cos(_circlingAngle), MathF.Sin(_circlingAngle));
                    Pos += _dir * Speed;
                    break;
            }
            if (_ticksUntillDeleting < 0)
            {
                Die();
                return true;
            }    
            return false;
        }

        public override bool OnCollision(Entity other)
        {
            return false;
        }
        public override bool OnCollision(Player other)
        {
            if (other == MyCollider.Owner || CurrentState != BloodWormEntityState.Attacking || _hitPlayers.Contains(other))
                return false;
            // damage
            _hitPlayers.Add(other);
            other.TakeDamage(Damage, _owner);
            other.ApplyKnockback((other.Pos - MyCollider.PreviousPos).Normalized(), Knockback);
            return false;
        }
        // this entity, can't be retargeted, but it will stop attacking, for now
        public override void ReTarget(Vector2 pos)
        {
            MyCollider.Owner = _owner;
            if (Parent != null)
                Parent.ReTarget(pos);
            else StopAttacking();
        }

        /// <summary>
        /// Returns true, if it was able to attack
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool StartAttacking(Vector2 target)
        {
            if (Parent != null)
                return Parent.StartAttacking(target);
            if (CurrentState != BloodWormEntityState.Dorment)
                return false;
            // this is the head, and it is ready to attack
            _dir = (target - Pos).Normalized();
            _target = target + _dir * Speed * 5;
            CurrentState = BloodWormEntityState.Attacking;
            Height = EntityHeight.Normal;
            ForwardAngle = HomingBoltEntity.GetAngleFromDirection(_dir);
            return true;
        }

        private void StopAttacking()
        {
            if (CurrentState != BloodWormEntityState.Attacking)
                return;
            _ticksUntillDeleting = -1;
        }
        private void Die()
        {
            // give the player the health back
            MyCollider.Owner.Stats.Health += BloodWorm.WormPartCost;
            // set the child as the new head
            if (Child != null)
            {
                Child.EntityId = "BloodWormHead";
                Child._circlingAngle = HomingBoltEntity.GetAngleFromDirection(_dir);
                Child.Parent = null;
            }
            else
                _spell.ForceGoOnCooldown();
            
        }
    }
}
