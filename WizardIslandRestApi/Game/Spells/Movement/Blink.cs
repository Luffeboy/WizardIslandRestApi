namespace WizardIslandRestApi.Game.Spells.Movement
{
    public class Blink : Spell
    {
        private float _damage = 5;
        private float _knockback = 1.5f;
        private float _range = 50.0f;
        public override int CooldownMax { get; protected set; } = 8 * Game._updatesPerSecond;
        public Blink(Player player) : base(player)
        {
        }
        public override void OnCast(Vector2 mousePos)
        {
            var dir = (mousePos - MyPlayer.Pos);
            if (dir.LengthSqr() > _range * _range)
            {
                dir = dir.Normalized() * _range;
            }
            MyPlayer.Pos += dir;
            MyPlayer.TargetPos = mousePos;
            
            GoOnCooldown();
        }
    }
}
