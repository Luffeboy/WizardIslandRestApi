namespace WizardIslandRestApi.Game.Spells
{
    public class BrickThrow : BrickSpell
    {
        public override string Name => "Brick Throw";
        public BrickThrow(Player player) : base(player)
        {
        }

        public override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            GetCurrentGame().Entities.Add(new BrickEntity(MyPlayer, CooldownMax, startPos)
            {
                Damage = 5,
                Knockback = 1.5f,
                Dir = (mousePos - startPos).Normalized()
            });
            GoOnCooldownBrick();
        }
    }
}
