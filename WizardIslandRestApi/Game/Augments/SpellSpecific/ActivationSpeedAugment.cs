using WizardIslandRestApi.Game.Spells;

namespace WizardIslandRestApi.Game.Augments.SpellSpecific
{
    public class ActivationSpeedAugment : AugmentBase
    {
        public float ActivationSpeedMultiplier { get; set; } = .8f;
        public ActivationSpeedAugment()
        {
            AugmentName = "Activation speed +";
            AugmentDescription = $"Increase the activation speed by {(1f - ActivationSpeedMultiplier) * 100}%.\nOf spells with a delay.";
            RequiredOneOfStats.Add(SpellSpecificStats.ActivationDelay);
        }
        public override void AugmentSpell(Spell spell)
        {
            var oldSpeed = (float)spell.StandardStats.OtherStatsInt[SpellSpecificStats.ActivationDelay];
            spell.StandardStats.OtherStatsInt[SpellSpecificStats.ActivationDelay] = Math.Max((int)(oldSpeed * ActivationSpeedMultiplier), 1);
        }
    }
    
}
