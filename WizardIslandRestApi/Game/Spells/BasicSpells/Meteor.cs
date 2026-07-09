using WizardIslandRestApi.Game.Physics;
using WizardIslandRestApi.Game.Spells.ExtraEntities;

namespace WizardIslandRestApi.Game.Spells.BasicSpells
{
    public class Meteor : Spell
    {
        public override string Name { get { return "Meteor"; } }
        public override int CooldownMax { get; protected set; } = (int)(15.0f * Game._updatesPerSecond);
        private int _fallTime = (int)(.5f * Game._updatesPerSecond);
        public Meteor(Player player) : base(player)
        {
            StandardStats.Damage = 15;
            StandardStats.Knockback = 3;
            StandardStats.Size = 4.3f;
        }
        public override void OnCast(Vector2 pos, Vector2 mousePos)
        {
            var dir = (mousePos - pos).Normalized();
            GetCurrentGame().Entities.Add(new MeteorEntity(MyPlayer, mousePos, GetCurrentGame())
            {
                Color = "50, 50, 50",
                Size = StandardStats.Size,
                FallTime = _fallTime,
                Damage = StandardStats.Damage,
                KnockbackMin = StandardStats.Knockback * 0.8f,
                KnockbackMax = StandardStats.Knockback * 1.2f,
            });
            GoOnCooldown();
        }
    }
    public class MeteorEntity : Entity
    {
        public float Damage { get; set; }
        public float KnockbackMin { get; set; }
        public float KnockbackMax { get; set; }
        public int FallTime { get; set; }
        List<Player> _hitPlayers = new List<Player>();
        private Player _player;
        private Game _game;
        public MeteorEntity(Player owner, Vector2 pos, Game game) : base(owner)
        {
            _player = owner;
            MyCollider = null;
            Pos = pos;
            Height = EntityHeight.Ground;
            _game = game;
            EntityId = "Meteor";
        }

        public override bool OnCollision(Entity other)
        {
            return false;
        }

        public override bool OnCollision(Player other)
        {
            // can't hit the same enemy twice
            if (_hitPlayers.Contains(other))
                return false;
            _hitPlayers.Add(other);
            // deal damage
            other.TakeDamage(Damage, _player);
            // calculate knockback, between kbMin and kbMax
            Vector2 dir = other.MyCollider.PreviousPos - Pos;
            float dist = dir.Length();
            float knockBackMultiplier = MathF.Max(1 - dist / Size, 0);
            float knockback = (KnockbackMax - KnockbackMin) * knockBackMultiplier + KnockbackMin;
            // if dist is 0, we could get a /0 error, we don't want that
            if (dist == 0)
                dist = 0.1f;
            other.ApplyKnockback(dir / dist, knockback);
            return false;
        }

        public override bool Update()
        {
            if (FallTime < -3)
            {
                _game.Entities.Add(new ShadowEntity(null)
                {
                    Pos = Pos,
                    EntityId = "Crator",
                    TicksUntilDeletion = 2 * Game._updatesPerSecond,
                    Size = Size
                });
                return true;
            }
            if (FallTime <= 0 && MyCollider == null)
            {
                MyCollider = new Collider(Pos) { Owner = _player, Pos = Pos, Size = Size };
                Color = "255, 0, 0";
                Height = EntityHeight.Normal;
            }
            FallTime--;
            return false;
        }
        public override void ReTarget(Vector2 pos) { }
    }
}
