using WizardIslandRestApi.Game.Spells.SpellHelpers;

namespace WizardIslandRestApi.Game.Spells.BasicSpells
{
    public class CrescentMoon : Spell
    {
        public override string Name { get { return "Crescent Moon"; } }
        public override int CooldownMax { get; protected set; } = (int)(2.5f * Game._updatesPerSecond);
        public CrescentMoon(Player player) : base(player)
        {
            StandardStats.Damage = 5;
            StandardStats.Knockback = 1.5f;
            StandardStats.Range = 40;
            StandardStats.Size = 1.0f;
            StandardStats.Speed = 1.35f;

            Tags.Add(SpellTags.Projectile);
            ProjectileHelper.SetProjectileStats(this, angle: MathF.PI / 16);
        }
        public override void OnCast(Vector2 pos, Vector2 mousePos)
        {
            Vector2 dir = mousePos - pos;
            float range = dir.Length();
            float minRange = 3f;
            if (range < minRange)
                range = minRange;
            var ticksUntilDeletion = Math.Max((int)(range / StandardStats.Speed), 1);
            var projectileDirections = ProjectileHelper.GetProjectileDirections(this, dir);

            ProjectileHelper.CastSpellWithBurst(this, pos, (spawnPos, iteration) =>
            {
                for (int i = 0; i < projectileDirections.Length; i++)
                {
                    var dir = projectileDirections[i];

                    Vector2 dirNormal = dir.Normal();
                    Vector2 startPos = spawnPos;
                    Vector2 endPos = spawnPos + dir * range;
                    GetCurrentGame().Entities.Add(new CrescentMoonEntity(MyPlayer, startPos, endPos)
                    {
                        Color = "100, 100, 255",
                        Size = StandardStats.Size,
                        TicksUntilDeletionMax = ticksUntilDeletion,
                        Damage = StandardStats.Damage,
                        Knockback = StandardStats.Knockback,
                    });
                }
            });
            GoOnCooldown();
        }
    }

    public class CrescentMoonEntity : Entity
    {
        public float AmountToSideMultiplier { get; set; } = 1.0f;
        public CrescentMoonEntity(Player owner, Vector2 startPos, Vector2 endPos, float amountToSideMultiplier = 1) : base(owner, startPos)
        {
            StartPos = startPos;
            EndPos = endPos;
            var diff = EndPos - StartPos;
            ControlPoint = StartPos + diff * .5f;
            var normal = diff.Normal();
            AmountToSideMultiplier = amountToSideMultiplier;
            ControlPoint += normal * AmountToSideMultiplier;
            EntityId = "CrescentMoon";
        }
        public override void ReTarget(Vector2 pos)
        {
            StartPos = Pos;
            EndPos = pos;
            var diff = EndPos - StartPos;
            ControlPoint = StartPos + diff * .5f;
            var normal = diff.Normal();
            ControlPoint += normal * AmountToSideMultiplier;
            _ticksUntilDeletion = _ticksUntilDeletionMax - 1;
        }
        private int _ticksUntilDeletion;
        private int _ticksUntilDeletionMax;
        public int DeleteWhenLessThan = -1;
        public int TicksUntillCanHitPlayer = 5;
        public Vector2 StartPos { get; set; }
        public Vector2 EndPos { get; set; }
        public Vector2 ControlPoint { get; private set; }
        public float Damage { get; set; }
        public float Knockback { get; set; }
        public int TicksUntilDeletionMax 
        { get { return _ticksUntilDeletionMax; } set 
            {
                _ticksUntilDeletionMax = value;
                _ticksUntilDeletion = _ticksUntilDeletionMax - 1;
            } }
        public override bool OnCollision(Entity other)
        {
            return false;
        }

        public override bool OnCollision(Player other)
        {
            // just created entity
            if (TicksUntilDeletionMax - _ticksUntilDeletion < TicksUntillCanHitPlayer && other == MyCollider.Owner)
                return false;
            other.TakeDamage(Damage, MyCollider.Owner);
            other.ApplyKnockback((other.MyCollider.PreviousPos - MyCollider.PreviousPos).Normalized(), Knockback);

            return true;
        }

        public override bool Update()
        {
            Pos = Vector2.CalculatePointOnSpline(StartPos, EndPos, ControlPoint, (TicksUntilDeletionMax - _ticksUntilDeletion) / (float)TicksUntilDeletionMax);
            return --_ticksUntilDeletion < DeleteWhenLessThan;
        }
    }
}
