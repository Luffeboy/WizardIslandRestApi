using WizardIslandRestApi.Game.Spells;

namespace WizardIslandRestApi.Game.Augments.SpellSpecific
{
    public class ProjectileQuantityPlus : AugmentBase
    {
        public ProjectileQuantityPlus()
        {
            AugmentName = "Projectile Quantity +";
            AugmentDescription = "Increases the number of projectiles shot by 2";
            RequiredOneOfStats.Add(SpellSpecificStats.ProjectileQuantity);
        }
        public override void AugmentSpell(Spell spell)
        {
            if (spell.StandardStats.OtherStatsInt.ContainsKey(SpellSpecificStats.ProjectileQuantity))
                spell.StandardStats.OtherStatsInt[SpellSpecificStats.ProjectileQuantity] += 2;
#if DEBUG
            else Console.WriteLine($"Warning: Spell {spell.Name} does not have the stat {SpellSpecificStats.ProjectileQuantity}, but it tried to augment it");
#endif
        }
    }
    
}
