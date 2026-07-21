using WizardIslandRestApi.Game.Spells;
using WizardIslandRestApi.Game.Spells.BasicSpells.BrickSpells;

namespace WizardIslandRestApi.Game.Augments.SpellSpecific
{
    public class BrickCarePackageAugment : AugmentBase
    {
        public int BricksPerCarePackage { get; set; } = 1;
        public float SecondsBetweenCarePackages { get; set; } = 10f;

        public BrickCarePackageAugment()
        {
            AugmentName = "Brick care-package";
            AugmentDescription = $"Throws {BricksPerCarePackage} care-package(s) with a brick,\nevery {SecondsBetweenCarePackages} seconds.\nNote doesn't apply per spell.";
            RequiredOneOfStats.Add(SpellSpecificStats.BricksToApplyOnRespawn);
            CanBeStacked = false;
        }

        public override void AugmentPlayer(Player player, int spellsThatWereCouldBeAugmented)
        {
            var game = player.GetGame();
            if (game != null)
            {
                var helper = new BrickCarePackageAugmentHelper(game, player, BricksPerCarePackage, SecondsBetweenCarePackages);
                helper.Start();
            }
        }
    }
    public class BrickCarePackageAugmentHelper
    {
        private Game _game;
        private Player _player;
        int _bricksPerCarePackage;
        int _delayBetweenCarePackages;
        Random _random = new();
        public BrickCarePackageAugmentHelper(Game game, Player player, int bricksPerCarePackage, float secondsBetweenCarePackages)
        {
            _game = game;
            _player = player;
            _bricksPerCarePackage = bricksPerCarePackage;
            _delayBetweenCarePackages = (int)(Game._updatesPerSecond * secondsBetweenCarePackages);
        }

        public void Start()
        {

            _game.ScheduleAction(1, GiveCarePackage);
        }

        public void GiveCarePackage()
        {
            for (int i = 0; i < _bricksPerCarePackage; i++)
            {
                float angle = (float)_random.NextDouble() * MathF.PI * 2;
                Vector2 dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                _game.Entities.Add(new BrickEntity(_player, 30 * Game._updatesPerSecond, _game.GameMap.GroundMiddle, 3)
                {
                    Damage = 0,
                    Knockback = 0f,
                    Dir = dir,
                    ShouldDropBrick = true,
                    VisableTo = _player.Id,
                });
            }
            _game.ScheduleAction(_delayBetweenCarePackages, GiveCarePackage);
        }
    }
}
