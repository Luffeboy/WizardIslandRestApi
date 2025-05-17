using WizardIslandRestApi.Game.Spells.Debuffs;
using WizardIslandRestApi.Game.Spells.ExtraEntities;

namespace WizardIslandRestApi.Game.Spells
{
    public abstract class BrickSpell : Spell
    {
        protected int _currentCooldown;
        public int BricksToApplyOnRespawn { get; protected set; } = 1; 
        protected int BrickCount { get { return MyPlayer.GetDebuffs().Where(d => d.ToString() == BrickBuff.BrickName).Count(); } }
        public override int CurrentCooldown { get { return (BrickCount > 0) ? -1 : _currentCooldown; } set { _currentCooldown = value; } }
        public BrickSpell(Player player) : base(player)
        {
        }

        public override int CooldownMax { get; protected set; } = 30 * Game._updatesPerSecond;
        public override bool CanCast {  get { return BrickCount > 0 || base.CanCast; } }

        protected void GoOnCooldownBrick()
        {
            MyPlayer.RemoveDebuff(BrickBuff.BrickName);
            GoOnCooldown();
        }
    }

    public class BrickEntity : EntityPlus
    {
        private int _stopedTicks = 0;
        private float _startSpeed;
        public BrickEntity(Player owner, int ticksUntilDeletion, Vector2 startPos, float startSpeed = 3) : base(owner, ticksUntilDeletion, startPos)
        {
            _startSpeed = startSpeed;
            Speed = startSpeed;
            EntityId = "Brick";
            Size = .75f;
            Color = "188,74,60";
        }

        public override void ReTarget(Vector2 pos)
        {
            if (_stopedTicks == 0)
            {
                Dir = (pos - Pos).Normalized();
                Speed = _startSpeed;
            }
        }

        public override bool Update()
        {
            Pos += Dir * (Speed*=.93f);
            if (Speed < .1f)
                _stopedTicks++;
            return --_ticksUntilDeletion < 0;
        }

        protected override bool HitPlayer(Player other)
        {
            if (_stopedTicks > 1 * Game._updatesPerSecond)
            {
                PlayerPickUp(other);
                return true;
            }
            if (_stopedTicks == 0)
            {
                other.TakeDamage(Damage * Speed, MyCollider.Owner);
                other.ApplyKnockback(Dir, Knockback * Speed);
                Height = EntityHeight.Ground;
                Speed = 0;
            }
            return false;
        }
        protected void PlayerPickUp(Player player)
        {
            player.ApplyDebuff(new BrickBuff(player));
        }
        public override bool OnCollision(Entity other)
        {
            if (base.OnCollision(other))
            {
                Speed = 0;
                Height = EntityHeight.Ground;
            }
            return false;
        }
    }
}
