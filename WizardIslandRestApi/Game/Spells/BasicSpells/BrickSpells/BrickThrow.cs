using WizardIslandRestApi.Game.Spells.SpellHelpers;

namespace WizardIslandRestApi.Game.Spells.BasicSpells.BrickSpells
{
    public class BrickThrow : BrickSpell
    {
        public override string Name => "Brick Throw";
        public BrickThrow(Player player) : base(player)
        {
            StandardStats.Damage = 5;
            StandardStats.Knockback = 1.5f;
            StandardStats.Speed = 3;

            ProjectileHelper.SetProjectileStats(this);

            Tags.Add(SpellTags.Projectile);
        }

        public override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            var dirs = ProjectileHelper.GetProjectileDirections(this, mousePos - startPos);
            ProjectileHelper.CastSpellWithBurst(this, startPos, (spawnPos, iteration) =>
            {
                for (int i = 0; i < dirs.Length; i++)
                    GetCurrentGame().Entities.Add(new BrickEntity(MyPlayer, CooldownMax, spawnPos, StandardStats.Speed)
                    {
                        Damage = StandardStats.Damage,
                        Knockback = StandardStats.Knockback,
                        Dir = dirs[i],
                        ShouldDropBrick = iteration == 0 && i == (dirs.Length / 2),
                    });
            });
            GoOnCooldownBrick();
        }
    }
}
