using WizardIslandRestApi.Game.Spells.ExtraEntities;
using WizardIslandRestApi.Game.Spells.SpellHelpers;

namespace WizardIslandRestApi.Game.Spells.BasicSpells
{
    public class SpiralBeam : Spell
    {
        public override string Name => "Spiral beam";

        public override int CooldownMax { get; protected set; } = (int)(20 * Game._updatesPerSecond);
        
        public SpiralBeam(Player player) : base(player)
        {
            StandardStats.Damage = 1;
            StandardStats.Speed = .1f;
            StandardStats.Knockback = .4f;
            StandardStats.SummonLifetime = (int)(10 * Game._updatesPerSecond);
            StandardStats.Range = 10;
            StandardStats.Size = .5f;
            StandardStats.OtherStatsInt.Add(SpellSpecificStats.SummonQuantity, 1);
            StandardStats.OtherStatsInt.Add(SpellSpecificStats.EntityHealth, 3);
            StandardStats.OtherStatsFloat.Add(SpellSpecificStats.RotationSpeed, .1f);
            StandardStats.OtherStatsFloat.Add(SpellSpecificStats.ProjectileAngle, MathF.PI / 8);
        }

        protected override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            Random r = new();
            var dirs = ProjectileHelper.GetProjectileDirections(this, mousePos - startPos, StandardStats.OtherStatsInt[SpellSpecificStats.SummonQuantity]);
            for (int i = 0; i < dirs.Length; i++)
                GetCurrentGame().Entities.Add(new SpiralBeamEntity(MyPlayer, StandardStats.SummonLifetime, startPos, StandardStats.Range, StandardStats.Size)
                {
                    Dir = dirs[i],
                    Speed = StandardStats.Speed,
                    Damage = StandardStats.Damage,
                    Knockback = StandardStats.Knockback,
                    RotationSpeed = StandardStats.OtherStatsFloat[SpellSpecificStats.RotationSpeed],
                    Health = StandardStats.OtherStatsInt[SpellSpecificStats.EntityHealth],
                    CurrentAngle = (float)(r.NextDouble() * Math.PI * 2),
                });

            GoOnCooldown();
        }
    }

    public class SpiralBeamEntity : CantHitOwnerAtStartSpellEntity
    {
        public Vector2 Dir { get; set; }

        public float Speed { get; set; }

        public float RotationSpeed { get; set; } = .2f;

        public float RangeMin { get; set; } = 0;

        public float RangeMax { get; set; }

        public int Health { get; set; } = 1;

        public float Damage { get; set; }

        public float Knockback { get; set; }

        public float CurrentAngle { get; set; }

        private ShadowEntity[] _beamVisual;
        Vector2 _beamDirection;
        const int _beamRemoveTime = 1 * Game._updatesPerSecond;

        public SpiralBeamEntity(Player owner, int ticksUntilDeletion, Vector2 startPos, float rangeMax, float size) : base(owner, ticksUntilDeletion, startPos)
        {
            Color = "50,50,255";
            var game = owner._game;
            Size = size;
            RangeMax = rangeMax;
            _beamVisual = new ShadowEntity[Math.Max((int)MathF.Ceiling(RangeMax), 1)];
            for (int i = 0; i < _beamVisual.Length; i++)
            {
                game.Entities.Add(_beamVisual[i] = new ShadowEntity()
                {
                    Pos = Pos,
                    Color = "30,30,128",
                    Size = Size,
                    TicksUntilDeletion = 99999,
                });
            }
        }

        public override void ReTarget(Vector2 pos)
        {
            Dir = (pos - Pos).Normalized();
            _ticksUntilDeletion = _ticksUntilDeletionMax;
            for (int i = 0; i < _beamVisual.Length; i++)
                _beamVisual[i].TicksUntilDeletion = _ticksUntilDeletion;
        }

        public override bool Update()
        {
            Pos += Dir * Speed;
            CurrentAngle += RotationSpeed;
            float currentFurthestDistance = 0;
            if (_ticksUntilDeletion > _beamRemoveTime)
                currentFurthestDistance = (1f - (float)(_ticksUntilDeletion - _beamRemoveTime) / (float)(_ticksUntilDeletionMax - _beamRemoveTime));
            else
            {
                currentFurthestDistance = (float)_ticksUntilDeletion / (float)_beamRemoveTime;
                RotationSpeed *= 1.05f;
            }
            currentFurthestDistance *= RangeMin + (RangeMax - RangeMin);
            _beamDirection = new Vector2(MathF.Cos(CurrentAngle), MathF.Sin(CurrentAngle));
            Vector2 _furthestBeamDistance = _beamDirection * currentFurthestDistance;
            
            // move collider
            MyCollider.Pos = Pos + _furthestBeamDistance;
            MyCollider.Pos = Pos;

            // show visual
            for (int i = 0; i < _beamVisual.Length; i++)
                _beamVisual[i].Pos = Pos + _furthestBeamDistance * ((float)i/(float)_beamVisual.Length);

            return base.Update();
        }

        public override bool OnCollision(Entity other)
        {
            if (!base.OnCollision(other) || other is SpiralBeamEntity)
                return false;
            // delete if hit right in the middle
            float distanceSqr = (other.Pos - Pos).LengthSqr();
            float sizeSqr = (other.Size + Size) * (other.Size + Size);
            if (distanceSqr > sizeSqr)
                return false;
            return --Health < 0;
        }

        protected override bool HitPlayer(Player other)
        {
            other.TakeDamage(Damage, MyCollider.Owner);
            other.ApplyKnockback(_beamDirection, Knockback);
            return false;
        }

        public override void OnExpire(EntityExpiredReason reason)
        {
            base.OnExpire(reason);
            for (int i = 0; i < _beamVisual.Length; i++)
                _beamVisual[i].TicksUntilDeletion = -1;
        }
    }
}
