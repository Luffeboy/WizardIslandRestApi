using System;
using System.Drawing;
using System.Xml.Linq;

namespace WizardIslandRestApi.Game.Spells
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
        public override void OnCast(Vector2 mousePos)
        {
            Vector2 dir = mousePos - MyPlayer.Pos;
            float len = dir.Length();
            if (len != 0)
            {
                dir.x /= len;
                dir.y /= len;
            }
            if (len > _range)
                len = _range;
            Vector2 pos = MyPlayer.Pos + dir * len;
            GetCurrentGame().Entities.Add(new BarrelEntity(MyPlayer, GetCurrentGame())
            {
                Pos = pos,
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
        private Game _game;
        public int TicksUntilDeletion { get; set; }
        public float Damage { get; set; }
        public float Knockback { get; set; }
        public BarrelEntity(Player owner, Game game) : base(owner)
        {
            _game = game;
            EntityId = "Barrel";
        }

        public override bool OnCollision(Player other)
        {
            Vector2 dir = Pos - other.MyCollider.PreviousPos;
            dir.Normalize();
            Pos = other.Pos + dir * (Size + other.Size);
            return false;
        }
        public override bool OnCollision(Entity other)
        {
            if (!base.OnCollision(other))
                return false;
            // create explosion
            _game.Entities.Add(new MeteorEntity(MyCollider.Owner, Pos)
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
