using WizardIslandRestApi.Game.Spells.ExtraEntities;
namespace WizardIslandRestApi.Game.Spells.BasicSpells
{
    public class FireBall : Spell
    {
        public override string Name { get { return "Fire-ball"; } }
        private float _damage = 5;
        private float _knockback = 1.5f;
        public override int CooldownMax { get; protected set; } = (int)(2.25f * Game._updatesPerSecond);
        public FireBall(Player player) : base(player)
        {
        }
        public override void OnCast(Vector2 pos, Vector2 mousePos)
        {
            var dir = (mousePos - pos).Normalized();
            float size = .5f;
            GetCurrentGame().Entities.Add(new SimpleSpellEntity(MyPlayer, pos + dir * (MyPlayer.Size + size + .1f))
            {
                Dir = dir,
                Speed = 2f,
                Color = "255, 0, 0",
                Size = size,
                TicksUntilDeletion = 90,
                Damage = _damage,
                Knockback = _knockback,
                EntityId = "FireBall",
                EntityIdsToIgnore = ["FireBall"]
            });
            GoOnCooldown();
        }
    }
}
