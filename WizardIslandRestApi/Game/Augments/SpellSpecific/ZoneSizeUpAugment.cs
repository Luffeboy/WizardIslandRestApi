using WizardIslandRestApi.Game.Spells;

namespace WizardIslandRestApi.Game.Augments.SpellSpecific
{
    public class ZoneSizeUpAugment : AugmentBase
    {
        public float SizeMultiplier { get; set; } = 2f;
        public ZoneSizeUpAugment()
        {
            AugmentName = "Zoning Spell";
            AugmentDescription = $"Increase the size of spells with the \"{SpellTags.Zone}\" tag, by {(SizeMultiplier - 1f) * 100}%.";
            RequiredOneOfTags.Add(SpellTags.Zone);
            CanBeStacked = false;
        }
        public override void AugmentSpell(Spell spell)
        {
            spell.StandardStats.Size *= SizeMultiplier;
        }
    }
    
}
