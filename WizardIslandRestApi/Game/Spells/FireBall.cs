namespace WizardIslandRestApi.Game.Spells
{
    public class FireBall : Spell
    {
        public override string Name { get { return "Fire-ball"; } }
        private float _damage;
        private float _knockback;
        protected override int CooldownMax { get; set; } = 3 * Game._updatesPerSecond;
        public FireBall(Player player) : base(player)
        {
            //Name = "Fireball";
        }
        public override void OnCast(Vector2 mousePos)
        {
            var dir = (mousePos - MyPlayer.Pos).Normalized();
            float size = .5f;
            GetCurrentGame().Entities.Add(new SimpleSpellEntity(MyPlayer)
            {
                Pos = MyPlayer.Pos + dir * (MyPlayer.Size + size + .1f),
                Dir = dir,
                Speed = 1,
                Color = "255, 0, 0",
                Size = size,
                TicksUntilDeletion = 90,
                Damage = 5,
                Knockback = 1.5f,
            });
            GoOnCooldown();
        }
    }
}
