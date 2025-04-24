namespace WizardIslandRestApi.Game.Spells.Ultimates
{
    public class Stella : Spell
    {
        private float _range = 50.0f;
        private float _lavaPoolSize = 15.0f;
        private int _waitUntillPoolActivates = (int)(1.0f * Game._updatesPerSecond);
        private int _duration = (int)(10.0f * Game._updatesPerSecond); // the actual duration is this - _waitUntillPoolActivates
        public override int CooldownMax { get; protected set; } = 1;// 20 * Game._updatesPerSecond;
        public Stella(Player player) : base(player)
        {
            Type = SpellType.Ultimate;
        }


        public override void OnCast(Vector2 mousePos)
        {
            var dir = (mousePos - MyPlayer.Pos);
            if (dir.LengthSqr() > _range * _range)
            {
                dir = dir.Normalized() * _range;
            }
            var target = MyPlayer.Pos + dir;

            GetCurrentGame().Entities.Insert(0, new ShadowEntity() // we insert it at index 0, so everything else will be rendered on top of it
            {
                Size = _lavaPoolSize,
                TicksUntilDeletion = (_duration - _waitUntillPoolActivates) / 2 + _waitUntillPoolActivates,
                Pos = target,
            });

            GetCurrentGame().Entities.Add(new WaitToDoSomethingEntity(_waitUntillPoolActivates, () =>
            {
                GetCurrentGame().Entities.Add(new LavaPoolEntity(MyPlayer)
                {
                    MinSize = 0,
                    MaxSize = _lavaPoolSize,
                    TicksUntilDeletion = (_duration - _waitUntillPoolActivates),
                    Pos = target,
                });
            }
            ));

            GoOnCooldown();
        }
    }
}
