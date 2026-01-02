namespace WizardIslandRestApi.Game.Spells.Ultimates
{
    public class Klepto : Spell
    {
        List<Spell> _stolenSpells = [];

        public override string Name => _stolenSpells.Count == 0 ? 
                                       base.Name : 
                                       base.Name + " Stolen:\n" + string.Join("\n", _stolenSpells.Select(x =>x.Name));
        public Klepto(Player player) : base(player)
        {
            Type = SpellType.Ultimate;
        }

        public override int CooldownMax { get; protected set; } = 20 * Game._updatesPerSecond;

        public override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            if (_stolenSpells.Any())
            {
                // activate all stolen spells
                for (int i = 0; i < _stolenSpells.Count; i++)
                {
                    var spell = _stolenSpells[i];
                    if (spell.CanCast)
                        spell.OnCast(startPos, mousePos);
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
                int ultsToSteal = 1;
                int normalsToSteal = 1;
                var spells = targetPlayer.GetSpells();
                Dictionary<SpellType, List<int>> spellsByType = new();
                // spells that connot be stolen
                List<System.Type> nonSealableSpells = [typeof(CopySpell), typeof(Klepto)]; // I don't think I can get these two to work properly
                foreach (var spell in spells)
                {
                    if (nonSealableSpells.Contains(spell.GetType()))
                    {
                        normalsToSteal++; // since you can't steal their ult, you get an extra normal spell. (this is asuming they are following the rules, and only using 1 ult :) )
                        continue;
                    }
                    if (spellsByType.ContainsKey(spell.Type))
                        spellsByType[spell.Type].Add(spell.SpellIndex);
                    else
                        spellsByType.Add(spell.Type, new() { spell.SpellIndex });
                }
                // Steal one ultimate and one normal spells
                for (int i = 0; i < ultsToSteal; i++)
                {
                    if (spellsByType.ContainsKey(SpellType.Ultimate) && spellsByType[SpellType.Ultimate].Any())
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
                    if (spellsByType.ContainsKey(SpellType.Attack) && spellsByType[SpellType.Attack].Any())
                    {
                        var normalsList = spellsByType[SpellType.Attack];
                        var ultsListIndex = new Random().Next(normalsList.Count);
                        var spellIndex = normalsList[ultsListIndex];
                        normalsList.RemoveAt(ultsListIndex);

                        var stolenSpell = Spell.GetSpell(MyPlayer, spellIndex);
                        stolenSpell.FullReset();
                        _stolenSpells.Add(stolenSpell);
                    }
                }
                // Visual effect for stealing - maybe later
                // Schedule a check to see if the spells are used up
                void CheckStolenSpellsAreOnCooldown()
                {
                    for (int i = 0; i < _stolenSpells.Count; i++)
                        if (!_stolenSpells[i].CanCast)
                            _stolenSpells.RemoveAt(i--);
                    
                    if (_stolenSpells.Any())
                        GetCurrentGame().ScheduleAction(10, CheckStolenSpellsAreOnCooldown);
                    else 
                        GoOnCooldown();
                }
                CheckStolenSpellsAreOnCooldown();
            }
        }

        public override void FullReset()
        {
            _stolenSpells = [];
            base.FullReset();
        }
    }
}
