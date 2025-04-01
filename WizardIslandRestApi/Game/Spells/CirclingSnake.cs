namespace WizardIslandRestApi.Game.Spells
{
    public class CirclingSnake : Spell
    {
        public override string Name { get { return "Fire-ball"; } }
        private float _damage = 5;
        private float _knockback = 1.5f;
        public override int CooldownMax { get; protected set; } = (int)(1.5f * Game._updatesPerSecond);
        public CirclingSnake(Player player) : base(player)
        {
        }
        public override void OnCast(Vector2 mousePos)
        {
            var dir = (mousePos - MyPlayer.Pos).Normalized();
            float size = .5f;
            GetCurrentGame().Entities.Add(new SimpleSpellEntity(MyPlayer)
            {
                Pos = MyPlayer.Pos + dir * (MyPlayer.Size + size + .1f),
                Dir = dir,
                Speed = 2f,
                Color = "255, 0, 0",
                Size = size,
                TicksUntilDeletion = 90,
                Damage = _damage,
                Knockback = _knockback,
            });
            GoOnCooldown();
        }
    }
    class CirclingSnakePart : CantHitOwnerAtStartSpellEntity
    {
        public CirclingSnakePart(Player owner, int ticksUntilDeletion) : base(owner, ticksUntilDeletion)
        {
        }

        public override int TicksUntillCanHitOwner { get; set; } = 10;

        public override void ReTarget(Vector2 pos)
        {
            throw new NotImplementedException();
        }

        public override bool Update()
        {
            throw new NotImplementedException();
        }

        protected override bool HitPlayer(Player other)
        {
            throw new NotImplementedException();
        }
    }
}
