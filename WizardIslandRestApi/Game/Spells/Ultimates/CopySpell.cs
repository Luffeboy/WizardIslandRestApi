namespace WizardIslandRestApi.Game.Spells.Ultimates
{
    public class CopySpell : Spell
    {
        private int _lastUsedSpellIndex = -1;
        private int _justUsedSpellIndex = -1;
        public CopySpell(Player player) : base(player)
        {
            Type = SpellType.Ultimate;
            if (player != null)
                player.OverridesAndObservers.OnSpellCast += Observe;
        }

        public override int CooldownMax { get; protected set; } = 5 * Game._updatesPerSecond;
        public override string Name => "Name";

        public override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            var spells = MyPlayer.GetSpells();
            if (_lastUsedSpellIndex < 0 || _lastUsedSpellIndex >= spells.Length || spells[_lastUsedSpellIndex] is CopySpell)
                return;
            spells[_lastUsedSpellIndex].OnCast(startPos, mousePos);
            GoOnCooldown();
        }

        private void Observe(int spellIndex)
        {
            _lastUsedSpellIndex = _justUsedSpellIndex;
            _justUsedSpellIndex = spellIndex;
        }
        public override string ToString()
        {
            var spells = MyPlayer.GetSpells();
            if (_justUsedSpellIndex < 0 || _justUsedSpellIndex >= spells.Length || spells[_justUsedSpellIndex] is CopySpell)
                return "Copy...";
            return "copy: " + spells[_justUsedSpellIndex].ToString();
        }
    }
}
