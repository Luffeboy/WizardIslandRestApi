using WizardIslandRestApi.Game.Spells;
using WizardIslandRestApi.Game.Spells.Ultimates;

namespace WizardIslandRestApi.Game.Augments.SpellSpecific
{
    public class GiveCooldownReductionOnCastAugment : AugmentBase
    {
        public float CooldownReduction { get; set; } = .5f;
        public int BuffDuration { get; set; } = 2 * Game._updatesPerSecond;

        public GiveCooldownReductionOnCastAugment()
        {
            AugmentName = "Addition reloading";
            AugmentDescription = $"Upon casting spell, gives a buff reducing all cooldowns by {MathF.Round((1f - CooldownReduction) * 100)}%.\nThe buff lasts {BuffDuration / Game._updatesPerSecond} seconds.";
            RequiredOneOfTags.Add(SpellTags.CanGiveCooldownReductionOnCast);
        }

        public override void AugmentSpell(Spell spell)
        {
            var reloadSpell = spell as IGiveCooldownReductionOnCast;
            if (reloadSpell != null)
                reloadSpell.Activate(CooldownReduction, BuffDuration);
        }
    }
    
}
