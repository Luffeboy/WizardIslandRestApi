using WizardIslandRestApi.Game.Spells.ExtraEntities;

namespace WizardIslandRestApi.Game.Spells.BasicSpells.Utility
{
    public class AnchorCast : Spell
    {
        public override int CooldownMax { get; protected set; } = (int)(10.0f * Game._updatesPerSecond);
        public override string Name => "Anchor cast";

        public AnchorCast(Player player) : base(player)
        {
        }


        protected override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            var anchorEntity = new ShadowEntity()
            {
                Color = "0, 0, 100",
                Size = .75f,
                TicksUntilDeletion = 9999 * Game._updatesPerSecond,
                Pos = startPos,
                VisableTo = MyPlayer.Id,
            };
            GetCurrentGame().Entities.Add(anchorEntity);
            MyPlayer.OverridesAndObservers.OnSpellCastOverride = (spell, startPos, endPos) =>
            {
                spell.CastSpell(anchorEntity.Pos, endPos);
                anchorEntity.TicksUntilDeletion = 1;
            };
            GoOnCooldown();
        }
    }
}
