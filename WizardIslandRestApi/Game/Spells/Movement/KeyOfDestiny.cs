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
            MyPlayer.TeleportTo(_teleportToLocation.Pos);
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
        public KeyOfDestinyEntity(Player owner, Game game, Vector2? startPos = null) : base(owner, startPos)
        {
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
            Vector2 pos = new Vector2();
            var playerCount = _game.Players.Count;
            for (int i = 0; i < playerCount; i++)
                pos += _game.Players[i].Pos;
            pos /= playerCount;
            // check if we are in the lave

            Map map = _game.GameMap;
            float distanceToMapCenterSqr = (map.GroundMiddle - pos).LengthSqr();
            // in middle
            if (distanceToMapCenterSqr < map.CircleInnerRadius * map.CircleInnerRadius)
                pos = map.GroundMiddle + (pos - map.GroundMiddle).Normalized() * map.CircleInnerRadius;
            // outside
            else if (distanceToMapCenterSqr > map.CircleRadius * map.CircleRadius)
                pos = map.GroundMiddle + (pos - map.GroundMiddle).Normalized() * (map.CircleRadius-.1f);
            var dir = (pos - Pos).Normalized();
            Pos += dir * Speed;
            if (dir.Dot(pos - Pos) < 0)
                Pos = pos;
            return ShouldBeDestroyed;
        }

        // these should not be called.
        public override bool OnCollision(Player other)
        { return false; }
        public override bool OnCollision(Entity other)
        { return false; }
    }
}
