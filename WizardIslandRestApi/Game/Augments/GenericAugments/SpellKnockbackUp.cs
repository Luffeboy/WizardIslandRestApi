#if false
namespace WizardIslandRestApi.Game.Augments.GenericAugments
{
    public class SpellKnockbackUp : AugmentBase
    {
        public SpellKnockbackUp()
        {
            AugmentName = "Spell Knockback Up";
            AugmentDescription = "Increases the knockback of spells by 1.15x";
        }
        public override void AugmentSpell(Spells.Spell spell)
        {
            spell.StandardStats.Knockback *= 1.15f;
        }
    }
}
#endif