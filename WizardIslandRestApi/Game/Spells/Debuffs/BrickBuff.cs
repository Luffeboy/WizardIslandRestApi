namespace WizardIslandRestApi.Game.Spells.Debuffs
{
    public class BrickBuff : DebuffBase
    {
        public const string BrickName = "Brick";

        public BrickBuff(Player player) : base(player)
        {
            Stackable = true;
        }

        public override void OnApply()
        {
        }

        public override void OnRemove()
        {
        }

        public override bool Update()
        {
            return false;
        }
        public override string ToString()
        {
            return BrickName;
        }
    }
}
