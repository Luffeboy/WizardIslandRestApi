namespace WizardIslandRestApi.Game.Spells.Ultimates
{
    public class SpeedBlits : MultiUseSpell
    {
        public SpeedBlits(Player player) : base(player)
        {
            UsesMax = 5;
            CooldownBetweenUses = 3;
        }

        public override int CooldownMax { get; protected set; } = 10 * Game._updatesPerSecond;

        public override void OnUse(Vector2 startPos, Vector2 mousePos)
        {
            MyPlayer.TeleportTo(mousePos);
        }
    }
}
