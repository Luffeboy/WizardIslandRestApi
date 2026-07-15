using WizardIslandRestApi.Game.Spells.ExtraEntities;
namespace WizardIslandRestApi.Game.Spells.Ultimates
{
    public class FireAtWill : Spell
    {
        public override string Name { get { return "Fire at will"; } }
        public FireAtWill(Player player) : base(player)
        {
            Type = SpellType.Ultimate;
            StandardStats.Damage = 5;
            StandardStats.Knockback = 1.3f;
            StandardStats.Size = 0.5f;
            StandardStats.Speed = 2f;
            StandardStats.OtherStatsInt.Add(SpellSpecificStats.ProjectileEmitterCount, 1);
            StandardStats.OtherStatsInt.Add(SpellSpecificStats.ShotsUntilDepletion, 50);
            StandardStats.OtherStatsFloat.Add(SpellSpecificStats.CircleRadius, 15f);

            Tags.Add(SpellTags.Projectile);
            Tags.Add(SpellTags.Zone);
        }

        public override int CooldownMax { get; protected set; } = 18 * Game._updatesPerSecond;

        public override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            int emitters = StandardStats.OtherStatsInt[SpellSpecificStats.ProjectileEmitterCount];
            float startAngle = (float)(new Random().NextDouble() * Math.PI * 2);
            for (int i = 0; i < emitters; i++)
                GetCurrentGame().Entities.Add(new FireAtWillEntity(MyPlayer, GetCurrentGame(), mousePos)
                {
                    Damage = StandardStats.Damage,
                    Knockback = StandardStats.Knockback,
                    ShotsBeforeDeleing = StandardStats.OtherStatsInt[SpellSpecificStats.ShotsUntilDepletion],
                    TicksBeforeShootingMax = (int)((float)Game._updatesPerSecond / 15f),
                    Size = StandardStats.Size,
                    EmittedProjectileSpeed = StandardStats.Speed,
                    CirclingRadius = StandardStats.OtherStatsFloat[SpellSpecificStats.CircleRadius],
                    CurrentOrbitAngle = startAngle + (MathF.PI * 2 / emitters * i),
                });
            GoOnCooldown();
        }
    }

    public class FireAtWillEntity : Entity
    {
        private int _ticksBeforeShooting;
        private int _ticksBeforeShootingMax;
        private Game _game;
        Vector2 _target;
        public float CurrentOrbitAngle { get; set; }
        public float CirclingRadius { get; set; }
        public int TicksBeforeShootingMax { get { return _ticksBeforeShootingMax; } set { _ticksBeforeShootingMax = value; _ticksBeforeShooting = value; } }
        public int ShotsBeforeDeleing { get; set; }
        public float Damage { get; set; }
        public float Knockback { get; set; }
        public float Speed { get; set; } = MathF.PI * 2 / Game._updatesPerSecond;
        public float EmittedProjectileSpeed { get; set; } = 1;
        public FireAtWillEntity(Player owner, Game game, Vector2 startPos) : base(owner, startPos)
        {
            // random angle at first
            _target = startPos;
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
            CurrentOrbitAngle += Speed;
            Pos = _target + new Vector2(MathF.Cos(CurrentOrbitAngle), MathF.Sin(CurrentOrbitAngle)) * CirclingRadius;
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
                    Speed = EmittedProjectileSpeed,
                    TicksUntilDeletion = (int)(CirclingRadius / EmittedProjectileSpeed * Game._updatesPerSecond),
                    Dir = dir,
                    IgnoreHitOnOwnerOnSpawn = false,
                    CantHitSameTypeOfEntityFromSamePlayer = false,
                });
                return --ShotsBeforeDeleing < 0;
            }
            return false;

        }
    }
}
