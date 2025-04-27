using WizardIslandRestApi.Game.Spells.Movement;
using WizardIslandRestApi.Game.Spells.Ultimates;

namespace WizardIslandRestApi.Game.Spells
{
    public enum SpellType
    {
        Attack,
        Movement,
        Ultimate,
    }
    public abstract class Spell
    {
        public virtual string Name { get { return GetType().Name; } }
        public abstract int CooldownMax { get; protected set; }
        public int CurrentCooldown { get; set; }
        public bool CanCast { get { return CurrentCooldown < GetCurrentGameTick(); } }
        public Player MyPlayer { get; private set; }
        public virtual SpellType Type { get; set; } = SpellType.Attack;
        public virtual bool CanBeReplaced { get; protected set; } = true; // set this to false, if it could "dangerous" to replace this spell currently
        // static stuff
        private static Func<Player, Spell>[] _availableSpells = new Func<Player, Spell>[]
        {
            (player) => new FireBall(player),
            (player) => new FireBurst(player),
            (player) => new CrescentMoon(player),
            (player) => new HomingBolt(player),
            (player) => new Meteor(player),
            (player) => new BlackHole(player),
            (player) => new FrostField(player),
            (player) => new CirclingSnake(player),
            (player) => new Barrel(player),
            (player) => new Parry(player),

            (player) => new Blink(player),
            (player) => new BullCharge(player),
            (player) => new Sprint(player),
            (player) => new Swap(player),

            (player) => new Luna(player),
            (player) => new Stella(player),
            (player) => new BloodWorm(player),
            (player) => new FireAtWill(player),
            (player) => new SpeedBlits(player),
            (player) => new CopySpell(player),
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
        public abstract void OnCast(Vector2 startPos, Vector2 mousePos);
        public void GoOnCooldown()
        {
#if DEBUG
#else
            CurrentCooldown = GetCurrentGameTick() + (int)(CooldownMax * GetCurrentGame().GameModifiers.CooldownMultiplier * MyPlayer.Stats.CooldownMultiplier);
#endif
        }
        public override string ToString()
        {
            return Name;
        }
        public virtual void FullReset()
        {
            CurrentCooldown = -9999;
        }
    }
}
