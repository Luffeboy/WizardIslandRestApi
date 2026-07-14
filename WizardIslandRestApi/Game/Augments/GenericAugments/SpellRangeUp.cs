#if false
namespace WizardIslandRestApi.Game.Augments.GenericAugments
{
    public class SpellRangeUp : AugmentBase
    {
        public SpellRangeUp()
        {
            AugmentName = "Spell Range Up";
            AugmentDescription = "Increases the range of the spell by 25%";
        }
        public override void AugmentSpell(Spells.Spell spell)
        {
            spell.StandardStats.Range *= 1.25f;
        }
    }
}
#endif