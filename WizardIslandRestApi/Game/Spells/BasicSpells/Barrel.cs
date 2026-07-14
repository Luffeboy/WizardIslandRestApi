using System;
using System.Drawing;
using System.Xml.Linq;

namespace WizardIslandRestApi.Game.Spells.BasicSpells
{
    public class Barrel : Spell
    {
        public override string Name { get { return "Barrel"; } }
        public override int CooldownMax { get; protected set; } = (int)(5.0f * Game._updatesPerSecond);
        public Barrel(Player player) : base(player)
        {
            StandardStats.Damage = 5;
            StandardStats.Knockback = 2.0f;
            StandardStats.Range = 30;
            StandardStats.Size = 1.5f;
            StandardStats.SummonLifetime = 30 * Game._updatesPerSecond;
            StandardStats.OtherStatsInt.Add(SpellSpecificStats.SummonQuantity, 1);

            Tags.Add(SpellTags.CreateEnvironment);
            Tags.Add(SpellTags.Summon);
            Tags.Add(SpellTags.Zone);
            Tags.Add(SpellTags.Static);
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
            if (len > StandardStats.Range)
                len = StandardStats.Range;
            Vector2 pos = startPos + dir * len;
            int quantity = StandardStats.OtherStatsInt[SpellSpecificStats.SummonQuantity];
            Random r = new Random();
            Vector2 firstPos = pos;
            for (int i = 0; i < quantity; i++)
            {
                GetCurrentGame().Entities.Add(new BarrelEntity(MyPlayer, GetCurrentGame(), pos)
                {
                    Color = "255, 0, 0",
                    Size = StandardStats.Size,
                    TicksUntilDeletion = StandardStats.SummonLifetime,
                    Damage = StandardStats.Damage,
                    Knockback = StandardStats.Knockback
                });
                pos = firstPos + new Vector2((float)(r.NextDouble() * 2 - 1), (float)(r.NextDouble() * 2 - 1)) * StandardStats.Size / 4;
            }
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
