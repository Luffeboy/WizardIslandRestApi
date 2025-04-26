namespace WizardIslandRestApi.Game.Spells.Ultimates
{
    public class FireAtWill : Spell
    {
        public override string Name { get { return "Fire at will"; } }
        public FireAtWill(Player player) : base(player)
        {
            Type = SpellType.Ultimate;
        }

        public override int CooldownMax { get; protected set; } = 20 * Game._updatesPerSecond;

        public override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            GetCurrentGame().Entities.Add(new FireAtWillEntity(MyPlayer, GetCurrentGame(), mousePos)
            {
                Damage = 5,
                Knockback = 1.3f,
                ShotsBeforeDeleing = 50,
                TicksBeforeShootingMax = (int)((float)5 / Game._updatesPerSecond),
                Size = .5f,
            });
            GoOnCooldown();
        }
    }

    public class FireAtWillEntity : Entity
    {
        private int _ticksBeforeShooting;
        private int _ticksBeforeShootingMax;
        private float _currentAngle;
        private Game _game;
        Vector2 _target;
        public float CirclingRadius { get; set; } = 15.0f;
        public int TicksBeforeShootingMax { get { return _ticksBeforeShootingMax; } set { _ticksBeforeShootingMax = value; _ticksBeforeShooting = value; } }
        public int ShotsBeforeDeleing { get; set; }
        public float Damage { get; set; }
        public float Knockback { get; set; }
        public float Speed { get; set; } = MathF.PI * 2 / Game._updatesPerSecond;
        public FireAtWillEntity(Player owner, Game game, Vector2 startPos) : base(owner, startPos)
        {
            // random angle at first
            _target = startPos;
            _currentAngle = (float)(new Random().NextDouble() * Math.PI * 2);
            _game = game;
            Color = "255, 0, 0";
            EntityId = "FireAtWill";
        }
        public override bool OnCollision(Entity other)
        {
            return false;
        }
        public override bool OnCollision(Player other)
        {
            other.TakeDamage(1, MyCollider.Owner);
            return false;
        }

        public override void ReTarget(Vector2 pos)
        {
            _target = pos;
        }

        public override bool Update()
        {
            // move
            _currentAngle += Speed;
            Pos = _target + new Vector2(MathF.Cos(_currentAngle), MathF.Sin(_currentAngle)) * CirclingRadius;
            // shoot, maybe
            if (_ticksBeforeShooting-- < 0)
            {
                Vector2 dir = (_target - Pos) / CirclingRadius;
                float size = .3f;
                _ticksBeforeShooting = _ticksBeforeShootingMax;
                _game.Entities.Add(new SimpleSpellEntity(MyCollider.Owner, Pos + dir * (Size + size + .1f))
                {
                    Damage = Damage,
                    Knockback = Knockback,
                    Color = "255, 0, 0",
                    EntityId = "FireBall",
                    Size = size,
                    Speed = 4,
                    TicksUntilDeletion = 10,
                    Dir = dir,
                    TimeUntilCanHitOwner = 0,
                });
                return --ShotsBeforeDeleing < 0;
            }
            return false;

        }
    }
}
