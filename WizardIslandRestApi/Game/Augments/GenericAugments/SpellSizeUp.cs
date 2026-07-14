#if false
using WizardIslandRestApi.Game.Spells;

namespace WizardIslandRestApi.Game.Augments.GenericAugments
{
    public class SpellSizeUp : AugmentBase
    {
        public SpellSizeUp()
        {
            AugmentName = "Spell Size +";
            AugmentDescription = "Increases the size of the spell by 25%";
        }
        public virtual void AugmentSpell(Spell spell)
        {
            spell.StandardStats.Size *= 1.25f;
        }
    }
}
#endif