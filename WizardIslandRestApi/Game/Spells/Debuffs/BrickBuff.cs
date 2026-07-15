namespace WizardIslandRestApi.Game.Spells.Debuffs
{
    public class BrickBuff : DebuffBase
    {
        public const string BrickName = "Brick";
        public const string BrickColor = "188,74,60";


        public BrickBuff(Player player) : base(player)
        {
        }

        public override void OnApply()
        {
            Stacks++;
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
