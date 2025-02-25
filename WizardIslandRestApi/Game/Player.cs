namespace WizardIslandRestApi.Game
{
    class PlayerStats
    {
        public float Speed { get; set; } = 1f / Game._updatesPerSecond;
        public float MaxSpeed { get { return Speed * Game._updatesPerSecond; } }
        public int Health { get; set; }
        public int MaxHealth { get; set; } = 100;

    }
    class PlayerScoreStats
    {
        public int Kills { get; set; }
        public int Deaths { get; set; }

    }
    public class Player
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public string Name { get; set; }
        public string Password { get; }
        //public int Health { get; set; }
        //public int MaxHealth { get; set; }

        //public float Speed { get; set; } = 1f / Game._updatesPerSecond;
        //public float MaxSpeed { get { return Speed * Game._updatesPerSecond; } }
        public float CanStopSpeed { get { return Stats.MaxSpeed * 1.1f; } }
        public Vector2 TargetPos { get; set; }
        public Vector2 Pos { get; set; }
        public Vector2 Vel {  get; set; }
        public float Size { get; set; } = 1.0f;

        PlayerStats Stats { get; set; } = new PlayerStats();
        PlayerScoreStats ScoreStats { get; set; } = new PlayerScoreStats();
        public Player? LastHitByPlayer { get; set; } = null; // the player that last hit this player
        public Player(int id, int gameId)
        {
            Id = id;
            GameId = gameId;
            Password = "";
            Random random = new Random();
            for (int i = 0; i < 10; i++)
                Password += random.Next(10);
            Reset();
        }

        public void Reset()
        {
            Stats.Health = Stats.MaxHealth;
            Vel = new Vector2();
            //get random spawn on the map
            Random r = new Random();
            float angle = (float)(r.NextDouble() * Math.PI * 2);
            var map = GetMap();
            float distance = (float)(r.NextDouble() * (map.CircleRadius - map.CircleInnerRadius));
            Pos = map.GroundMiddle + new Vector2(MathF.Cos(angle) * distance, MathF.Sin(angle) * distance);
        }
#pragma warning disable
        private Map GetMap()
        {
            return GameManager.Instance.GetGame(GameId).GameMap;
        }
#pragma warning enable

        public void Update()
        {
            // update velocity
            Vector2 dir = TargetPos - Pos;
            dir.Normalize();
            // if we move along our current velocity, we might need to move a bit to the right or left, to correctly hit the target position
            Vector2 posPlusVelocityDir = TargetPos - (Pos + Vel * 10);
            posPlusVelocityDir.Normalize();
            dir = dir + (posPlusVelocityDir * 10);
            dir.Normalize();
            if (Vel.Normalized().Dot(dir) < .5f || Vel.LengthSqr() < Stats.MaxSpeed * Stats.MaxSpeed)
                Vel += dir * Stats.Speed;

            // update position
            Pos += Vel;
            Vector2 dirAfter = TargetPos - Pos;
            if ((TargetPos - Pos).LengthSqr() < Size && Vel.LengthSqr() < CanStopSpeed * CanStopSpeed)
            {
                Vel = new Vector2(0, 0);
                Pos = TargetPos;
            }
        }
    }
    public class PlayerMinimum
    {
        public int Id { get; set; }
        public Vector2 Pos { get; set; }
        public float Size { get; set; }
        public PlayerMinimum(Player player) 
        {
            Id = player.Id;
            Pos = player.Pos;
            Size = player.Size;
        }

        public static IEnumerable<PlayerMinimum> Copy(IEnumerable<Player> players)
        {
            List<PlayerMinimum> list = new List<PlayerMinimum>(players.Count());
            foreach (var player in players)
            {
                list.Add(new PlayerMinimum(player));
            }
            return list;
        }
    }
}
