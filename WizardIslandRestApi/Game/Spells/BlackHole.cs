using System.Reflection.Metadata.Ecma335;

namespace WizardIslandRestApi.Game.Spells
{
    public class BlackHole : Spell
    {
        public override string Name { get { return "Black hole"; } }
        public override int CooldownMax { get; protected set; } = (int)(8.0f * Game._updatesPerSecond);
        public BlackHole(Player player) : base(player)
        {
        }

        public override void OnCast(Vector2 mousePos)
        {
            Vector2 dir = (mousePos - MyPlayer.Pos).Normalized();
            float size = 1.5f;
            GetCurrentGame().Entities.Add(new BlackHoleEntity(MyPlayer, GetCurrentGame())
            {
                Pos = MyPlayer.Pos + dir * (size + MyPlayer.Size + 1.0f),
                Color = "0, 0, 0",
                TicksUntilDeletion = (int)(3.0f * Game._updatesPerSecond),
                Size = size,
                Dir = dir,
                Speed = 1.0f
            });
            GoOnCooldown();
        }
    }
    public class BlackHoleEntity : Entity
    {
        private const float _gravitationalConstant = 10.0f;
        private Game _game;
        private int _TicksUntilDeletionMax;
        private int _TicksUntilDeletion;
        public int TicksUntilDeletion { get { return _TicksUntilDeletion; } set { _TicksUntilDeletion = value; _TicksUntilDeletionMax = value; } }
        public Vector2 Dir {  get; set; }
        public float Speed {  get; set; }
        public BlackHoleEntity(Player owner, Game game) : base(owner)
        {
            _game = game;
            EntityId = "BlackHole";
        }
        public override void ReTarget(Vector2 pos)
        {
            _TicksUntilDeletion = _TicksUntilDeletionMax;
            Dir = (pos - Pos).Normalized();
        }

        public override bool OnCollision(Entity other)
        {
            return false;
        }

        public override bool OnCollision(Player other)
        {
            other.TakeDamage(0, MyCollider.Owner);
            return false;
        }

        public override bool Update()
        {
            // move all players and entities towards it self
            // F = (G * m1 * m2) / d^2
            float minAmount = 1.0f;
            foreach (Player p in _game.Players.Values)
            {
                Vector2 dir = Pos - p.Pos;
                float amount = dir.LengthSqr();
                if (amount < minAmount)
                    amount = minAmount;
                p.ApplyKnockback(dir.Normalized(), _gravitationalConstant / amount * .1f);
            }
            //
            foreach (Entity e in _game.Entities)
            {
                if (e == this || e.Height == EntityHeight.Ground)
                    continue;
                Vector2 dir = Pos - e.Pos;
                float amount = dir.LengthSqr();
                if (amount < minAmount)
                    amount = minAmount;
                e.Pos += dir * (_gravitationalConstant / amount);
            }
            Pos += Dir * Speed;
            _TicksUntilDeletion--;
            return _TicksUntilDeletion < 0;
        }
    }
}
