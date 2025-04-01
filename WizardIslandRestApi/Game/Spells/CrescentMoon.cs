using System.Drawing;

namespace WizardIslandRestApi.Game.Spells
{
    public class CrescentMoon : Spell
    {
        public override string Name { get { return "Crescent Moon"; } }
        private float _damage = 5;
        private float _knockback = 1.5f;
        private float _rangeMax = 40;
        public override int CooldownMax { get; protected set; } = 2 * Game._updatesPerSecond;
        public CrescentMoon(Player player) : base(player)
        {
        }
        public override void OnCast(Vector2 mousePos)
        {
            float size = 1.0f;
            Vector2 endPos = mousePos;
            Vector2 dir = mousePos - MyPlayer.Pos;
            if (dir.LengthSqr() > _rangeMax * _rangeMax)
            {
                dir = dir.Normalized() * _rangeMax;
                endPos = MyPlayer.Pos + dir;
            }
            var ticksUntilDeletion = (int)(dir.Length() * .75f);
            dir.Normalize();
            Vector2 dirNormal = dir.Normal();
            Vector2 startPos = MyPlayer.Pos + 
                               dir * (MyPlayer.Size + size + .1f) + 
                               dirNormal * (MyPlayer.Size + size + .1f);
            GetCurrentGame().Entities.Add(new CrescentMoonEntity(MyPlayer, startPos, endPos)
            {
                Color = "100, 100, 255",
                Size = size,
                TicksUntilDeletionMax = ticksUntilDeletion,
                Damage = _damage,
                Knockback = _knockback,
            });
            GoOnCooldown();
        }
    }

    public class CrescentMoonEntity : Entity
    {
        public CrescentMoonEntity(Player owner, Vector2 startPos, Vector2 endPos) : base(owner)
        {
            StartPos = startPos;
            EndPos = endPos;
            var diff = EndPos - StartPos;
            ControlPoint = StartPos + diff * .5f;
            var normal = diff.Normal();
            ControlPoint += normal;
        }
        public override void ReTarget(Vector2 pos)
        {
            StartPos = Pos;
            EndPos = pos;
            var diff = EndPos - StartPos;
            ControlPoint = StartPos + diff * .5f;
            var normal = diff.Normal();
            ControlPoint += normal;
            _ticksUntilDeletion = _ticksUntilDeletionMax - 1;
        }
        private int _ticksUntilDeletion;
        private int _ticksUntilDeletionMax;
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
            if (TicksUntilDeletionMax - _ticksUntilDeletion < 5 && other == MyCollider.Owner)
                return false;
            other.TakeDamage(Damage, MyCollider.Owner);
            other.ApplyKnockback((other.MyCollider.PreviousPos - MyCollider.PreviousPos).Normalized(), Knockback);

            return true;
        }

        public override bool Update()
        {
            Pos = Vector2.CalculatePointOnSpline(StartPos, EndPos, ControlPoint, (float)(TicksUntilDeletionMax - _ticksUntilDeletion) / (float)TicksUntilDeletionMax);
            _ticksUntilDeletion--;
            MyCollider.Pos = Pos;
            return _ticksUntilDeletion < -1;
        }
    }
}
