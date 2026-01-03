using WizardIslandRestApi.Game.Interfaces;

namespace WizardIslandRestApi.Game.Spells.Ultimates
{
    public class Ignis : Spell
    {
        private Spell[]? _previousSpells = null;
        private Spell[] _fireSpells;
        public Ignis(Player player) : base(player)
        {
            Type = SpellType.Ultimate;
            if (MyPlayer is null)
                return;
            _fireSpells = new Spell[GetCurrentGame().AllowedSpellCount];
            if (_fireSpells.Length == 0) // should not happen
                return;
            _fireSpells[0] = new BasicSpells.FireBurst(player);
            for (int i = 1; i < _fireSpells.Length - 1; i++)
                _fireSpells[i] = new BasicSpells.FireBall(player);
            _fireSpells[_fireSpells.Length - 1] = this;
            for (int i = 1; i < _fireSpells.Length - 1; i++)
                (_fireSpells[i] as ISetCooldownMax)?.SetCooldownMax((int)(2.5f * _fireSpells[i].CooldownMax));
        }

        public override int CooldownMax { get; protected set; } = 1 * Game._updatesPerSecond;

        public override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            if (_previousSpells is null) // first activation
            {
                _previousSpells = MyPlayer.GetSpells();
                MyPlayer.SetSpells(_fireSpells);
                GoOnCooldown();
            }
            else // second activation / go back to normal spells
            {
                MyPlayer.SetSpells(_previousSpells);
                _previousSpells = null;
                GoOnCooldown();
            }

        }
        public override void FullReset()
        {
            _previousSpells = null;
            base.FullReset();
        }
    }
}
