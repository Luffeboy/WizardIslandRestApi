using System;
using System.Drawing;
using System.Xml.Linq;

namespace WizardIslandRestApi.Game.Spells.BasicSpells
{
    public class Barrel : Spell
    {
        public override string Name { get { return "Barrel"; } }
        private float _damage = 5;
        private float _knockback = 2.0f;
        public override int CooldownMax { get; protected set; } = (int)(5.0f * Game._updatesPerSecond);
        private float _range = 30;
        public Barrel(Player player) : base(player)
        {
        }
        public override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            Vector2 dir = mousePos - startPos;
            float len = dir.Length();
            if (len != 0)
            {
                dir.x /= len;
                dir.y /= len;
            }
            if (len > _range)
                len = _range;
            Vector2 pos = startPos + dir * len;
            GetCurrentGame().Entities.Add(new BarrelEntity(MyPlayer, GetCurrentGame(), pos)
            {
                Color = "255, 0, 0",
                Size = 1.5f,
                TicksUntilDeletion = 30 * Game._updatesPerSecond,
                Damage = _damage,
                Knockback = _knockback
            });
            GoOnCooldown();
        }
    }
    public class BarrelEntity : Entity
    {
        private bool _justMoved = false;
        private Game _game;
        public int TicksUntilDeletion { get; set; }
        public float Damage { get; set; }
        public float Knockback { get; set; }
        public BarrelEntity(Player owner, Game game, Vector2 startPos) : base(owner, startPos)
        {
            _game = game;
            EntityId = "Barrel";
            TeleportTo(startPos);
        }

        public override bool OnCollision(Player other)
        {
            Vector2 dir = Pos - other.MyCollider.Pos;
            dir.Normalize();
            TeleportTo(other.Pos + dir * (Size + other.Size + .1f));
            return false;
        }
        public override bool OnCollision(Entity other)
        {
            if (!base.OnCollision(other))
                return false;
            if (other is BarrelEntity barrel)
            {
                if (barrel._justMoved)
                {
                    barrel._justMoved = false;
                    return false;
                }
                _justMoved = true;
                Vector2 midpoint = (Pos + other.Pos) / 2;
                Vector2 dir = (Pos - other.Pos).Normalized();
                if (dir.LengthSqr() == 0) dir.x = .1f;
                float moveAmount = (Size + other.Size + .01f) * .5f;
                barrel.TeleportTo(midpoint + dir * moveAmount);
                TeleportTo(Pos = midpoint - dir * moveAmount);
                return false;
            }
            // create explosion
            _game.Entities.Add(new MeteorEntity(MyCollider.Owner, Pos, _game)
            {
                Color = "50, 50, 50",
                Size = Size * 3,
                FallTime = 3,
                Damage = Damage,
                KnockbackMin = Knockback * .75f,
                KnockbackMax = Knockback * 1.5f,
            });
            return true;
        }

        public override void ReTarget(Vector2 pos)
        {
        }

        public override bool Update()
        {
            return TicksUntilDeletion-- < 0;
        }
    }
}
