using WizardIslandRestApi.Game.Spells;

namespace WizardIslandRestApi.Game.Augments.SpellSpecific
{
    public class CopyOneMoreSpellAugment : AugmentBase
    {
        public CopyOneMoreSpellAugment()
        {
            AugmentName = "Another one";
            AugmentDescription = "Yoinky sploinky one more (basic) spell.";
            RequiredOneOfStats.Add(SpellSpecificStats.BasicSpellsToCopy);
        }
        public override void AugmentSpell(Spell spell)
        {
            spell.StandardStats.OtherStatsInt[SpellSpecificStats.BasicSpellsToCopy] += 1;
        }
    }
    
}
