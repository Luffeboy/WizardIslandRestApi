using WizardIslandRestApi.Game.Spells.BasicSpells.BrickSpells;
using WizardIslandRestApi.Game.Spells.Debuffs;

namespace WizardIslandRestApi.Game.Spells.Movement
{
    public class BrickBridge : BrickSpell
    {
        public override SpellType Type { get; set; } = SpellType.Movement;
        public override int CooldownMax { get; protected set; } = (int)(7 * Game._updatesPerSecond);
        public override string Name => "Brick Bridge";
        public BrickBridge(Player player) : base(player)
        {
            BricksToApplyOnRespawn = 4;
        }
        public override void OnCast(Vector2 pos, Vector2 mousePos)
        {
            int brickCount = BrickCount;
            if (brickCount > BricksToApplyOnRespawn)
                brickCount = BricksToApplyOnRespawn;
            float bridgePartSize = 5.0f;
            int bridgePartDuration = 10 * Game._updatesPerSecond;

            GetCurrentGame().Entities.Add(new BrickBridgeEntity(MyPlayer, pos + (mousePos - pos).Normalized() * bridgePartSize * .25f, bridgePartSize)
            {
                MoreBridgesToCreate = brickCount - 1,
                TicksUntilDeletion = bridgePartDuration,
            });
            GoOnCooldownBrick(brickCount);
        }
    }
    public class BrickBridgeEntity : EntityWithDuration
    {
        private bool _isUnderPlayer = true;
        private float _startSize;
        public float MinSize { get; set; }
        public int MoreBridgesToCreate { get; set; }
        public BrickBridgeEntity(Player owner, Vector2 startPos, float startSize) : base(owner, startPos)
        {
            EntityId = "BrickBridge";
            _startSize = startSize;
            Size = startSize;
            MinSize = Size * .2f;
            Color = BrickBuff.BrickColor;
            Height = EntityHeight.Ground;
        }

        public override bool OnCollision(Entity other)
        {
            return false;
        }

        public override bool OnCollision(Player other)
        {
            if (other == MyCollider.Owner)
            {
                other.ApplyDebuff(new ImmuneToLave(other, 5));
                _isUnderPlayer = true;
            }
            return false;
        }

        public override void ReTarget(Vector2 pos)
        {}

        public override bool Update()
        {
            if (!_isUnderPlayer && MoreBridgesToCreate > 0)
            {
                var newBridePos = Pos + (MyCollider.Owner.Pos - Pos).Normalized() * (Size + _startSize * .8f);
                var game = MyCollider.Owner._game;
                game.Entities.Add(new BrickBridgeEntity(MyCollider.Owner, newBridePos, _startSize)
                {
                    MoreBridgesToCreate = MoreBridgesToCreate - 1,
                    TicksUntilDeletion = _ticksUntilDeletionMax,
                });
                MoreBridgesToCreate = -1;
            }
            _isUnderPlayer = false;
            Size = (_startSize - MinSize) * ((float)TicksUntilDeletion / (float)_ticksUntilDeletionMax) + MinSize;
            if (base.Update())
            {
                MyCollider.Owner._game.ScheduleAction(Game._updatesPerSecond, () => 
                    MyCollider.Owner.ApplyDebuff(new BrickBuff(MyCollider.Owner)));
                return true;
            }
            return false;
        }
    }
}
