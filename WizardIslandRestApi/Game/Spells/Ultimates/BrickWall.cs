using WizardIslandRestApi.Game.Physics;
using WizardIslandRestApi.Game.Spells.BasicSpells.BrickSpells;
using WizardIslandRestApi.Game.Spells.Debuffs;

namespace WizardIslandRestApi.Game.Spells.Ultimates
{
    public class BrickWall : BrickSpell
    {
        private float _distToWall = 3;

        private float _damage = 5;
        private float _knockback = 1.15f;
        private int _wallEntitiesThatGivesABrickRemaining = 0;
        private List<BrickWallEntity> _wallEntities = [];
        public override int CooldownMax { get; protected set; } = (int)(25 * Game._updatesPerSecond);
        public override string Name => _wallEntities.Count == 0 ? "Brick Wall" : "Shoot Brick";
        public BrickWall(Player player) : base(player)
        {
            Type = SpellType.Ultimate;
            BricksToApplyOnRespawn = 5;
            MinBricksToCast = 4;
        }


        public override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            if (_wallEntities.Count > 0)
            {
                // shoot a random brick from the wall
                var index = new Random().Next(_wallEntities.Count);
                var wallPart = _wallEntities[index];
                _wallEntities.RemoveAt(index);
                wallPart.ShouldBeRemoved = true;
                bool shouldDropBrick = _wallEntitiesThatGivesABrickRemaining-- > 0;
                var wallPos = wallPart.Pos;
                var brickSpeed = 4;
                GetCurrentGame().Entities.Add(new BrickEntity(MyPlayer, CooldownMax, wallPos, brickSpeed)
                {
                    Dir = (mousePos - wallPos).Normalized(),
                    ShouldDropBrick = shouldDropBrick,
                    Damage = _damage,
                    Knockback = _knockback,
                    EntityNamesToIgnore = ["Brick", "BrickWall"]
                });
                if (_wallEntities.Count == 0)
                    GoOnCooldown();
                return;
            }
            int bricksToRemove = BrickCount;
            _wallEntitiesThatGivesABrickRemaining = bricksToRemove;
            if (_wallEntitiesThatGivesABrickRemaining < MinBricksToCast) // if you cast this spell via cooldown, rather than bricks
                _wallEntitiesThatGivesABrickRemaining = MinBricksToCast;
            int brickWallParts = _wallEntitiesThatGivesABrickRemaining * 2;
            _wallEntitiesThatGivesABrickRemaining--;// you loose 1 brick when casting this spell :)
            Vector2 spellDir = (mousePos - startPos).Normalized();
            Vector2 spellDirNormal = spellDir.Normal();
            float brickWallPartSize = 1.0f;

            for (int i = 0; i < brickWallParts; i++)
            {
                Vector2 brickPos = startPos + 
                    spellDir * _distToWall +
                    spellDirNormal * brickWallPartSize * i -
                    spellDirNormal * brickWallPartSize * brickWallParts * .5f;
                var wallPart = new BrickWallEntity(MyPlayer, brickPos)
                {
                    Size = brickWallPartSize,
                };
                GetCurrentGame().Entities.Add(wallPart);
                _wallEntities.Add(wallPart);
            }

            RemoveBrickBuffs(bricksToRemove);
        }
    }

    public class BrickWallEntity : Entity
    {
        public bool ShouldBeRemoved = false;
        public BrickWallEntity(Player owner, Vector2 startPos) : base(owner, startPos)
        {
            EntityId = "BrickWall";
            Color = BrickBuff.BrickColor;
        }
        public override bool OnCollision(Entity other)
        {
            if (other.EntityId == "BrickWall")
                return false;
            return false;
        }
        public override bool OnCollision(Player other)
        {
            return false;
        }
        public override void ReTarget(Vector2 pos)
        {
        }
        public override bool Update()
        {
            return ShouldBeRemoved;
        }
    }
}
