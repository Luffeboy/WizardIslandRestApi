using WizardIslandRestApi.Game.Spells;

namespace WizardIslandRestApi.Game.Augments
{
    public abstract class AugmentBase
    {
        /// <summary>
        /// The name of tags, where at least one of them must be present on the spell for this augment to be valid.
        /// if this list is empty, then no tags are required.
        /// </summary>
        public List<string> RequiredOneOfTags { get; } = new List<string>();

        /// <summary>
        /// The name of stat, where at least one of them must be present on the spell for this augment to be valid.
        /// if this list is empty, then no stats are required.
        /// </summary>
        public List<string> RequiredOneOfStats { get; } = new List<string>();

        /// <summary>
        /// The name of stat, where at least one of them must be present on the spell for this augment to be valid.
        /// if this list is empty, then no stats are required.
        /// </summary>
        public List<string> RequiredOneOfStandardStats { get; } = new List<string>();

        public string AugmentName { get; set; } = "This augment doesn't have a name...";
        public string AugmentDescription { get; set; } = "This augment doesn't have a description...";

        public bool CanAugmentSpell(Spell spell)
        {
            if (RequiredOneOfStandardStats.Count > 0)
            {
                var allPropertiesOnSpellAboveZero =
                spell.StandardStats.GetType().GetProperties().Where(p =>
                {
                    if (p.PropertyType == typeof(int))
                        return (int)p.GetValue(spell.StandardStats) >= 0;
                    if (p.PropertyType == typeof(float))
                        return (float)p.GetValue(spell.StandardStats) >= 0f;
                    return false;
                }).Select(p => p.Name).ToList();

                if (RequiredOneOfStandardStats.Any(requiredStat => !allPropertiesOnSpellAboveZero.Contains(requiredStat)))
                    return false;
            }
            if (RequiredOneOfStats.Count > 0)
            {
                var spellStatNames = spell.StandardStats.OtherStatNames();
                if (!RequiredOneOfStats.Any(statName => spellStatNames.Contains(statName)))
                    return false;
            }
            return RequiredOneOfTags.Count == 0 || spell.Tags.Any(tag => RequiredOneOfTags.Contains(tag));
        }

        public void AttemptAugmentSpell(Spell spell)
        {
            if (CanAugmentSpell(spell))
                AugmentSpell(spell);
        }

        public virtual void AugmentSpell(Spell spell) { }
        public virtual void AugmentPlayer(Player player) { }
    }
}
