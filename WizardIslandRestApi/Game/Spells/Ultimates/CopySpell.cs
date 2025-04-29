namespace WizardIslandRestApi.Game.Spells.Ultimates
{
    public class CopySpell : Spell
    {
        private int _lastUsedSpellIndex = -1;
        private int _justUsedSpellIndex = -1;
        private Spell? _spell = null;
        public CopySpell(Player player) : base(player)
        {
            Type = SpellType.Ultimate;
            if (player != null)
                player.OverridesAndObservers.OnSpellCast += Observe;
        }

        public override int CooldownMax { get; protected set; } = 8 * Game._updatesPerSecond;
        public override string Name => "Copy";
        public override bool CanBeReplaced { get { return _spell == null || _spell.CanBeReplaced; } protected set { } }

        public override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            // we already cast the copy, reuse the old spell
            if (_spell != null)
            {
                _spell.OnCast(startPos, mousePos);
                if (!_spell.CanCast)
                    CopyGoOnCooldown();
                return;
            }

            var spells = MyPlayer.GetSpells();
            if (_lastUsedSpellIndex < 0 || _lastUsedSpellIndex >= spells.Length || spells[_lastUsedSpellIndex] is CopySpell)
                return;
            _spell = Spell.GetSpell(MyPlayer, spells[_lastUsedSpellIndex].SpellIndex);
            _spell.OnCast(startPos, mousePos);
            if (!_spell.CanCast)
            {
                CopyGoOnCooldown();
                return;
            }
            // spell can be cast, again
            GetCurrentGame().Entities.Add(new WaitUntillSpellOnCooldownEntity(MyPlayer, _spell, () =>
            {
                Console.WriteLine("abc");
                CopyGoOnCooldown();
            }));
        }

        private void Observe(int spellIndex)
        {
            _lastUsedSpellIndex = _justUsedSpellIndex;
            _justUsedSpellIndex = spellIndex;
        }
        public override string ToString()
        {
            if (_spell != null)
                return "Copy of " + _spell.ToString();
                var spells = MyPlayer.GetSpells();
            if (_justUsedSpellIndex < 0 || _justUsedSpellIndex >= spells.Length || spells[_justUsedSpellIndex] is CopySpell)
                return "Copy...";
            return "copy: " + spells[_justUsedSpellIndex].ToString();
        }
        private void CopyGoOnCooldown()
        {
            GoOnCooldown();
            _spell = null;
        }
    }

    public class WaitUntillSpellOnCooldownEntity : Entity
    {
        private Spell _spell;
        private Action _actionWhenSpellOnCooldown;
        public WaitUntillSpellOnCooldownEntity(Player owner, Spell spell, Action actionWhenSpellOnCooldown) : base(owner, null)
        {
            _spell = spell;
            _actionWhenSpellOnCooldown = actionWhenSpellOnCooldown;
        }

        public override bool OnCollision(Player other)
        {
            return false;
        }

        public override void ReTarget(Vector2 pos)
        {
        }

        public override bool Update()
        {
            if (!_spell.CanCast)
            {
                _actionWhenSpellOnCooldown();
                return true;
            }
            return false;
        }
    }
}
