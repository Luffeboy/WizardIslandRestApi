using System.Numerics;
using WizardIslandRestApi.Game.Spells.Debuffs;
using WizardIslandRestApi.Game.Spells.ExtraEntities;

namespace WizardIslandRestApi.Game.Spells.BasicSpells.BrickSpells
{
    public abstract class BrickSpell : Spell
    {
        protected int _currentCooldown;
        public int MinBricksToCast { get; protected set; } = 1;

        protected BrickBuff? MyPlayersBrickBuff => 
            MyPlayer.GetDebuffs().FirstOrDefault(d => d.ToString() == BrickBuff.BrickName) as BrickBuff;

        protected int BrickCount
            => MyPlayersBrickBuff?.Stacks ?? 0;
        
        public override int CurrentCooldown { get { return BrickCount >= MinBricksToCast ? -1 : _currentCooldown; } set { _currentCooldown = value; } }
        public override int CooldownMax { get; protected set; } = 30 * Game._updatesPerSecond;
        public override bool CanCast {  get { return BrickCount > 0 || base.CanCast; } }

        public BrickSpell(Player player, int bricksToApplyOnRespawn = 1) : base(player)
        {
            StandardStats.OtherStatsInt.Add(SpellSpecificStats.BricksToApplyOnRespawn, bricksToApplyOnRespawn);
            Tags.Add(SpellTags.Brick);
        }

        protected void RemoveBrickBuffs(int amount)
        {
            var brickBuff = MyPlayersBrickBuff;
            if (brickBuff is null)
                return;
            brickBuff.Stacks -= amount;
            if (brickBuff.Stacks <= 0)
                MyPlayer.RemoveDebuff(BrickBuff.BrickName);
        }

        protected void GoOnCooldownBrick(int bricksToRemove = 1)
        {
            RemoveBrickBuffs(bricksToRemove);
            GoOnCooldown();
        }

        public override void OnPlayerReset()
        {
            base.OnPlayerReset();
            for (int i = 0; i < StandardStats.OtherStatsInt[SpellSpecificStats.BricksToApplyOnRespawn]; i++)
                MyPlayer.ApplyDebuff(new BrickBuff(MyPlayer));
        }
    }

    public class BrickEntity : EntityPlus
    {
        private int _stopedTicks = 0;
        private float _startSpeed;
        public bool ShouldDropBrick { get; set; } = true;
        public List<string> EntityNamesToIgnore { get; set; } = [];
        public float SpeedMultiplierPerUpdate { get; set; } = .93f;

        private Player _originalOwner;

        public BrickEntity(Player owner, int ticksUntilDeletion, Vector2 startPos, float startSpeed = 3) : base(owner, ticksUntilDeletion, startPos)
        {
            _originalOwner = owner;
            _startSpeed = startSpeed;
            Speed = startSpeed;
            EntityId = "Brick";
            Size = .75f;
            Color = BrickBuff.BrickColor;
            Density = 8;
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
            Pos += Dir * (Speed *= SpeedMultiplierPerUpdate);
            if (Speed < .1f)
            {
                if (!ShouldDropBrick)
                    return true;
                _stopedTicks++;
                Density += 1.0f / Game._updatesPerSecond;
            }
            return base.Update();
        }

        protected override bool HitPlayer(Player other)
        {
            if (_stopedTicks > 1 * Game._updatesPerSecond && other == MyCollider.Owner)
            {
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

        public override void OnExpire(EntityExpiredReason reason)
        {
            if (ShouldDropBrick)
                _originalOwner?.ApplyDebuff(new BrickBuff(_originalOwner));
        }

        public override bool OnCollision(Entity other)
        {
            if (EntityNamesToIgnore.Contains(other.EntityId))
                return false;
            if (base.OnCollision(other) && other is not BlackHoleEntity)
            {
                Speed = 0;
                Height = EntityHeight.Ground;
            }
            return false;
        }
    }
}
