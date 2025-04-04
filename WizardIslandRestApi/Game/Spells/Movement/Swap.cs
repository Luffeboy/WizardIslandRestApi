namespace WizardIslandRestApi.Game.Spells.Movement
{
    public class Swap : Spell
    {
        private float _range = 50.0f;
        public override SpellType Type { get; set; } = SpellType.Movement;
        public override int CooldownMax { get; protected set; } = 8 * Game._updatesPerSecond;
        public Swap(Player player) : base(player)
        {
        }
        public override void OnCast(Vector2 mousePos)
        {
            var dir = (mousePos - MyPlayer.Pos).Normalized();
            int ticksUntillDeletion = 10; 
            GetCurrentGame().Entities.Add(new SwapEntity(MyPlayer, ticksUntillDeletion)
            {
                Pos = MyPlayer.Pos,
                Dir = dir,
                Speed = 4,
                Color = "0, 0, 0",
                Size = 1.0f
            });
            GoOnCooldown();
        }
    }
    public class SwapEntity : CantHitOwnerAtStartSpellEntity
    {
        public override int TicksUntillCanHitOwner { get; set; } = 30;
        public Vector2 Dir {  get; set; }
        public float Speed { get; set; }

        public SwapEntity(Player owner, int ticksUntilDeletion) : base(owner, ticksUntilDeletion)
        {
            Height = EntityHeight.Ground; // just so we don't destroy all the other entities
        }


        public override bool Update()
        {
            Pos += Dir * Speed;
            _ticksUntilDeletion--;
            if (_ticksUntilDeletion < 0)
            {
                MyCollider.Owner.Pos = Pos;
                return true;
            }
            return false;
        }

        protected override bool HitPlayer(Player other)
        {
            Vector2 pos = other.Pos;
            other.Pos = MyCollider.Owner.Pos;
            MyCollider.Owner.Pos = pos;

            Vector2 vel = other.Vel;
            other.Vel = MyCollider.Owner.Vel;
            MyCollider.Owner.Vel = vel;
            return true;
        }
        public override bool OnCollision(Entity other)
        {
            if (base.OnCollision(other))
            {
                Vector2 pos = other.Pos;
                other.Pos = MyCollider.Owner.Pos;
                MyCollider.Owner.Pos = pos;
                return true;
            }
            return false;
        }
        public override void ReTarget(Vector2 pos)
        {
            Dir = (pos - Pos).Normalized();
        }
    }
}
