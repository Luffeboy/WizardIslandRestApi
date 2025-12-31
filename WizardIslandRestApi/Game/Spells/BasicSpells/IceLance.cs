using WizardIslandRestApi.Game.Spells.Debuffs;
using WizardIslandRestApi.Game.Spells.ExtraEntities;

namespace WizardIslandRestApi.Game.Spells.BasicSpells
{
    public class IceLance : Spell
    {
        private IceLanceEntity? _entity = null;
        private int _usedSpellTick = -1;
        public override string Name => "Ice lance";
        public IceLance(Player player) : base(player)
        {
            
        }

        public override int CooldownMax { get; protected set; } = 12 * Game._updatesPerSecond;

        public override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            int ticksUntillDeletion = (int)(.5f * Game._updatesPerSecond);
            if (_entity == null)
            {
                _usedSpellTick = GetCurrentGameTick() + (int)(CooldownMax * GetCurrentGame().GameModifiers.CooldownMultiplier * MyPlayer.Stats.CooldownMultiplier);
                GetCurrentGame().Entities.Add(_entity = new IceLanceEntity(MyPlayer, startPos, ticksUntillDeletion, this)
                {
                    Damage = 5,
                    Knockback = 1.3f,
                    Dir = (mousePos - startPos).Normalized(),
                    IceTarget = mousePos,
                    Speed = 2.0f,
                    Size = .35f
                });
            }
            else
            {
                _entity.IceTarget = mousePos;
            }
        }
        public override void FullReset()
        {
            EntityExpired();
            base.FullReset();
        }
        public void EntityExpired()
        {
            _entity = null;
            CurrentCooldown = _usedSpellTick;
        }
    }
    public class IceLanceEntity : CantHitOwnerAtStartSpellEntity
    {
        private IceLance _spell;
        public float Damage { get; set; }
        public float Knockback { get; set; }
        public Vector2 Dir { get; set; }
        public Vector2 IceTarget { get; set; }
        public float Speed { get; set; }
        public float SlowAmount { get; set; } = .3f;
        public int AmountOfIceSpaces { get; set; } = 10;
        public float SizeOfIce { get; set; } = 1.5f;
        public int IceTimeUntillDeletion { get; set; } = 5 * Game._updatesPerSecond;
        public override int TicksUntillCanHitOwner { get; set; } = 5;

        public IceLanceEntity(Player owner, Vector2 startPos, int ticksUntillDeltion, IceLance spell) : base(owner, ticksUntillDeltion, startPos)
        {
            _spell = spell;
            EntityId = "IceLance";
            Color = "100, 100, 255";
        }
        public override bool OnCollision(Entity other)
        {
            if ( base.OnCollision(other))
            {
                Die();
                return true; 
            }
            return false;
        }

        public override void ReTarget(Vector2 pos)
        {
            Dir = (pos - Pos).Normalized();
            _ticksUntilDeletion = _ticksUntilDeletionMax;
        }

        public override bool Update()
        {
            Pos += Dir * Speed;

            if (--_ticksUntilDeletion < 0)
            {
                Die();
                return true;
            }
            return false;
        }
        private void Die()
        {
            var pos = Pos;
            var dir = (IceTarget - Pos).Normalized();
            var distBetweenIce = .75f;
            float angle = HomingBoltEntity.GetAngleFromDirection(dir);
            for (int i = 0; i < AmountOfIceSpaces; i++)
            {
                _spell.GetCurrentGame().Entities.Add(new FrostFieldEntity(MyCollider.Owner, pos + dir * i * (SizeOfIce * distBetweenIce))
                {
                    ForwardAngle = angle,
                    Size = SizeOfIce,
                    TicksUntilDeletion = IceTimeUntillDeletion,
                    SlowAmount = SlowAmount,
                });
            }
            _spell.EntityExpired();
        }

        protected override bool HitPlayer(Player other)
        {
            Die();
            other.TakeDamage(Damage, MyCollider.Owner);
            other.ApplyKnockback((other.MyCollider.Pos - (Pos - Dir * Speed * 5)).Normalized(), Knockback);
            other.ApplyDebuff(new Slowed(other) { SpeedMultiplier = SlowAmount, TicksTillRemoval = 30 });
            return true;
        }
    }
}
