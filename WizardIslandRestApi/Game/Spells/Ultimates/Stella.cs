namespace WizardIslandRestApi.Game.Spells.Ultimates
{
    public class Stella : Spell
    {
        private float _range = 50.0f;
        private float _lavaPoolSize = 15.0f;
        private int _waitUntillPoolActivates = (int)(.1f * Game._updatesPerSecond);
        private int _duration = (int)(20.0f * Game._updatesPerSecond); // the actual duration is this - _waitUntillPoolActivates
        public override int CooldownMax { get; protected set; } = 15 * Game._updatesPerSecond;
        public Stella(Player player) : base(player)
        {
            Type = SpellType.Ultimate;
        }


        public override void OnCast(Vector2 pos, Vector2 mousePos)
        {
            var dir = (mousePos - pos);
            if (dir.LengthSqr() > _range * _range)
            {
                dir = dir.Normalized() * _range;
            }
            var target = pos + dir;

            GetCurrentGame().Entities.Insert(0, new ShadowEntity() // we insert it at index 0, so everything else will be rendered on top of it
            {
                Size = _lavaPoolSize,
                TicksUntilDeletion = (_duration - _waitUntillPoolActivates) / 2 + _waitUntillPoolActivates,
                Pos = target,
            });

            GetCurrentGame().ScheduleAction(_waitUntillPoolActivates, () =>
            {
                GetCurrentGame().Entities.Add(new LavaPoolEntity(MyPlayer, target)
                {
                    MinSize = 0,
                    MaxSize = _lavaPoolSize,
                    TicksUntilDeletion = (_duration - _waitUntillPoolActivates),
                });
            }
            );

            GoOnCooldown();
        }
    }
}
