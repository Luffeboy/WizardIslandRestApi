namespace WizardIslandRestApi.Game.Spells
{
    public class Link : Spell
    {
        public Link(Player player) : base(player)
        {
        }

        public override int CooldownMax { get; protected set; } = 15 * Game._updatesPerSecond;

        public override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            GetCurrentGame().Entities.Add(new LinkEntity(MyPlayer, startPos, GetCurrentGame()) 
            {
                Dir = (mousePos - startPos).Normalized(),
                Speed = (float)30 / Game._updatesPerSecond,
                TicksUntilDeletion = Game._updatesPerSecond * 3,
                Size = .5f,
                PullDuration = 3 * Game._updatesPerSecond,
            });
            GoOnCooldown();
        }
    }
    public class LinkEntity : Entity
    {
        private Player _owner;
        private bool _hitAPlayer = false;
        private Player? _hitPlayer = null;

        private int _ticksUntilDeletion;
        private int _ticksUntilDeletionMax;
        private ShadowEntity[] _linksVisual = new ShadowEntity[24];
        public Vector2 Dir {  get; set; }
        public float Speed {  get; set; }
        public float PullAmount { get; set; } = .05f;
        public int PullDuration { get; set; }
        public int TimeUntilCanHitOwner { get; set; } = 5;
        public int TicksUntilDeletion
        {
            get { return _ticksUntilDeletion; }
            set
            {
                _ticksUntilDeletionMax = value;
                _ticksUntilDeletion = _ticksUntilDeletionMax;
            }
        }
        public LinkEntity(Player owner, Vector2 startPos, Game game) : base(owner, startPos)
        {
            _owner = owner;
            Color = "255,200,0";
            for (int i = 0; i < _linksVisual.Length; i++)
                game.Entities.Add(_linksVisual[i] = new ShadowEntity() { Color = Color, EntityId = "LinkLink", Size = .3f, TicksUntilDeletion = 9999 });
        }

        public override bool OnCollision(Player other)
        {
            if (other == MyCollider.Owner)
                return false;
            _hitPlayer = other;
            other.TakeDamage(0, MyCollider.Owner);
                _ticksUntilDeletion = PullDuration;
            _hitAPlayer = true;
            return false;
        }
        public override bool OnCollision(Entity other)
        {
            if (base.OnCollision(other))
                Speed *= .5f;
            return false;
        }

        public override void ReTarget(Vector2 pos)
        {
            Dir = (pos - Pos).Normalized();
            _ticksUntilDeletion = _ticksUntilDeletionMax;
            _owner = MyCollider == null ? _owner : MyCollider.Owner;
        }

        public override bool Update()
        {
            // move forward or pull other player
            if (_hitPlayer != null)
            {
                // pull
                Pos = _hitPlayer.Pos;
                var pullDir = (_owner.Pos - Pos);
                var len = pullDir.Length();
                if (len < 1)
                    len = 1;
                _hitPlayer.ApplyKnockback(pullDir / len, PullAmount * len/5);
            } else
            {
                // move
                Pos += Dir * Speed;
            }
            if (--_ticksUntilDeletion < 0)
            {
                for (int i = 0; i < _linksVisual.Length; i++)
                    _linksVisual[i].TicksUntilDeletion = -1;
                return true;
            }
            if (_hitAPlayer && MyCollider != null)
                MyCollider = null; // once this has hit a player, it can no longer hit anyone/anything else
            // move links to make it look like a chain
            var dir = (Pos - _owner.Pos);
            var distBetween = dir.Length();
            dir /= distBetween;
            distBetween /= _linksVisual.Length + 1;
            for (int i = 0; i < _linksVisual.Length; i++)
                _linksVisual[i].Pos = _owner.Pos + dir * distBetween * (i + 1);
            return false;
        }
    }
}
