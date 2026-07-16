using WizardIslandRestApi.Game.Spells.ExtraEntities;
using WizardIslandRestApi.Game.Spells.SpellHelpers;

namespace WizardIslandRestApi.Game.Spells.BasicSpells
{
    public class HomingBolt : Spell
    {
        public override string Name { get { return "Homing bolt"; } }
        public override int CooldownMax { get; protected set; } = (int)(7.5f * Game._updatesPerSecond);
        public HomingBolt(Player player) : base(player)
        {
            StandardStats.Damage = 5;
            StandardStats.Knockback = 1.5f;
            StandardStats.Size = .5f;
            StandardStats.Speed = 1f;
            StandardStats.Range = 3f * StandardStats.Speed;

            Tags.Add(SpellTags.Projectile);
            ProjectileHelper.SetProjectileStats(this);
        }
        public override void OnCast(Vector2 pos, Vector2 mousePos)
        {
            float distance = (mousePos - pos).Length();
            var dirs = ProjectileHelper.GetProjectileDirections(this, mousePos - pos);
            ProjectileHelper.CastSpellWithBurst(this, pos, (startPos, iteration) =>
            {
                for(int i = 0; i < dirs.Length; i++)
                {
                    GetCurrentGame().Entities.Add(new HomingBoltEntity(MyPlayer, startPos, startPos + dirs[i] * distance, GetCurrentGame())
                    {
                        Speed = StandardStats.Speed,
                        Color = "255, 255, 255",
                        Size = StandardStats.Size,
                        TicksUntilDeletion = StandardStats.GetLifetime(),
                        Damage = StandardStats.Damage,
                        Knockback = StandardStats.Knockback,
                    });

                    GetCurrentGame().Entities.Add(new ShadowEntity() { Pos = startPos + dirs[i] * distance, Size = 1 });
                }
            });
            GoOnCooldown();
        }
    }
    public class HomingBoltEntity : Entity
    {
        public float Damage { get; set; }
        public float Knockback { get; set; }
        public float Speed { get; set; }
        public int TicksUntilDeletion { get { return _ticksUntilDeletion; } set { _ticksUntilDeletion = value; _ticksUntilDeletionMax = value; } }
        private float _rotationSpeed = .075f;
        private Player CurrentTarget { get; set; }
        private int _ticksUntilDeletion;
        private int _ticksUntilDeletionMax;
        private Game _game;
        private float _angle = 0;

        private bool _hitOwnerLastUpdate = true;
        private bool _ignoreHitOnOwner = true;

        public HomingBoltEntity(Player owner, Vector2 startPos, Vector2 mousePos, Game game) : base(owner, startPos)
        {
            _game = game;
            Pos = startPos;
            ReTarget(mousePos);
            EntityId = "HomingBolt";
        }
        public override void ReTarget(Vector2 pos)
        {
            _angle = MathF.Atan2(pos.y - Pos.y, pos.x - Pos.x);
            CurrentTarget = FindClosestPlayer(pos);
            Vector2 dir = new Vector2(MathF.Cos(_angle), MathF.Sin(_angle));
            Pos += dir * Speed * 2;
        }

        public override bool OnCollision(Player other)
        {
            if (_ignoreHitOnOwner && other == MyCollider.Owner)
            {
                _hitOwnerLastUpdate = true;
                return false;
            }
            other.TakeDamage(Damage, MyCollider.Owner);
            other.ApplyKnockback((other.MyCollider.PreviousPos - MyCollider.PreviousPos).Normalized(), Knockback);
            return true;
        }


        public override bool Update()
        {
            // maybe find new target
            if (_ticksUntilDeletionMax - _ticksUntilDeletion > 20)
            {
                CurrentTarget = FindClosestPlayer(Pos);
            }
            // rotate towards new angle
            var tempAngle = GetAngleToTarget();
            ForwardAngle = tempAngle; // looks at target
            float diff = DeltaAngle(_angle, tempAngle);
            _angle += Math.Sign(diff) * Math.Min(Math.Abs(diff), _rotationSpeed);
            // move
            Vector2 dir = new Vector2(MathF.Cos(_angle), MathF.Sin(_angle));
            Pos += dir * Speed;

            _ignoreHitOnOwner = _ignoreHitOnOwner && _hitOwnerLastUpdate;
            _hitOwnerLastUpdate = false;
            return --_ticksUntilDeletion < 0;
        }

        private Player FindClosestPlayer(Vector2 pos)
        {
            Player p = _game.Players[0];
            float closestDist = (pos - _game.Players[0].Pos).LengthSqr();
            if (p.IsDead) // we would prefere not to fly towards a dead person
                closestDist = float.MaxValue;
            for (int i = 1; i < _game.Players.Count; i++)
            {
                if (_game.Players[i].IsDead)
                    continue;
                float dst = (pos - _game.Players[i].Pos).LengthSqr();
                if (dst < closestDist)
                {
                    closestDist = dst;
                    p = _game.Players[i];
                }
            }
            return p;
        }
        private float GetAngleToTarget()
        {
            return GetAngleFromDirection(CurrentTarget.Pos - Pos);
        }
        public static float GetAngleFromDirection(Vector2 dir)
        {
            float angle = MathF.Atan2(dir.y, dir.x);
            return angle;
        }
        public static float DeltaAngle(float current, float target)
        {
            float diff = (target - current) % (MathF.PI * 2);
            if (diff >= MathF.PI)
            {
                diff -= MathF.PI * 2;
            }
            else if (diff < -MathF.PI)
            {
                diff += MathF.PI * 2;
            }
            return diff;
        }
    }
}
