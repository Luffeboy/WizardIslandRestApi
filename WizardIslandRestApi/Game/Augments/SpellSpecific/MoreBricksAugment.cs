using WizardIslandRestApi.Game.Spells;
using WizardIslandRestApi.Game.Spells.Debuffs;

namespace WizardIslandRestApi.Game.Augments.SpellSpecific
{
    public class MoreBricksAugment : AugmentBase
    {
        public int BrickQuantity { get; set; } = 1;

        public MoreBricksAugment()
        {
            AugmentName = "More Bricks";
            AugmentDescription = $"Increase the amount of bricks you spawn with, by {BrickQuantity}.";
            RequiredOneOfStats.Add(SpellSpecificStats.BricksToApplyOnRespawn);
        }

        public override void AugmentSpell(Spell spell)
        {
            spell.StandardStats.OtherStatsInt[SpellSpecificStats.BricksToApplyOnRespawn] += BrickQuantity;
            if (spell.MyPlayer != null && !spell.MyPlayer.IsDead)
                for (int i = 0; i < BrickQuantity; i++)
                    spell.MyPlayer.ApplyDebuff(new BrickBuff(spell.MyPlayer));
        }
    }
    
}
