using WizardIslandRestApi.Game.Interfaces;
using WizardIslandRestApi.Game.Spells.ExtraEntities;
namespace WizardIslandRestApi.Game.Spells.BasicSpells
{
    public class FireBall : Spell, ISetCooldownMax
    {
        public override string Name { get { return "Fire-ball"; } }
        public override int CooldownMax { get; protected set; } = (int)(2.25f * Game._updatesPerSecond);
        public FireBall(Player player) : base(player)
        {
            StandardStats.Damage = 5;
            StandardStats.Knockback = 1.5f;
            StandardStats.Size = .5f;
        }
        public override void OnCast(Vector2 pos, Vector2 mousePos)
        {
            var dir = (mousePos - pos).Normalized();
            GetCurrentGame().Entities.Add(new SimpleSpellEntity(MyPlayer, pos + dir * (MyPlayer.Size + StandardStats.Size + .1f))
            {
                Dir = dir,
                Speed = 2f,
                Color = "255, 0, 0",
                Size = StandardStats.Size,
                TicksUntilDeletion = 90,
                Damage = StandardStats.Damage,
                Knockback = StandardStats.Knockback,
                EntityId = "FireBall"
            });
            GoOnCooldown();
        }

        public void SetCooldownMax(int cooldownMax)
        {
            CooldownMax = cooldownMax;
        }
    }
}
