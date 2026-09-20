namespace WizardIslandRestApi.Game.Spells.Ultimates
{
    public class Klepto : Spell
    {
        List<Spell> _stolenSpells = [];

        public override string Name => _stolenSpells.Count == 0 ? 
                                       base.Name : 
                                       base.Name + " Looted:\n" + string.Join("\n", _stolenSpells.Select(x =>x.Name));
        public Klepto(Player player) : base(player)
        {
            Type = SpellType.Ultimate;
            StandardStats.OtherStatsInt.Add(SpellSpecificStats.UltimateSpellsToCopy, 1);
            StandardStats.OtherStatsInt.Add(SpellSpecificStats.BasicSpellsToCopy, 1);
            Tags.Add(SpellTags.UseOtherSpell);
        }

        public override int CooldownMax { get; protected set; } = 20 * Game._updatesPerSecond;

        protected override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            if (_stolenSpells.Any())
            {
                // activate all stolen spells
                for (int i = _stolenSpells.Count - 1; i >= 0; i--)
                {
                    var spell = _stolenSpells[i];
                    if (spell.CanCast)
                        spell.CastSpell(startPos, mousePos);
                }
                return;
            }
            // steal spells :D :D :D :D :D
            // find closest player to mousePos
            Player? targetPlayer = null;
            {
                float lowestDist = float.MaxValue;
                foreach (var player in GetCurrentGame().Players.Values)
                {
                    if (player.Id == MyPlayer.Id || player.IsDead)
                        continue;
                    var dist = (player.Pos - mousePos).LengthSqr();
                    if (dist < lowestDist)
                    {
                        targetPlayer = player;
                        lowestDist = dist;
                    }
                }
            }
            if (targetPlayer is not null)
            {
                // steal their spells
                // amount of spells to steal
                int ultsToSteal = StandardStats.OtherStatsInt[SpellSpecificStats.UltimateSpellsToCopy];
                int normalsToSteal = StandardStats.OtherStatsInt[SpellSpecificStats.BasicSpellsToCopy];
                var spells = targetPlayer.GetOriginalSpells();
                Dictionary<SpellType, List<int>> spellsByType = new();
                foreach (var spellType in Enum.GetValues<SpellType>())
                    spellsByType.Add(spellType, []);

                // spells that connot be stolen
                List<System.Type> nonSealableSpells = [typeof(CopySpell), typeof(Klepto)]; // I don't think I can get these two to work properly
                foreach (var spell in spells)
                {
                    if (nonSealableSpells.Contains(spell.GetType()))
                        continue;
                    spellsByType[spell.Type].Add(spell.SpellIndex);
                }

                // if you can't steal their ult, you get an extra normal spell.
                if (spellsByType[SpellType.Ultimate].Count == 0)
                    normalsToSteal++; 

                // Steal one ultimate and one normal spells
                for (int i = 0; i < ultsToSteal; i++)
                {
                    if (spellsByType[SpellType.Ultimate].Any())
                    {
                        var ultsList = spellsByType[SpellType.Ultimate];
                        var ultsListIndex = new Random().Next(ultsList.Count);
                        var spellIndex = ultsList[ultsListIndex];
                        ultsList.RemoveAt(ultsListIndex);

                        var stolenSpell = Spell.GetSpell(MyPlayer, spellIndex);
                        stolenSpell.FullReset();
                        _stolenSpells.Add(stolenSpell);
                    }
                }
                for (int i = 0; i < normalsToSteal; i++)
                {
                    if (spellsByType[SpellType.Attack].Any())
                    {
                        var normalsList = spellsByType[SpellType.Attack];
                        var ultsListIndex = new Random().Next(normalsList.Count);
                        var spellIndex = normalsList[ultsListIndex];
                        normalsList.RemoveAt(ultsListIndex);

                        var stolenSpell = Spell.GetSpell(MyPlayer, spellIndex);
                        stolenSpell.FullReset();
                        stolenSpell.OnPlayerReset();
                        _stolenSpells.Add(stolenSpell);
                    }
                }

                // apply augments
                GetCurrentGame().GameAugmentSystem.ReApplyAllAugmentToPlayersSpellsOnly(MyPlayer, _stolenSpells);

                // Visual effect for stealing - maybe later

                // Check to see if the spells are used up
                for (int i = 0; i < _stolenSpells.Count; i++)
                    _stolenSpells[i].Observers.WentOnCooldown += RemoveFromStolenSpellsObserver;
            }
        }

        private void RemoveFromStolenSpellsObserver(object? sender, EventArgs e)
        {
            Spell? spell = sender as Spell;
            for (int i = 0; i < _stolenSpells.Count; i++)
                if (_stolenSpells[i] == spell)
                {
                    _stolenSpells[i].RemovedFromPlayer();
                    _stolenSpells.RemoveAt(i);
                }
            if (!_stolenSpells.Any())
                GoOnCooldown();
        }

        public override void FullReset()
        {
            _stolenSpells = [];
            base.FullReset();
        }
    }
}
