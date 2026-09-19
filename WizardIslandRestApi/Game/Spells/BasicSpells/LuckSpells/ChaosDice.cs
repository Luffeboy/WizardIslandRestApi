using WizardIslandRestApi.Game.Spells.ExtraEntities;
using WizardIslandRestApi.Game.Spells.SpellHelpers;

namespace WizardIslandRestApi.Game.Spells.BasicSpells.LuckSpells
{
    public class ChaosDice : Spell
    {

        public override int CooldownMax { get ; protected set; } = 20 * Game._updatesPerSecond;

        public override string Name => "Chaos dice";

        public ChaosDice(Player player) : base(player)
        {
            StandardStats.Damage = 5;
            StandardStats.Knockback = 1.1f;
            StandardStats.Size = 2f;
            StandardStats.Speed = 1f;

            StandardStats.OtherStatsInt.Add(SpellSpecificStats.Luck, 1);
        }

        protected override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            // spawn a new die
            var die = new ChaosDiceEntity(MyPlayer, StandardStats.OtherStatsInt[SpellSpecificStats.Luck], startPos)
            {
                Dir = (mousePos - startPos).Normalized(),
                Damage = StandardStats.Damage,
                Knockback = StandardStats.Knockback,
                Size = StandardStats.Size,
                FlyTime = (int)(1.5f * Game._updatesPerSecond),
                Speed = StandardStats.Speed,
            };
            GetCurrentGame().Entities.Add(die);
            GoOnCooldown();
        }
    }

    public class ChaosDiceEntity : Entity
    {
        private Random _random = new();
        private int _aliveTime = 0;
        private int _ticksBetweenRolls = (int)(0.5f * Game._updatesPerSecond);
        private int _highestAllowedValue;
        private int _ticksBetweenFlyAndActivate = (int)(0.25f * Game._updatesPerSecond);

        public List<ChaosDiceEntity> AdditionalDice { get; } = [];

        public int FlyTime { get; set; } = (int)(1.5f * Game._updatesPerSecond);

        public int DieValue { get; private set; } = 1;

        public Vector2 Dir { get; set; }

        public float Speed { get; set; } = 1.0f;

        public bool ShouldDelete { get; set; } = false;

        public float Damage { get; set; }

        public float Knockback { get; set; }

        public int Luck { get; set; }

        public ChaosDiceEntity(Player owner, int luck, Vector2? startPos = null, int highestAllowedValue = 6) : base(owner, startPos)
        {
            Height = EntityHeight.Ground;
            Size = 1.0f;
            Luck = luck;
            _highestAllowedValue = highestAllowedValue;
            RollDie();
            Color = "0,0,0";
        }

        public override bool OnCollision(Player other)
        {
            return false;
        }
        public override bool OnCollision(Entity other)
        {
            if (other is ChaosDiceEntity otherDie)
            {
                Vector2 dir = (Pos - other.Pos).Normalized();
                if (dir.LengthSqr() == 0)
                    dir.x = new Random().NextSingle() - .5f;
                float moveAmount = .1f;
                Pos += dir * moveAmount;
            }
            return false;
        }

        public override void ReTarget(Vector2 pos)
        {
            _aliveTime = 0;
            Dir = (pos - Pos).Normalized();
        }

        public override bool Update()
        {
            if (_aliveTime++ < FlyTime)
            {
                Pos += Dir * (Speed * (1.0f - (float)_aliveTime / (float)FlyTime));
                if (_aliveTime % _ticksBetweenRolls == 0)
                    RollDie();
            }
            if (CanActivate())
            {
                var nearestEnemy = MyCollider.Owner.GetGame().GetNearestPlayer(Pos, MyCollider.Owner);
                var targetPos = nearestEnemy != null ? nearestEnemy.Pos : Pos + Dir * 10;
                Activate(targetPos);
            }
            return ShouldDelete;
        }

        private void RollDie()
        {
            int prevValue = DieValue;
            DieValue = 1;
            for (int i = 0; i < Luck; i++)
                DieValue = Math.Max(_random.Next(1, _highestAllowedValue + 1), DieValue);
            if (DieValue == prevValue)
            {
                if (DieValue > 3)
                    DieValue--;
                else
                    DieValue++;
            }
            EntityId = "ChaosDice" + DieValue;
        }

        public void ForceReady()
        {
            _aliveTime = FlyTime + _ticksBetweenFlyAndActivate + 1;
            AdditionalDice.ForEach(d => d.ForceReady());
        }

        public bool CanActivate()
        {
            return _aliveTime > FlyTime + _ticksBetweenFlyAndActivate;
        }

        public bool Activate(Vector2 endPos)
        {
            if (!CanActivate())
                return false;
            var game = MyCollider.Owner.GetGame();
            Vector2 dir = (endPos - Pos);
            ShouldDelete = true;
            switch (DieValue)
            {
                // case 1: does nothing... unlucky I guess break;
                case 2:
                    game.Entities.Add(new SimpleSpellEntity(MyCollider.Owner, Pos)
                    {
                        Dir = dir.Normalized(),
                        Damage = Damage,
                        Knockback = Knockback,
                        Size = Size / 2,
                        Speed = Speed,
                        Color = "255, 255, 255",
                        EntityId = "DiceEye",
                        TicksUntilDeletion = FlyTime * 2,
                    });
                    break;
                case 3:
                    {
                        var dirs = ProjectileHelper.GetProjectileDirections(dir, 3, MathF.PI / 8);
                        for (int i = 0; i < dirs.Length; i++)
                        {
                            game.Entities.Add(new SimpleSpellEntity(MyCollider.Owner, Pos)
                            {
                                Dir = dirs[i],
                                Damage = Damage,
                                Knockback = Knockback,
                                Size = Size / 2,
                                Speed = Speed,
                                Color = "255, 255, 255",
                                EntityId = "DiceEye",
                                TicksUntilDeletion = FlyTime * 2,
                            });
                        }
                    }
                    break;
                case 4:
                    {
                        int projectileQuantity = 2 + Luck;
                        var dirs = ProjectileHelper.GetProjectileDirections(dir, projectileQuantity, MathF.PI * 2 / projectileQuantity);
                        for (int i = 0; i < dirs.Length; i++)
                        {
                            game.Entities.Add(new HomingBoltEntity(MyCollider.Owner, Pos + dirs[i], Pos + dirs[i] * 5, game)
                            {
                                Speed = Speed,
                                Size = Size / 4,
                                TicksUntilDeletion = FlyTime * 3,
                                Damage = Damage,
                                Knockback = Knockback,
                            });
                        }
                    }
                    break;
                case 5:
                    var newDie = new ChaosDiceEntity(MyCollider.Owner, Luck, Pos, _highestAllowedValue - 1)
                    {
                        Dir = dir.Normalized(),
                        Damage = Damage * 1.1f,
                        Knockback = Knockback * 1.1f,
                        Size = Size,
                        FlyTime = FlyTime,
                        Speed = Speed,
                    };
                    AdditionalDice.Add(newDie);
                    game.Entities.Add(newDie);
                    ShouldDelete = false;
                    RollDie();
                    break;
                case 6: // jackpot
                    {
                        // spawn more dice that activate immediately
                        int additionalDiceCount = 1 + Luck;
                        float angleStep = MathF.PI * 2 / additionalDiceCount;
                        var dirs = ProjectileHelper.GetProjectileDirections(dir, additionalDiceCount, angleStep);
                        float distance = Size * 1.5f;
                        for (int i = 0; i < dirs.Length; i++)
                        {
                            float angle = angleStep * i;
                            var jackpotDie = new ChaosDiceEntity(MyCollider.Owner, Luck, Pos + dirs[i] * distance, _highestAllowedValue - 1)
                            {
                                Dir = dirs[i],
                                Damage = Damage * 1.1f,
                                Knockback = Knockback * 1.1f,
                                Size = Size * 1.1f,
                                FlyTime = FlyTime,
                                Speed = Speed * 1.1f,
                            };
                            AdditionalDice.Add(jackpotDie);
                            game.Entities.Add(jackpotDie);
                            jackpotDie.ForceReady();
                        }
                        // also explode this die
                        game.Entities.Add(new MeteorEntity(MyCollider.Owner, Pos, game)
                        {
                            Damage = Damage * 1.5f,
                            KnockbackMin = Knockback,
                            KnockbackMax = Knockback * 1.5f,
                            Size = Size * 3,
                            FallTime = (int)(0.5f * Game._updatesPerSecond),
                        });
                    }
                    break;
            }
            for (int i = 0; i < AdditionalDice.Count; i++)
                AdditionalDice[i].Activate(endPos);
            
            return ShouldDelete;
        }

        public override void OnExpire(EntityExpiredReason reason)
        {
            base.OnExpire(reason);
            ShouldDelete = true; // incase the die get deleted by something else.
        }

        public List<ChaosDiceEntity> GetAliveAdditionalDice()
        {
            List<ChaosDiceEntity> returnList = [];
            foreach (var die in AdditionalDice)
                returnList.AddRange(die.GetAliveAdditionalDice());
            if (!ShouldDelete)
                returnList.Add(this);
            AdditionalDice.Clear();
            return returnList;
        }
    }
}
