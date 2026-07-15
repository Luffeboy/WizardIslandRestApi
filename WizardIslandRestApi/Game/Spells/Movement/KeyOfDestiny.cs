using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace WizardIslandRestApi.Game.Spells.Movement
{
    public interface ICanWander
    {
        public void StartWander();
    }
    public class KeyOfDestiny : Spell, ICanWander
    {
        private KeyOfDestinyEntity _teleportToLocation;
        public override string Name => "Key of Destiny";
        public override int CooldownMax { get; protected set; } = 8 * Game._updatesPerSecond;

        public KeyOfDestiny(Player player) : base(player)
        {
            Type = SpellType.Movement;

            Tags.Add(SpellTags.CanWander);
        }

        public override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            var dir = _teleportToLocation.Pos - startPos;
            MyPlayer.TeleportTo(_teleportToLocation.Pos);
            MyPlayer.Vel = new Vector2();
            MyPlayer.ApplyKnockback(dir.Normalized(), 1.25f);

            GoOnCooldown();
        }

        public override void OnPlayerReset()
        {
            base.OnPlayerReset();
            RemovedFromPlayer();

            _teleportToLocation = new KeyOfDestinyEntity(MyPlayer, GetCurrentGame(), MyPlayer.Pos);
            _teleportToLocation.Update();
            GetCurrentGame().Entities.Add(_teleportToLocation);
        }

        public override void RemovedFromPlayer()
        {
            base.RemovedFromPlayer();
            if (_teleportToLocation is not null)
                _teleportToLocation.ShouldBeDestroyed = true;
        }

        public void StartWander()
        {
#if DEBUG
            if (_teleportToLocation is null)
                Console.WriteLine("Error: _teleportToLocation is null");
#endif
            _teleportToLocation.IsWandering = true;
        }
    }
    public class KeyOfDestinyEntity : Entity
    {
        private Game _game;
        public bool ShouldBeDestroyed { get; set; } = false;
        public float Speed { get; set; } = .75f;
        private Player _player;

        public bool IsWandering { get; set; } = false;
        private Vector2 _wanderTarget = new Vector2();
        private Vector2 _wanderDir = new Vector2();
        private int _wanderTicksUntilTargetChange = -1;
        private int _wanderTicksUntilTargetChangeMax = 5 * Game._updatesPerSecond;

        public KeyOfDestinyEntity(Player owner, Game game, Vector2? startPos = null) : base(owner, startPos)
        {
            _player = owner;
            MyCollider = null;
            Height = EntityHeight.Ground;
            VisableTo = owner.Id;
            Size = 1.0f;
            _game = game;
        }

        public override void ReTarget(Vector2 pos)
        {}

        public override bool Update()
        {
            Map map = _game.GameMap;
            Vector2 pos = _player.Pos;
            Vector2 dir = new Vector2();
            if (IsWandering)
            {
                dir = _wanderTarget - Pos;
                if (_wanderDir.Dot(dir) < 0 || --_wanderTicksUntilTargetChange < 0)
                {
                    _wanderTicksUntilTargetChange = _wanderTicksUntilTargetChangeMax;
                    // new wander target
                    Random r = new();
                    float angle = (float)(r.NextDouble() * Math.PI * 2);
                    float distance = (float)(r.NextDouble() * (map.CircleRadius - map.CircleInnerRadius) + map.CircleInnerRadius);
                    _wanderTarget = map.GroundMiddle + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * distance;
                    _wanderDir = (_wanderTarget - Pos).Normalized();
                }
                dir = _wanderDir;
            }
            else
            {
                var playerCount = _game.Players.Count;
                for (int i = 0; i < playerCount; i++)
                    if (_game.Players[i] != _player && !_game.Players[i].IsDead)
                        dir += (_game.Players[i].Pos - _player.Pos) * .75f;
                pos += dir / ((playerCount == 1 ? 2 : playerCount) - 1);
                dir = (pos - Pos).Normalized();
            }

            Pos += dir * Speed;
            // check if we are in the lava
            float distanceToMapCenterSqr = (map.GroundMiddle - Pos).LengthSqr();
            // in middle
            if (distanceToMapCenterSqr < map.CircleInnerRadius * map.CircleInnerRadius)
                Pos = map.GroundMiddle + (pos - map.GroundMiddle).Normalized() * map.CircleInnerRadius;
            // outside
            else if (distanceToMapCenterSqr > map.CircleRadius * map.CircleRadius)
                Pos = map.GroundMiddle + (pos - map.GroundMiddle).Normalized() * (map.CircleRadius - .1f);
            if (dir.Dot(pos - Pos) < 0)
                Pos = pos;
            return ShouldBeDestroyed;
        }

        // these should not be called, as it doesn't have a collider.
        public override bool OnCollision(Player other)
        { return false; }
        public override bool OnCollision(Entity other)
        { return false; }
    }
}
