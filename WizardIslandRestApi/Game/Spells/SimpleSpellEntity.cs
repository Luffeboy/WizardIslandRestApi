﻿namespace WizardIslandRestApi.Game.Spells
{
    public class SimpleSpellEntity : Entity
    {
        public SimpleSpellEntity(Player owner, Vector2 startPos) : base(owner, startPos)
        {
        }

        private int _ticksUntilDeletion;
        private int _ticksUntilDeletionMax;
        public Vector2 Dir { get; set; }
        public float Speed { get; set; }
        public float Damage { get; set; }
        public float Knockback { get; set; }
        public int TimeUntilCanHitOwner { get; set; } = 5;
        public int TicksUntilDeletion
        {
            get { return _ticksUntilDeletion; }
            set
            {
                _ticksUntilDeletionMax = value;
                _ticksUntilDeletion = _ticksUntilDeletionMax - 1;
            }
        }
        //public override bool OnCollision(Entity other)
        //{
        //    return true;
        //}
        public override void ReTarget(Vector2 pos)
        {
            _ticksUntilDeletion = _ticksUntilDeletionMax;
            Dir = (pos - Pos).Normalized();
        }

        public override bool OnCollision(Player other)
        {
            if (_ticksUntilDeletionMax - _ticksUntilDeletion < TimeUntilCanHitOwner && other == MyCollider.Owner)
                return false;
            other.TakeDamage(Damage, MyCollider.Owner);
            other.ApplyKnockback((other.MyCollider.Pos - (Pos - Dir * Speed * 5)).Normalized(), Knockback);
            //other.ApplyKnockback((other.MyCollider.Pos - MyCollider.PreviousPos).Normalized(), Knockback);

            return true;
        }

        public override bool Update()
        {
            Pos += Dir * Speed;
            _ticksUntilDeletion--;
            MyCollider.Pos = Pos;
            return _ticksUntilDeletion <= 0;
        }
    }
}
