using WizardIslandRestApi.Game.Spells;

namespace WizardIslandRestApi.Game.Augments.SpellSpecific
{
    public class ProjectileBurstPlus : AugmentBase
    {
        public ProjectileBurstPlus()
        {
            AugmentName = "Projectile Burst +";
            AugmentDescription = "Increases the burst count of projectiles shot by 1";
            RequiredOneOfStats.Add(SpellSpecificStats.ProjectileBurst);
        }
        public override void AugmentSpell(Spell spell)
        {
            if (spell.StandardStats.OtherStatsInt.ContainsKey(SpellSpecificStats.ProjectileBurst))
                spell.StandardStats.OtherStatsInt[SpellSpecificStats.ProjectileBurst] += 1;
#if DEBUG
            else Console.WriteLine($"Warning: Spell {spell.Name} does not have the stat {SpellSpecificStats.ProjectileBurst}, but it tried to augment it");
#endif
        }
    }
    
}
