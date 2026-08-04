using WizardIslandRestApi.Game.Spells.SpellHelpers;

namespace WizardIslandRestApi.Game.Spells.BasicSpells.LuckSpells
{
    public class FlipACoint : Spell
    {
        public override int CooldownMax { get; protected set; } = (int)(15.0f * Game._updatesPerSecond);

        public override string Name => "Flip a coin";

        public FlipACoint(Player player) : base(player)
        {
            StandardStats.Damage = 5;
            StandardStats.Knockback = 2;
            StandardStats.Size = .75f;
            StandardStats.Speed = 3f;
            StandardStats.OtherStatsInt.Add(SpellSpecificStats.Luck, 1);

            ProjectileHelper.SetProjectileStats(this, quantity: 1, angle: MathF.PI / 8, burstCount: 1, burstDelay: Game._updatesPerSecond / 4);
            Tags.Add(SpellTags.Luck);
            Tags.Add(SpellTags.Projectile);
        }


        protected override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            var dirs = ProjectileHelper.GetProjectileDirections(this, mousePos - startPos);
            ProjectileHelper.CastSpellWithBurst(this, startPos, (spawnPos, iteration) =>
            {
                for (int i = 0; i < dirs.Length; i++)
                {
                    GetCurrentGame().Entities.Add(new CoinEntity(MyPlayer, spawnPos)
                    {
                        Damage = StandardStats.Damage,
                        Knockback = StandardStats.Knockback,
                        Size = StandardStats.Size,
                        SpeedMax = StandardStats.Speed,
                        Luck = StandardStats.OtherStatsInt[SpellSpecificStats.Luck],
                        Dir = dirs[i],
                        DistanceBetweenExplotions = 10,
                        OnExplodeMultiplier = 1.2f,
                    });
                }
            });
            GoOnCooldown();
        }
    }

    public class CoinEntity : Entity
    {
        private Player _player;
        private Random _random = new Random();
        private int _explotionCount = 0;
        private int _beginnerLuck = 5;

        public float Damage { get; set; }

        public float Knockback { get; set; }

        public float SpeedMax { get; set; }

        public int Luck { get; set; }

        public Vector2 Dir { get; set; }

        public float DistanceSinceLastExplotion { get; set; }

        public float DistanceBetweenExplotions { get; set; } = 1f;

        public float OnExplodeMultiplier { get; set; } = 1;

        public int MaxExplosions { get => 10 + Luck * 2; }

        public CoinEntity(Player owner, Vector2? startPos = null) : base(owner, startPos)
        {
            _player = owner;
            MyCollider = null;
            Color = "212,175,55";
        }

        public override bool Update()
        {
            float t = (DistanceSinceLastExplotion / DistanceBetweenExplotions) * 2;
            if (t > 1)
                t = 2 - t;
            if (t < 0)
                return Explode();

            float currentSpeed = MathF.Max(SpeedMax * t, .05f);
            Pos += Dir * currentSpeed;
            DistanceSinceLastExplotion += currentSpeed;
            return false;
        }

        /// <summary>
        /// returns true, if the coin should be removed from the game
        /// </summary>
        /// <returns></returns>
        private bool Explode()
        {
            float explodeThreshold = 1f / (1 + Luck + _beginnerLuck);
            _beginnerLuck = 0;
            float randomValue = _random.NextSingle();

            if (randomValue < explodeThreshold || ++_explotionCount > MaxExplosions)
                return true;

            _player.GetGame().Entities.Add(new MeteorEntity(_player, Pos, _player.GetGame())
            {
                Damage = Damage,
                KnockbackMin = Knockback * .8f,
                KnockbackMax = Knockback * 1.2f,
                FallTime = 1,
                Size = Size * 3,
            });


            Damage *= OnExplodeMultiplier;
            Knockback *= OnExplodeMultiplier;
            Size *= OnExplodeMultiplier;

            DistanceSinceLastExplotion -= DistanceBetweenExplotions;
            return false;
        }

        public override bool OnCollision(Player other)
        {
            return false;
        }

        public override void ReTarget(Vector2 pos)
        {}
    }
}
