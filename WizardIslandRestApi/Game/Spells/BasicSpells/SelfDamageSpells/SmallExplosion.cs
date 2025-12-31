using WizardIslandRestApi.Game.Spells.ExtraEntities;

namespace WizardIslandRestApi.Game.Spells.BasicSpells.SelfDamageSpells
{
    public class SmallExplosion : Spell
    {
        private float _damage = 8f;
        private float _knockback = 1.5f;
        private float _selfDamage { get => _damage / 2; }
        private float _selfKnockback { get => _knockback; }
        public override string Name => "Small Explosion";
        public override int CooldownMax { get; protected set; } = (int)(2.5f * Game._updatesPerSecond);

        public SmallExplosion(Player player) : base(player)
        {
        }

        public override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            var dir = (mousePos - startPos).Normalized();
            float radius = .75f;
            GetCurrentGame().Entities.Add(new ExplosionEntity(MyPlayer, startPos + dir * (radius/2 + MyPlayer.Size))
            {
                Damage = _damage,
                Knockback = _knockback,
                Size = radius
            });
            // apply self knockback
            MyPlayer.ApplyKnockback(dir * -1, _selfKnockback);
            MyPlayer.TakeDamage(_selfDamage);
            // move target position
            //var playerDir = (MyPlayer.TargetPos - MyPlayer.Pos).Normalized();
            //if (playerDir.LengthSqr() < .1f || playerDir.Dot(dir * -1) < .7f)
            {
                float targetDist = 20;// MyPlayer.Vel.LengthSqr() * 20;
                MyPlayer.TargetPos = MyPlayer.Pos + (dir * -targetDist);
            }
            GoOnCooldown();
        }
    }

    public class ExplosionEntity : CantHitOwnerAtStartSpellEntity
    {
        public float Damage { get; set; }
        public float Knockback { get; set; }
        private List<Player> _hitPlayers = [];
        public ExplosionEntity(Player owner, Vector2 startPos, int ticksUntilDeletion = Game._updatesPerSecond / 3) : base(owner, ticksUntilDeletion, startPos)
        {
            Color = "255, 0, 0";
            EntityId = "Meteor";
            TicksUntillCanHitOwner = ticksUntilDeletion + 1;
        }

        public override void ReTarget(Vector2 pos)
        {
            _ticksUntilDeletion = _ticksUntilDeletionMax;
        }

        protected override bool HitPlayer(Player other)
        {
            if (_hitPlayers.Contains(other))
                return false;
            _hitPlayers.Add(other);
            Vector2 dir = (other.Pos - Pos).Normalized();
            other.TakeDamage(Damage, MyCollider.Owner);
            other.ApplyKnockback(dir, Knockback);
            return false;
        }
    }
}
