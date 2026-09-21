using System.Globalization;
using WizardIslandRestApi.Game.Spells.ExtraEntities;

namespace WizardIslandRestApi.Game.Spells.BasicSpells.TrapSeplls
{
    public class ExplosionMine : Spell
    {
        private Meteor _onTriggerSpell;

        public override int CooldownMax { get; protected set; } = 19 * Game._updatesPerSecond;

        public override string Name => "Explosion mine";
        
        public ExplosionMine(Player player) : base(player)
        {
            _onTriggerSpell = new Meteor(player);
            SetStandardStats(_onTriggerSpell.StandardStats);
            StandardStats.Knockback = 2.3f;
            StandardStats.Range = 0;
            StandardStats.OtherStatsInt[SpellSpecificStats.ActivationDelay] /= 2;

            Tags.Add(SpellTags.Static);
            Tags.Add(SpellTags.Zone);
        }

        protected override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            AddEntityToGame(new MineEntity(MyPlayer, startPos, (mine) =>
            {
                _onTriggerSpell.CastSpell(mine.Pos, mine.Pos);
            })
            {
                Size = StandardStats.Size / 4,
                Color = "255,10,10",
                Density = 3,
                EntityId = "ExplosionMine",
            });

            GoOnCooldown();
        }
    }

    public class MineEntity : CantHitOwnerAtStartSpellEntity
    {
        private Action<MineEntity> _onTrigger;

        private int _ticksAlive = 0;

        public float TicksUntilInvisable { get; set; } = 2.5f * Game._updatesPerSecond;

        public float TimeUntilInvisableSeconds { set => TicksUntilInvisable = (int)(value * Game._updatesPerSecond); }

        public MineEntity(Player owner, Vector2 startPos, Action<MineEntity> onTrigger) : base(owner, 9999999, startPos)
        {
            EntityId = "Mine";
            _onTrigger = onTrigger;
        }

        protected override bool HitPlayer(Player other)
        {
            return true;
        }

        public override void ReTarget(Vector2 pos)
        {
            VisableTo = -1;
            _ticksAlive = 0;
        }

        public override bool Update()
        {
            _ticksAlive++;
            if (_ticksAlive < TicksUntilInvisable)
                Transparancy = (1f - _ticksAlive / TicksUntilInvisable).ToString("0.00", CultureInfo.InvariantCulture);
            else if (_ticksAlive == TicksUntilInvisable)
            {
                VisableTo = MyCollider.Owner!.Id;
                Transparancy = ".8";
            }

            return base.Update();
        }

        public override void OnExpire(EntityExpiredReason reason)
        {
            base.OnExpire(reason);
            _onTrigger.Invoke(this);
        }
    }
}
