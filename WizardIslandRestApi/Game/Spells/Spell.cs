namespace WizardIslandRestApi.Game.Spells
{
    public abstract class Spell
    {
        public virtual string Name { get { return GetType().Name; } }
        protected abstract int CooldownMax { get; set; }
        protected int CurrentCooldown { get; set; }
        public bool CanCast { get { return CurrentCooldown < GetCurrentGameTick(); } }
        public Player MyPlayer { get; private set; }
        // static stuff
        private static Func<Player, Spell>[] _availableSpells = new Func<Player, Spell>[]
        {
            (player) => new FireBall(player),
        };
        public static Spell GetSpell(Player player, int index)
        {
            return _availableSpells[index](player);
        }
        public static Spell[] GetSpells()
        {
            Spell[] spells = new Spell[_availableSpells.Length];
            for (int i = 0; i < spells.Length; i++)
            {
                spells[i] = _availableSpells[i](null);
            }
            return spells;
        }
        public Spell(Player player)
        {
            MyPlayer = player;
        }
        protected Game GetCurrentGame() { return MyPlayer.GetGame(); }
        protected int GetCurrentGameTick() { return GetCurrentGame().GameTick; }
        //protected int GetCurrentGameTick() { return MyPlayer != null ? GetCurrentGame().GameTick : 0; }
        public abstract void OnCast(Vector2 mousePos);
        public void GoOnCooldown()
        {
            CurrentCooldown = GetCurrentGameTick() + CooldownMax;
        }
    }
}
