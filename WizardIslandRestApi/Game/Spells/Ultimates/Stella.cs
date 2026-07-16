using WizardIslandRestApi.Game.Spells.BasicSpells;
using WizardIslandRestApi.Game.Spells.ExtraEntities;
namespace WizardIslandRestApi.Game.Spells.Ultimates
{
    public class Stella : Spell
    {
        private int _waitUntillPoolActivates = (int)(.1f * Game._updatesPerSecond);
        private float _sizeMultiplier = .75f;
        public override int CooldownMax { get; protected set; } = 15 * Game._updatesPerSecond;
        public Stella(Player player) : base(player)
        {
            Type = SpellType.Ultimate;
            StandardStats.Range = 50;
            StandardStats.Size = 15 / _sizeMultiplier;
            StandardStats.SummonLifetime = (int)(20 * Game._updatesPerSecond);

            Tags.Add(SpellTags.Zone);
        }


        public override void OnCast(Vector2 pos, Vector2 mousePos)
        {
            var dir = (mousePos - pos);
            float size = StandardStats.Size * _sizeMultiplier;
            if (dir.LengthSqr() > StandardStats.Range * StandardStats.Range)
            {
                dir = dir.Normalized() * StandardStats.Range;
            }
            var target = pos + dir;

            GetCurrentGame().Entities.Insert(0, new ShadowEntity() // we insert it at index 0, so everything else will be rendered on top of it
            {
                Size = size,
                TicksUntilDeletion = (StandardStats.SummonLifetime - _waitUntillPoolActivates) / 2 + _waitUntillPoolActivates,
                Pos = target,
            });

            GetCurrentGame().ScheduleAction(_waitUntillPoolActivates, () =>
            {
                GetCurrentGame().Entities.Add(new LavaPoolEntity(MyPlayer, target)
                {
                    MinSize = 0,
                    MaxSize = size,
                    TicksUntilDeletion = (StandardStats.SummonLifetime - _waitUntillPoolActivates),
                });
            }
            );

            GoOnCooldown();
        }
    }
}
