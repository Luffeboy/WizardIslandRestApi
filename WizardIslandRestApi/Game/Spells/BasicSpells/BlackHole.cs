using System.Reflection.Metadata.Ecma335;

namespace WizardIslandRestApi.Game.Spells.BasicSpells
{
    public class BlackHole : Spell
    {
        public override string Name { get { return "Black hole"; } }
        public override int CooldownMax { get; protected set; } = (int)(10.0f * Game._updatesPerSecond);
        public BlackHole(Player player) : base(player)
        {
            StandardStats.Size = 1.5f;
            StandardStats.Speed = 1.0f;
            StandardStats.Range = 3;

            StandardStats.OtherStatsFloat.Add(SpellSpecificStats.GravityPull, 10.0f);

            Tags.Add(SpellTags.Projectile);
        }

        public override void OnCast(Vector2 pos, Vector2 mousePos)
        {
            Vector2 dir = (mousePos - pos).Normalized();
            GetCurrentGame().Entities.Add(new BlackHoleEntity(MyPlayer, GetCurrentGame(), pos + dir * (StandardStats.Size + MyPlayer.Size + 1.0f))
            {
                Color = "0, 0, 0",
                TicksUntilDeletion = StandardStats.GetLifetime(),
                Size = StandardStats.Size,
                Dir = dir,
                Speed = StandardStats.Speed,
                GravityPull = StandardStats.OtherStatsFloat[SpellSpecificStats.GravityPull],
            });
            GoOnCooldown();
        }
    }

    public class BlackHoleEntity : Entity
    {
        private Game _game;
        private int _TicksUntilDeletionMax;
        private int _TicksUntilDeletion;
        public int TicksUntilDeletion { get { return _TicksUntilDeletion; } set { _TicksUntilDeletion = value; _TicksUntilDeletionMax = value; } }
        public Vector2 Dir {  get; set; }
        public float Speed {  get; set; }
        public float GravityPull { get; set; } = 10.0f;
        public BlackHoleEntity(Player owner, Game game, Vector2 startPos) : base(owner, startPos)
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
            float playerKnockBackMultiplier = .35f;
            foreach (Player p in _game.Players.Values)
            {
                Vector2 dir = Pos - p.Pos;
                float amount = dir.LengthSqr();
                if (amount < minAmount)
                    amount = minAmount;
                if (p == MyCollider.Owner)
                    amount *= 2;
                p.ApplyKnockback(dir.Normalized(), GravityPull / amount * playerKnockBackMultiplier);
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
                e.Pos += dir * (GravityPull / amount) / e.Density;
            }
            Pos += Dir * Speed;
            _TicksUntilDeletion--;
            return _TicksUntilDeletion < 0;
        }
    }
}
