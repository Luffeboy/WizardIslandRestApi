namespace WizardIslandRestApi.Game.Spells
{
    public abstract class MultiUseSpell : Spell
    {
        protected int _lastUseTick = -9999;

        public int Uses 
        { 
            get 
            {
                return Math.Min((GetCurrentGameTick() - _lastUseTick) / CooldownMax, UsesMax);
            } 
        }
        public int UsesMax {  get; set; }
        public int CooldownBetweenUses { get; set; } = 0;
        public MultiUseSpell(Player player) : base(player)
        {
        }

        public override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            int cooldown = (int)(CooldownMax * GetCurrentGame().GameModifiers.CooldownMultiplier * MyPlayer.Stats.CooldownMultiplier);
            _lastUseTick += cooldown;
            int highestPosibleCooldown = GetCurrentGameTick() - (UsesMax - 1) * cooldown;
            if (_lastUseTick < highestPosibleCooldown) 
                _lastUseTick = highestPosibleCooldown;
            if (Uses > 0)
                CurrentCooldown = GetCurrentGameTick() + CooldownBetweenUses;
            else
                CurrentCooldown = _lastUseTick + cooldown;

            OnUse(startPos, mousePos);
        }

        public abstract void OnUse(Vector2 startPos, Vector2 mousePos);

        public override string ToString()
        {
            return $"{Name} ({Uses})";
        }
    }
}
