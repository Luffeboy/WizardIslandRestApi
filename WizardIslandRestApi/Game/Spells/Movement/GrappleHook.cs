using WizardIslandRestApi.Game.Spells.ExtraEntities;

namespace WizardIslandRestApi.Game.Spells.Movement
{
    public class GrappleHook : Spell
    {
        private GrappleHookEntity? _hook = null;
        public override SpellType Type { get; set; } = SpellType.Movement;
        public override string Name => "Grapple hook";
        public GrappleHook(Player player) : base(player)
        {
        }

        public override int CooldownMax { get; protected set; } = 10 * Game._updatesPerSecond;

        public override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            if (_hook != null)
            {
                if (_hook.HasHit)
                {
                    _hook.Delete();
                }
            }
            else
            {
                GetCurrentGame().Entities.Add(_hook = new GrappleHookEntity(MyPlayer, (int)(1.5f * Game._updatesPerSecond), startPos, this)
                {
                    Speed = 4,
                    Dir = (mousePos - startPos).Normalized(),
                    Size = .5f
                });
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cooldownMultiplier">Should be 1, unless the hook didn't hit anything</param>
        public void ForceGoOnCooldown(float cooldownMultiplier = 1)
        {
            if (_hook != null)
            {
                _hook.Delete();
                _hook = null;
                var cd = CooldownMax;
                CooldownMax = (int)(CooldownMax * cooldownMultiplier);
                GoOnCooldown();
                CooldownMax = cd;
            }
        }
    }
    public class GrappleHookEntity : EntityPlus
    {
        private GrappleHook _spell;
        public bool HasHit { get { return _hitEntity != null || _hitPlayer != null; } }
        private Entity? _hitEntity = null;
        private Player? _hitPlayer = null;
        private ShadowEntity[] _linksVisual = new ShadowEntity[24];
        public float PullAmount { get; set; } = .05f;
        public GrappleHookEntity(Player owner, int ticksUntilDeletion, Vector2 startPos, GrappleHook spell) : base(owner, ticksUntilDeletion, startPos)
        {
            Height = EntityHeight.Ground;
            TicksUntillCanHitOwner = ticksUntilDeletion; // doesn't make sense, to hook yourself
            _spell = spell;
            Color = "100,100,100";
            EntityId = "GrappleHook";
            for (int i = 0; i < _linksVisual.Length; i++)
                owner._game.Entities.Add(_linksVisual[i] = new ShadowEntity() { Color = Color, EntityId = "GrappleHookLink", Size = .3f, TicksUntilDeletion = 9999 });
        }

        public override bool Update()
        {
            if (HasHit)
            {
                Vector2 targetPos = _hitPlayer == null ? _hitEntity.Pos : _hitPlayer.Pos;

                // go towards
                Pos = targetPos;
                var pullDir = (targetPos - MyCollider.Owner.Pos);
                var len = pullDir.Length();
                if (len < 1)
                    len = 1;
                MyCollider.Owner.ApplyKnockback(pullDir / len, PullAmount * len / 5);
            }
            else
                Pos += Dir * Speed;

            if (--_ticksUntilDeletion < 0)
            {
                for (int i = 0; i < _linksVisual.Length; i++)
                    _linksVisual[i].TicksUntilDeletion = -1;
                Die();
                return true;
            }
            // move links to make it look like a chain
            var dir = (Pos - MyCollider.Owner.Pos);
            var distBetween = dir.Length();
            dir /= distBetween;
            distBetween /= _linksVisual.Length + 1;
            for (int i = 0; i < _linksVisual.Length; i++)
                _linksVisual[i].Pos = MyCollider.Owner.Pos + dir * distBetween * (i + 1);
            return false;
        }
        public override bool OnCollision(Entity other)
        {
            if (base.OnCollision(other) && !HasHit)
            {
                _hitEntity = other;
            }
            return false;
        }

        protected override bool HitPlayer(Player other)
        {
            if (HasHit)
                return false;
            _hitPlayer = other;
            return false;
        }
        public void Delete()
        {
            _ticksUntilDeletion = -1;
        }
        public void Die()
        {
            if (_spell.CanCast)
                _spell.ForceGoOnCooldown(HasHit ? 1.0f : .5f);
        }
    }
}
