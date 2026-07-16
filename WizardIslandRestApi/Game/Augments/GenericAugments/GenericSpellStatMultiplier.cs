using WizardIslandRestApi.Game.Spells;

namespace WizardIslandRestApi.Game.Augments.GenericAugments
{
    public class GenericSpellStatMultiplier : AugmentBase
    {
        private readonly string _statName;
        private readonly float _statMultiplier;

        public GenericSpellStatMultiplier(string statName, float statMultiplier)
        {
            AugmentName = $"Spell Stat Up: {statName}";
            AugmentDescription = $"Increases the spell stat {statName} by {MathF.Round((statMultiplier-1f) * 100)}%.";
            RequiredOneOfStandardStats.Add(statName);
            _statName = statName;
            _statMultiplier = statMultiplier;
        }

        public override void AugmentSpell(Spell spell)
        {
            var t = spell.StandardStats.GetType();
            var propInfo = t.GetProperty(_statName);
            if (propInfo?.PropertyType == typeof(float))
            {
                var currentValue = (float)propInfo.GetValue(spell.StandardStats);
                propInfo.SetValue(spell.StandardStats, currentValue * _statMultiplier);
            }
            else if (propInfo?.PropertyType == typeof(int))
            {
                var currentValue = (float)propInfo.GetValue(spell.StandardStats);
                propInfo.SetValue(spell.StandardStats, MathF.Round(currentValue * _statMultiplier));
            }
#if DEBUG
                else Console.WriteLine($"Tried to augment spell {spell}, stat: {_statName}, but the property was null ({propInfo}) or not of type float or int ({propInfo?.PropertyType})");
#endif
        }
    }
    public class GenericSpecificSpellStatIncreaseAugment : AugmentBase
    {
        private readonly string _statName;
        private readonly int _statIncreaseAmount;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="statName"></param>
        /// <param name="statIncreaseAmount"></param>
        /// <param name="augmentName"></param>
        /// <param name="description">It will say "Increase <discription> by <statIncreaseAmount>"</param>
        public GenericSpecificSpellStatIncreaseAugment(string statName, int statIncreaseAmount, string augmentName, string description)
        {
            AugmentName = augmentName;
            AugmentDescription = $"Increase {description} by {statIncreaseAmount}.";
            RequiredOneOfStats.Add(statName);
            _statName = statName;
            _statIncreaseAmount = statIncreaseAmount;
        }

        public override void AugmentSpell(Spell spell)
        {
            spell.StandardStats.OtherStatsInt[_statName] += _statIncreaseAmount;
        }
    }
}
