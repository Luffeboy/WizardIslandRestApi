using WizardIslandRestApi.Game.Spells.Debuffs;

namespace WizardIslandRestApi.Game.Spells.Movement
{
    public class Sprint : Spell
    {
        private float _speedMultiplier = 4.0f;
        public override SpellType Type { get; set; } = SpellType.Movement;
        public override int CooldownMax { get; protected set; } = 8 * Game._updatesPerSecond;
        public int Duration { get; protected set; } = 4 * Game._updatesPerSecond;
        public Sprint(Player player) : base(player)
        {
        }
        public override void OnCast(Vector2 mousePos)
        {
            MyPlayer.ApplyDebuff(new SpeedBuff(MyPlayer) { SpeedMultiplier = _speedMultiplier, TicksTillRemoval = Duration });
            GoOnCooldown();
        }
    }
}
