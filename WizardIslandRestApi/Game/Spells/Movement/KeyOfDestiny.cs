using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace WizardIslandRestApi.Game.Spells.Movement
{
    public class KeyOfDestiny : Spell
    {
        private KeyOfDestinyEntity _teleportToLocation;
        public override string Name => "Key of Destiny";
        public KeyOfDestiny(Player player) : base(player)
        {
            Type = SpellType.Movement;
        }

        public override int CooldownMax { get; protected set; } = 8 * Game._updatesPerSecond;

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
            if (_teleportToLocation != null) 
                _teleportToLocation.ShouldBeDestroyed = true;
            
            _teleportToLocation = new KeyOfDestinyEntity(MyPlayer, GetCurrentGame(), MyPlayer.Pos);
            GetCurrentGame().Entities.Add(_teleportToLocation);
        }
    }
    public class KeyOfDestinyEntity : Entity
    {
        private Game _game;
        public bool ShouldBeDestroyed { get; set; } = false;
        public float Speed { get; set; } = .75f;
        private Player _player;
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
            Vector2 pos = _player.Pos;
            Vector2 dir = new Vector2();
            var playerCount = _game.Players.Count;
            for (int i = 0; i < playerCount; i++)
                if (_game.Players[i] != _player && !_game.Players[i].IsDead)
                    dir += (_game.Players[i].Pos - _player.Pos) * .75f;
            pos += dir / ((playerCount == 1 ? 2 : playerCount) - 1);
            // check if we are in the lave

            Map map = _game.GameMap;
            dir = (pos - Pos).Normalized();
            Pos += dir * Speed;
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
