using WizardIslandRestApi.Game.Spells.BasicSpells;
using WizardIslandRestApi.Game.Spells.ExtraEntities;

namespace WizardIslandRestApi.Game.Spells.Ultimates
{
    public class SpeedBlits : MultiUseSpell
    {
        public override string Name => "Speed blitz";
        public SpeedBlits(Player player) : base(player)
        {
            UsesMax = 5;
            CooldownBetweenUses = 3;
            Type = SpellType.Ultimate;
            StandardStats.Damage = 10;
            StandardStats.Knockback = 1.2f;
            StandardStats.Speed = 2;
            StandardStats.Size = .5f;
            StandardStats.Range = 10;
        }

        public override int CooldownMax { get; protected set; } = 10 * Game._updatesPerSecond;

        public override void OnUse(Vector2 startPos, Vector2 mousePos)
        {
            Vector2 endPos = (mousePos - startPos);
            float len = endPos.Length();
            if (len > StandardStats.Range)
            {
                endPos = endPos / len * StandardStats.Range;
                len = StandardStats.Range;
            }
            endPos = endPos + startPos;

            string color = "255,255,0";
            Vector2 pos = startPos;
            Vector2 normal = (endPos - startPos).Normal() / len;
            int boltLifetime = 100;
            GetCurrentGame().Entities.Add(new SpeedBlitsEntity(MyPlayer, pos + normal * len, pos, GetCurrentGame())
            {
                Damage = StandardStats.Damage,
                Knockback = StandardStats.Knockback,
                Speed = StandardStats.Speed,
                Size = StandardStats.Size,
                Color = color,
                Dir = normal * -1,
                TicksUntilDeletion = boltLifetime,
            });
            GetCurrentGame().Entities.Add(new SpeedBlitsEntity(MyPlayer, pos + normal * -len, pos, GetCurrentGame())
            {
                Damage = StandardStats.Damage,
                Knockback = StandardStats.Knockback,
                Speed = StandardStats.Speed,
                Size = StandardStats.Size,
                Color = color,
                Dir = normal * 1,
                TicksUntilDeletion = boltLifetime,
            });

            MyPlayer.TeleportTo(endPos);
        }
    }

    public class SpeedBlitsEntity : Entity
    {
        public float Speed { get; set; }
        public float Damage { get; set; }
        public float Knockback { get; set; }
        public Vector2 Dir { get; set; }

        private int _ticksUntilDeletion;
        private int _ticksUntilDeletionMax;

        private bool _goingBackToPlayer = false;
        private bool _hasSpawnedMeteor = false;
        private Vector2 _meetingPos;
        private Game _game;
        private List<Player> _hitPlayers = new List<Player> ();

        public int TicksUntilDeletion
        {
            get { return _ticksUntilDeletion; }
            set
            {
                _ticksUntilDeletionMax = value;
                _ticksUntilDeletion = _ticksUntilDeletionMax - 1;
            }
        }
        public SpeedBlitsEntity(Player owner, Vector2 startPos, Vector2 meetingPos, Game game) : base(owner, startPos)
        {
            _meetingPos = meetingPos;
            _game = game;
        }

        public override bool OnCollision(Player other)
        {
            if (other == MyCollider.Owner || _hitPlayers.Contains(other))
                return false;
            _hitPlayers.Add(other);
            other.TakeDamage(Damage, MyCollider.Owner);
            other.ApplyKnockback(Dir, Knockback);
            return false;
        }
        public override bool OnCollision(Entity other)
        {
            if (other is SpeedBlitsEntity && !_hasSpawnedMeteor)
            {
                _hasSpawnedMeteor = true;
                _game.Entities.Add(new MeteorEntity(MyCollider.Owner, Pos, _game)
                {
                    FallTime = 10,
                    Damage = Damage,
                    KnockbackMin = Knockback / 2,
                    KnockbackMax = Knockback,
                    Size = 3,
                    Color = "100,100,0",
                    EntityId = "ElectricExplosion",
                });
            }
            return false;
        }

        public override void ReTarget(Vector2 pos)
        {
            Speed = -Speed;
        }

        public override bool Update()
        {
            Pos += Dir * Speed;
            if (_goingBackToPlayer)
            {
                if ((MyCollider.Owner.Pos - Pos).Dot(Dir) < 0)
                    return true;
            }
            else
            {
                if ((_meetingPos - Pos).Dot(Dir) < 0)
                {
                    Dir = (MyCollider.Owner.Pos - Pos).Normalized();
                    _goingBackToPlayer = true;
                }
            }
            return --_ticksUntilDeletion < 0;
        }
    }
}
