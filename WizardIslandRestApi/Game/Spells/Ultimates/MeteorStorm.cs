using WizardIslandRestApi.Game.Spells.BasicSpells;
using WizardIslandRestApi.Game.Spells.ExtraEntities;

namespace WizardIslandRestApi.Game.Spells.Ultimates
{
    public class MeteorStorm : Spell
    {
        Meteor _meteorSpell;
        public override string Name => "Meteor storm";

        public override int CooldownMax { get; protected set; } = (int)(50.0f * Game._updatesPerSecond);

        public MeteorStorm(Player player) : base(player)
        {
            Type = SpellType.Ultimate;
            StandardStats.Damage = 20;
            StandardStats.Knockback = 3;
            StandardStats.Size = 15f;
            StandardStats.Range = 20f;
            StandardStats.OtherStatsInt.Add(SpellSpecificStats.ActivationDelay, (int)(.5f * Game._updatesPerSecond));
            StandardStats.OtherStatsInt.Add(SpellSpecificStats.SummonQuantity, 1);
            StandardStats.OtherStatsInt.Add(SpellSpecificStats.ShotsUntilDepletion, 10);
            StandardStats.OtherStatsInt.Add(SpellSpecificStats.BurstDelay, (int)(1.0f * Game._updatesPerSecond));

            Tags.Add(SpellTags.Zone);
            Tags.Add(SpellTags.Static);
            if (MyPlayer != null)
                _meteorSpell = new Meteor(MyPlayer);
        }

        protected override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            Vector2 dir = mousePos - startPos;
            if (dir.LengthSqr() > StandardStats.Range * StandardStats.Range)
                dir = dir.Normalized() * StandardStats.Range;

            Vector2 stormCenter = startPos + dir;
            int burstDelay = StandardStats.OtherStatsInt[SpellSpecificStats.BurstDelay];
            Random random = new Random();
            CopyStatsToMeteorSpell();
            var game = GetCurrentGame();
            int shots = StandardStats.OtherStatsInt[SpellSpecificStats.ShotsUntilDepletion];
            game.Entities.Add(new ShadowEntity()
            {
                Color = "90, 30, 30",
                TicksUntilDeletion = (shots + 1) * burstDelay,
                Size = StandardStats.Size,
                Pos = stormCenter,

            });
            for (int i = 0; i < shots; i++)
            {
                game.ScheduleAction(i * burstDelay, ()=>
                {
                    // Random position within storm radius
                    float randomAngle = (float)(random.NextDouble() * MathF.PI * 2);
                    float randomDistance = (float)(random.NextDouble() * StandardStats.Size);
                    Vector2 meteorPos = stormCenter + new Vector2(MathF.Cos(randomAngle), MathF.Sin(randomAngle)) * randomDistance;
                    _meteorSpell.CastSpell(stormCenter, meteorPos);
                });
            }
        }

        private void CopyStatsToMeteorSpell()
        {
            _meteorSpell.StandardStats.Damage = StandardStats.Damage;
            _meteorSpell.StandardStats.Knockback = StandardStats.Knockback;
            _meteorSpell.StandardStats.Size = StandardStats.Size / 4;
            _meteorSpell.StandardStats.Range = StandardStats.Range;
            _meteorSpell.StandardStats.OtherStatsInt[SpellSpecificStats.ActivationDelay] = StandardStats.OtherStatsInt[SpellSpecificStats.ActivationDelay];
            _meteorSpell.StandardStats.OtherStatsInt[SpellSpecificStats.SummonQuantity] = StandardStats.OtherStatsInt[SpellSpecificStats.SummonQuantity];
        }
    }
}
