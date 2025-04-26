namespace WizardIslandRestApi.Game.Spells.Movement
{
    public class Blink : Spell
    {
        private float _range = 50.0f;
        public override SpellType Type { get; set; } = SpellType.Movement;
        public override int CooldownMax { get; protected set; } = (int)(10 * Game._updatesPerSecond);
        public Blink(Player player) : base(player)
        {
        }
        public override void OnCast(Vector2 pos, Vector2 mousePos)
        {
            var dir = (mousePos - pos);
            if (dir.LengthSqr() > _range * _range)
            {
                dir = dir.Normalized() * _range;
            }
            MyPlayer.TeleportTo(pos + dir);
            MyPlayer.Vel = new Vector2();
            MyPlayer.TargetPos = mousePos;
            
            GoOnCooldown();
        }
    }
}
