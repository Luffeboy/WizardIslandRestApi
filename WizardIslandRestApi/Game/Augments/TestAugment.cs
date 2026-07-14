#if DEBUG && false
using WizardIslandRestApi.Game.Spells;

namespace WizardIslandRestApi.Game.Augments
{
    public class TestAugment : AugmentBase
    {
        public TestAugment()
        {
            AugmentName = "Test augment";
            AugmentDescription = "Omega spell size up";
        }
        public override void AugmentSpell(Spell spell)
        {
            spell.StandardStats.Size *= 10f;
        }
    }
}
#endif
