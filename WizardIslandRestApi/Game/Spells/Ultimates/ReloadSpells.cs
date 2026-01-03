namespace WizardIslandRestApi.Game.Spells.Ultimates
{
    public class ReloadSpells : Spell
    {
        public override string Name => "Reloaded";
        public ReloadSpells(Player player) : base(player)
        {
            Type = SpellType.Ultimate;
        }
        public override int CooldownMax { get; protected set; } = 999 * Game._updatesPerSecond;
        public override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            var spells = MyPlayer.GetSpells();
            var cooldownsSummed = 0;
            foreach (var spell in spells)
            {
                if (spell.CanCast || spell.Type == SpellType.Ultimate)
                    continue;
                cooldownsSummed += spell.CurrentCooldown - GetCurrentGameTick();
                spell.CurrentCooldown = -999;
            }
            CooldownMax = Math.Max(cooldownsSummed, Game._updatesPerSecond); // min 1 sec cooldown
            GoOnCooldown();
        }
    }
}
