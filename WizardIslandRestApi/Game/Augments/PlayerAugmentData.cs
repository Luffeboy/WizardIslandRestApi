namespace WizardIslandRestApi.Game.Augments
{
    public class PlayerAugmentData
    {
        public Player MyPlayer { get; set; }
        public List<AugmentBase> UsableAugments { get; } = [];
        public List<AugmentBase> AugmentHistory { get; } = [];

        public PlayerAugmentData(Player player)
        {
            MyPlayer = player;
            GetAllUseableAugments();
        }

        private void GetAllUseableAugments()
        {
            var playerSpells = MyPlayer.GetOriginalSpells();
            var augments = AugmentSystem.AllAugments.Where(aug => playerSpells.Any(spell => aug.CanAugmentSpell(spell)));
            if (augments.Any())
                UsableAugments.AddRange(augments);
        }
    }
}
