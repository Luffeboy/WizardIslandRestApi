#if false
using WizardIslandRestApi.Game.Spells;
using WizardIslandRestApi.Game.Spells.Movement;

namespace WizardIslandRestApi.Game.Augments.SpellSpecific
{
    public class EmitterPlusAugment : AugmentBase
    {
        public int emitterIncreaseCount { get; set; } = 1;

        public EmitterPlusAugment()
        {
            AugmentName = "Fire away... more";
            AugmentDescription = $"Increases emitters by {emitterIncreaseCount}.";
            RequiredOneOfStats.Add(SpellSpecificStats.ProjectileEmitterCount);
        }

        public override void AugmentSpell(Spell spell)
        {
            spell.StandardStats.OtherStatsInt[SpellSpecificStats.ProjectileEmitterCount] += emitterIncreaseCount;
        }
    }
    
}
#endif
