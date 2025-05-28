using WizardIslandRestApi.Game.Spells.ExtraEntities;

namespace WizardIslandRestApi.Game.Spells.Ultimates
{
    public class LastResort : Spell
    {
        private int _cooldown;
        private int _maxRange = 30;
        public LastResort(Player player) : base(player)
        {
            Type = SpellType.Ultimate;
        }
        public override string Name => "Last Resort";

        public override int CurrentCooldown { get => MyPlayer.Stats.Health <= MyPlayer.Stats.MaxHealth/3 ? _cooldown : GetCurrentGameTick()+Game._updatesPerSecond*100/3; set => _cooldown = value; }
        public override int CooldownMax { get; protected set; } = 10 * Game._updatesPerSecond;

        public override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            Vector2 endPos = mousePos - startPos;
            var len = endPos.Length();
            if (len > _maxRange)
            {
                endPos = endPos / len * _maxRange;
                len = _maxRange;
            }
            endPos += startPos;
            const int healthCost = 2;
            int boltCount = MyPlayer.Stats.Health / healthCost;
            if (boltCount < 5) boltCount = 5; // min bolts
            const float maxToSide = 1;
            const float maxDeviation = 3;
            const float damage = 3;
            const float knockback = 1.3f;
            int ticksUntilDeletion = (int)len + 1;
            Random r = new Random();
            Vector2 GetRandomPosInRange(float maxAmount) { return new Vector2((float)(r.NextDouble() * 2 - 1) * maxAmount, (float)(r.NextDouble() * 2 - 1) * maxAmount); }
            for (int i = 0; i < boltCount; i++)
            {
                if (MyPlayer.Stats.Health > healthCost)
                    MyPlayer.Stats.Health -= healthCost;
                GetCurrentGame().Entities.Add(new ChaosBolt(MyPlayer, ticksUntilDeletion, startPos, endPos + GetRandomPosInRange(maxDeviation), healthCost, (float)(r.NextDouble() * 2 - 1) * maxToSide, (float)r.NextDouble() * .6f + .2f)
                {
                    Size = .35f,
                    Color = $"{r.Next(256)},{r.Next(256)},{r.Next(256)}",
                    Damage = damage,
                    Knockback = knockback,
                    DeleteWhenLessThan = -10,
                });
            }
            GoOnCooldown();
        }
    }
    class ChaosBolt : EntityPlus
    {
        private int _healthCost;
        public int DeleteWhenLessThan = -3;
        public Vector2 StartPos { get; set; }
        public Vector2 EndPos { get; set; }
        public Vector2 ControlPoint { get; private set; }
        public float AmountToSideMultiplier { get; set; } = 1.0f;
        public float ControlPointAmountForward { get; set; } = 1.0f;
        public ChaosBolt(Player owner, int ticksUntilDeletion, Vector2 startPos, Vector2 endPos, int healthCost = 1, float amountToSideMultiplier = 1, float controlPointAmountForward = .5f) : base(owner, ticksUntilDeletion, startPos)
        {
            EntityId = "ChaosBolt";
            StartPos = startPos;
            EndPos = endPos;
            var diff = EndPos - StartPos;
            ControlPointAmountForward = controlPointAmountForward;
            ControlPoint = StartPos + diff * ControlPointAmountForward;
            var normal = diff.Normal();
            AmountToSideMultiplier = amountToSideMultiplier;
            ControlPoint += normal * AmountToSideMultiplier;
            _healthCost = healthCost;
        }
        public override void ReTarget(Vector2 pos)
        {
            StartPos = Pos;
            EndPos = pos;
            var diff = EndPos - StartPos;
            ControlPoint = StartPos + diff * ControlPointAmountForward;
            var normal = diff.Normal();
            ControlPoint += normal * AmountToSideMultiplier;
            _ticksUntilDeletion = _ticksUntilDeletionMax - 1;
        }

        public override bool Update()
        {
            Pos = Vector2.CalculatePointOnSpline(StartPos, EndPos, ControlPoint, (float)(_ticksUntilDeletionMax - _ticksUntilDeletion) / (float)_ticksUntilDeletionMax);
            _ticksUntilDeletion--;
            MyCollider.Pos = Pos;
            if (_ticksUntilDeletion < DeleteWhenLessThan)
            {
                Die();
                return true;
            }
            return false;
        }

        protected override bool HitPlayer(Player other)
        {
            other.TakeDamage(Damage, MyCollider.Owner);
            Vector2 prevPos = Vector2.CalculatePointOnSpline(StartPos, EndPos, ControlPoint, (float)(_ticksUntilDeletionMax - _ticksUntilDeletion) / (float)_ticksUntilDeletionMax - .3f);
            other.ApplyKnockback((other.Pos - MyCollider.PreviousPos).Normalized(), Knockback);
            Die();
            return true;
        }
        public override bool OnCollision(Entity other)
        {
            return false;
        }
        private void Die()
        {
            MyCollider.Owner.Stats.Health += _healthCost;
        }
    }
}
