using static WizardIslandRestApi.Game.Spells.BasicSpells.CirclingSnakePart;

namespace WizardIslandRestApi.Game.Spells.Ultimates
{
    public class MagmaWhale : Spell
    {
        private MagmaWhaleEntity _whale;

        public override int CooldownMax { get; protected set; } = 30 * Game._updatesPerSecond;

        public override string Name => "Magma whale";

        public MagmaWhale(Player player) : base(player)
        {
            Type = SpellType.Ultimate;
            StandardStats.Speed = 1.5f;
            StandardStats.Size = 10.0f;
            StandardStats.Damage = 10.0f;
            StandardStats.Knockback = 4.0f;
        }

        protected override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            if (!_whale.IsReady)
                return;
            _whale.Activate(mousePos);
            GoOnCooldown();
        }

        public override void RemovedFromPlayer()
        {
            base.RemovedFromPlayer();
            _whale.IsDead = true;
        }

        public override void FullReset()
        {
            base.FullReset();
            GetCurrentGame().Entities.Add(_whale = new MagmaWhaleEntity(MyPlayer, this, MyPlayer.Pos));
        }
    }

    public class MagmaWhaleEntity : Entity
    {
        private Player _player;
        private MagmaWhale _spell;
        private string _readyTransparancy = ".35";
        private string _notReadyTransparancy = ".1";

        public bool IsReady { get; private set; }
        public bool IsDead { get; set; } = false;

        public MagmaWhaleEntity(Player owner, MagmaWhale spell, Vector2? startPos = null) : base(owner, startPos)
        {
            _player = owner;
            _spell = spell;
            MyCollider = null;
            VisableTo = owner.Id;
            EntityId = "MagmaWhale";
            Color = "0,0,0";
            Transparancy = ".25";
        }

        public override bool Update()
        {
            if (_player.IsDead)
                return IsDead;
            Size = _spell.StandardStats.Size;

            var game = _player._game;
            float distanceFromShore = Size / 5;
            var targetPos = game.GameMap.GroundMiddle + (_player.Pos - game.GameMap.GroundMiddle).Normalized() * (game.GameMap.CircleRadius + Size + distanceFromShore);
            var moveDir = (targetPos - Pos).Normalized();
            Pos += moveDir * _spell.StandardStats.Speed;
            if (moveDir.Dot(targetPos - Pos) < 0)
                Pos = targetPos;

            float minRadiusFromCenter = game.GameMap.CircleRadius + Size;
            IsReady = (game.GameMap.GroundMiddle - Pos).LengthSqr() > minRadiusFromCenter * minRadiusFromCenter;
            Transparancy = IsReady ? _readyTransparancy : _notReadyTransparancy;

            return IsDead;
        }

        public void Activate(Vector2 targetPosition)
        {
            if (VisableTo == -2) // whale already out
                return;
            VisableTo = -2;
            _player._game.Entities.Add(new MagmaWhaleActivatedEntity(_player, _player._game.GameMap, Pos, targetPosition, Deactivate)
            {
                Damage = _spell.StandardStats.Damage,
                Knockback = _spell.StandardStats.Knockback,
                MaxSpeed = _spell.StandardStats.Speed,
                Size = Size,
                SecondsToReachMaxSpeed = 3.0f,
            });
        }

        public void Deactivate(Entity entity)
        {
            Pos = entity.Pos;
            VisableTo = _player.Id;
        }

        public override bool OnCollision(Player other)
        {
            return false;
        }

        public override void ReTarget(Vector2 pos)
        {
        }
    }

    public class MagmaWhaleActivatedEntity : Entity
    {
        private int _existedTicks = 0;
        private int _ticksToReachMaxSpeed = 0;
        private Vector2 _dir { get; set; }
        private Map _map;
        private Action<Entity> _onExpire;
        private int _hitCooldown = 15;
        private List<PlayerAndHitTime> _hitPlayers = [];

        public float Damage { get; set; }
        public float Knockback { get; set; }
        public float CurrentSpeed { get; set; } = 0;
        public float MaxSpeed { get; set; }
        public float SecondsToReachMaxSpeed { set { _ticksToReachMaxSpeed = (int)(value * Game._updatesPerSecond); } }
        private Vector2 TargetPos { get; set; }
        public MagmaWhaleActivatedEntity(Player owner, Map map, Vector2 startPos, Vector2 targetPos, Action<Entity> onExpire) : base(owner, startPos)
        {
            _onExpire = onExpire;
            TargetPos = targetPos;
            ReTarget(TargetPos);
            _map = map;
            EntityId = "MagmaWhaleActivated";
            Color = "255,50,50";
        }

        public override bool Update()
        {
            CurrentSpeed = MathF.Min((float)(++_existedTicks) / (float)_ticksToReachMaxSpeed * MaxSpeed, MaxSpeed);
            Pos += _dir * CurrentSpeed;

            // remove entity once it has passed the target point, and is outside the map
            float distanceToBeOutOfMap = _map.CircleRadius + Size;
            return _dir.Dot(TargetPos - Pos) < 0 && (_map.GroundMiddle - Pos).LengthSqr() > distanceToBeOutOfMap * distanceToBeOutOfMap;
        }

        public override bool OnCollision(Player other)
        {
            PlayerAndHitTime paht = _hitPlayers.FirstOrDefault(p => p.HitPlayer == other);
            if (paht == null)
                _hitPlayers.Add(paht = new PlayerAndHitTime() { HitPlayer = other, HitTick = -1 });

            int currentTick = MyCollider.Owner._game.GameTick;
            if (paht.HitTick + _hitCooldown > currentTick)
                return false;

            paht.HitTick = currentTick;
            other.TakeDamage(Damage, MyCollider.Owner);
            other.ApplyKnockback(_dir, Knockback);
            return false;
        }

        public override bool OnCollision(Entity other)
        {
            return false;
        }

        public override void ReTarget(Vector2 pos)
        {
            TargetPos = pos;
            _dir = (pos - Pos).Normalized();
            ForwardAngle = MathF.Atan2(_dir.y, _dir.x);
        }

        public override void OnExpire(EntityExpiredReason reason)
        {
            base.OnExpire(reason);
            _onExpire?.Invoke(this);
        }
    }
}
