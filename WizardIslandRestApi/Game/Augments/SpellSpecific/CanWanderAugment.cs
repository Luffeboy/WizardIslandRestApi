using WizardIslandRestApi.Game.Spells;
using WizardIslandRestApi.Game.Spells.Movement;

namespace WizardIslandRestApi.Game.Augments.SpellSpecific
{
    public class CanWanderAugment : AugmentBase
    {
        public float CooldownReduction { get; set; } = .7f;

        public CanWanderAugment()
        {
            AugmentName = "Wandering Around";
            AugmentDescription = $"Makes Key Of Destiny wander around on its own.\nAlso reduces its cooldown by {MathF.Round((1f - CooldownReduction) * 100)}%.";
            RequiredOneOfTags.Add(SpellTags.CanWander);
        }

        public override void AugmentSpell(Spell spell)
        {
            var wanderSpell = spell as ICanWander;
            if (wanderSpell != null)
            {
                wanderSpell.StartWander();
                spell.StandardStats.CooldownMultiplier *= CooldownReduction;
            }
        }
    }
    
}
