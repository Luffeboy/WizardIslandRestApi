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

            Tags.Add(SpellTags.Projectile);
        }

        public override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            GetCurrentGame().Entities.Add(new BrickEntity(MyPlayer, CooldownMax, startPos, StandardStats.Speed)
            {
                Damage = StandardStats.Damage,
                Knockback = StandardStats.Knockback,
                Dir = (mousePos - startPos).Normalized()
            });
            GoOnCooldownBrick();
        }
    }
}
