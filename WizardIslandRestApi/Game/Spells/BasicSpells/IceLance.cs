using WizardIslandRestApi.Game.Spells.Debuffs;
using WizardIslandRestApi.Game.Spells.ExtraEntities;
using WizardIslandRestApi.Game.Spells.SpellHelpers;

namespace WizardIslandRestApi.Game.Spells.BasicSpells
{
    public class IceLance : Spell
    {
        private List<IceLanceEntity> _entities = [];
        private int _usedSpellTick = -1;
        public override string Name => "Ice lance";
        public IceLance(Player player) : base(player)
        {
            StandardStats.Damage = 5;
            StandardStats.Knockback = 1.3f;
            StandardStats.Speed = 2f;
            StandardStats.Size = .35f;
            StandardStats.Range = .5f * StandardStats.Speed;

            Tags.Add(SpellTags.Projectile);
            Tags.Add(SpellTags.Debuff);

            StandardStats.OtherStatsInt.Add(SpellSpecificStats.ProjectileQuantity, 1);
            StandardStats.OtherStatsFloat.Add(SpellSpecificStats.ProjectileAngle, MathF.PI / 4f);
        }

        public override int CooldownMax { get; protected set; } = 12 * Game._updatesPerSecond;

        public override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            if (_entities.Count == 0)
            {
                _usedSpellTick = GetCurrentGameTick() + (int)(CooldownMax * GetCurrentGame().GameModifiers.CooldownMultiplier * MyPlayer.Stats.CooldownMultiplier);

                var dirs = ProjectileHelper.GetProjectileDirections(this, mousePos - startPos);
                for (int i = 0; i < dirs.Length; i++)
                {
                    var newEntity = new IceLanceEntity(MyPlayer, startPos, StandardStats.GetLifetime(), this)
                    {
                        Damage = StandardStats.Damage,
                        Knockback = StandardStats.Knockback,
                        Dir = dirs[i],
                        IceTarget = mousePos,
                        Speed = StandardStats.Speed,
                        Size = StandardStats.Size
                    };
                    _entities.Add(newEntity);
                    GetCurrentGame().Entities.Add(newEntity);
                }
            }
            else
            {
                foreach (var entity in _entities)
                    entity.IceTarget = mousePos;
            }
        }
        public override void FullReset()
        {
            while (_entities.Count > 0)
                EntityExpired(_entities[_entities.Count - 1]);
            base.FullReset();
        }
        public void EntityExpired(IceLanceEntity entity)
        {
            _entities.Remove(entity);
#if !NO_COOLDOWN
            CurrentCooldown = _usedSpellTick;
#endif
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

        public IceLanceEntity(Player owner, Vector2 startPos, int ticksUntillDeltion, IceLance spell) : base(owner, ticksUntillDeltion, startPos)
        {
            _spell = spell;
            EntityId = "IceLance";
            Color = "100, 100, 255";
        }
        public override bool OnCollision(Entity other)
        {
            if (base.OnCollision(other))
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

            bool ended = base.Update();

            if (ended)
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
            _spell.EntityExpired(this);
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
