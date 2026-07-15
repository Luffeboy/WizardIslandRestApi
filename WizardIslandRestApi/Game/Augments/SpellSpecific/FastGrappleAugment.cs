using WizardIslandRestApi.Game.Spells;

namespace WizardIslandRestApi.Game.Augments.SpellSpecific
{
    public class FastGrappleAugment : AugmentBase
    {
        public float CooldownReduction { get; set; } = .5f;
        public float SpeedMultiplier { get; set; } = 4f;

        public FastGrappleAugment()
        {
            AugmentName = "Super fast grappler";
            AugmentDescription = $"A buff to the gappling hook.\nReduces its cooldown by {MathF.Round((1f - CooldownReduction) * 100)}%.\n Increases it's speed by {MathF.Round((SpeedMultiplier-1)*100)}%.";
            RequiredOneOfTags.Add(SpellTags.GrappleHook);
        }

        public override void AugmentSpell(Spell spell)
        {
            spell.StandardStats.Speed *= SpeedMultiplier;
            spell.StandardStats.CooldownMultiplier *= CooldownReduction;
        }
    }
    
}
