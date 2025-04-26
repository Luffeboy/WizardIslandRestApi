namespace WizardIslandRestApi.Game.Spells.Ultimates
{
    public class BloodWorm : Spell
    {
        public BloodWorm(Player player) : base(player)
        {
        }

        public override int CooldownMax { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }

        public override void OnCast(Vector2 pos, Vector2 mousePos)
        {
            throw new NotImplementedException();
        }
    }
}
