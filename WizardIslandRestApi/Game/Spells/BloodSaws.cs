using System.Runtime.CompilerServices;
using WizardIslandRestApi.Game.Spells.ExtraEntities;

namespace WizardIslandRestApi.Game.Spells
{
    public class BloodSaws : Spell
    {
        private int _currentCooldown;
        private int _instancesOfDamageTaken = 0;
        private const int _stackCost = 5; // how many instances of damage the player must have taken, before they can cast this ability
        private const int _instancesOfDamageTakenMaxStacks = _stackCost * 4;
        public override string Name => "Blood Saws";
        public BloodSaws(Player player) : base(player)
        {
            if (player == null)
                return;
            player.Stats.Observers.OnHealthChanged += (before, after) =>
            {
                int diff = before - after;
                if (diff > Game.LavaDamage && ++_instancesOfDamageTaken > _instancesOfDamageTakenMaxStacks) // you don't get a stack for lava damage
                {
                    _instancesOfDamageTaken = _instancesOfDamageTakenMaxStacks;
                }
            };
            CurrentCooldown = CooldownMax; // we don't need this here
        }
        public override void OnPlayerReset()
        {
            _instancesOfDamageTaken = _instancesOfDamageTakenMaxStacks;
        }

        public override int CooldownMax { get; protected set; } = 999 * Game._updatesPerSecond;
        public override bool CanCast => _instancesOfDamageTaken >= _stackCost;
        public override int CurrentCooldown { get { return (_instancesOfDamageTaken >= _stackCost) ? -1 : GetCurrentGameTick() + (int)((float)_instancesOfDamageTaken * Game._updatesPerSecond / _stackCost); } set { _currentCooldown = value; } }

        public override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            int sawCount = _instancesOfDamageTaken / _stackCost;
            if (sawCount == 0) return; // this should not happen, but just in case :)
#if !DEBUG
            _instancesOfDamageTaken -= sawCount * _stackCost;
#endif
            // entity attibutes
            int ticksUntillDeletion = 5 * Game._updatesPerSecond;
            float size = 1.5f;
            float damage = 7.5f;
            float knockback = 1.3f;
            float speed = 7.5f / ticksUntillDeletion; // It will go 3 units away from the player
            const float rotationsPerSecond = .5f; // how many times each saw should go around the player, each second
            float rotSpeed = MathF.PI * 2 / Game._updatesPerSecond * rotationsPerSecond;

            // get starting angle
            float angle = Vector2.AngleBetween(startPos, mousePos) - MathF.PI / 2; // 90° offset
            float deltaAngle = MathF.PI * 2 / sawCount;
            Dictionary<Player, int> hitPlayersSharedDictionay = new Dictionary<Player, int>();
            for (int i = 0; i < sawCount; i++)
            {
                float activeAngle = angle + i * deltaAngle;
                GetCurrentGame().Entities.Add(new BloodSawEntity(MyPlayer, ticksUntillDeletion, startPos + new Vector2(MathF.Cos(activeAngle), MathF.Sin(activeAngle)), activeAngle, hitPlayersSharedDictionay)
                {
                    Size = size,
                    Damage = damage,
                    Knockback = knockback,
                    Speed = speed,
                    RotationSpeed = rotSpeed,
                    ForwardAngle = activeAngle
                });
            }
            //GetCurrentGame().Entities.Add(new ShadowEntity() { Pos = startPos + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 5, Size = 3 });
        }
        public override string ToString()
        {
            return base.ToString() + (CanCast ? $" ({_instancesOfDamageTaken / _stackCost})" : "");
        }
    }
    public class BloodSawEntity : EntityPlus
    {
        private float _angle;
        private float _distFromPlayer = 0;
        public float RotationSpeed;
        private Dictionary<Player, int> _hitPlayers;
        public BloodSawEntity(Player owner, int ticksUntilDeletion, Vector2 startPos, float startAngle, Dictionary<Player, int>? hitPlayersDictionary = null) : base(owner, ticksUntilDeletion, startPos)
        {
            TicksUntillCanHitOwner = ticksUntilDeletion + 1;
            EntityId = "BloodSaw";
            Color = "175,0,0";
            _angle = startAngle;
            if (hitPlayersDictionary == null)
                hitPlayersDictionary = new Dictionary<Player, int>();
            _hitPlayers = hitPlayersDictionary;
        }

        public override bool Update()
        {
            ForwardAngle += .1f;
            _angle += RotationSpeed;
            _distFromPlayer += Speed;
            Pos = MyCollider.Owner.Pos + new Vector2(MathF.Cos(_angle), MathF.Sin(_angle)) * _distFromPlayer;


            return --_ticksUntilDeletion < 0;
        }
        public override bool OnCollision(Entity other)
        {
            return false;
        }

        protected override bool HitPlayer(Player other)
        {
            const int hitCooldown = Game._updatesPerSecond * 2 / 3; // can hit every .66... sec
            if (_hitPlayers.ContainsKey(other))
            {
                if (_hitPlayers[other] > other.GetGame().GameTick - hitCooldown)
                    return false;
                _hitPlayers[other] = other.GetGame().GameTick;
            }
            else
                _hitPlayers.Add(other, other.GetGame().GameTick);
            other.TakeDamage(Damage, MyCollider.Owner);
            other.ApplyKnockback((other.MyCollider.Pos - MyCollider.PreviousPos), Knockback);
            return false;
        }
    }
}
