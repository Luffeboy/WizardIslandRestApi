using WizardIslandRestApi.Game.Interfaces;
using WizardIslandRestApi.Game.Spells.ExtraEntities;
using WizardIslandRestApi.Game.Spells.SpellHelpers;
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
            StandardStats.Speed = 2;
            StandardStats.Range = 3f * StandardStats.Speed;

            Tags.Add(SpellTags.Projectile);
            ProjectileHelper.SetProjectileStats(this, quantity: 1, angle: MathF.PI / 8, burstCount: 1);
        }
        public override void OnCast(Vector2 pos, Vector2 mousePos)
        {
            var burst = StandardStats.OtherStatsInt[SpellSpecificStats.ProjectileBurst];
            int delayAdd = Game._updatesPerSecond / 4;
            int delay = -delayAdd;
            Vector2 startPos = pos;
            Vector2 playerLastPos = MyPlayer.Pos;
            var projectileDirections = ProjectileHelper.GetProjectileDirections(this, mousePos - pos);
            //MyPlayer
            for (int j = 0; j < burst; j++)
            {
                GetCurrentGame().ScheduleAction(delay += delayAdd, () =>
                {
                    startPos += MyPlayer.Pos - playerLastPos;
                    playerLastPos = MyPlayer.Pos;
                    var position = startPos;
                    for (int i = 0; i < projectileDirections.Length; i++)
                    {
                        var dir = projectileDirections[i];
                        GetCurrentGame().Entities.Add(new SimpleSpellEntity(MyPlayer, position + dir * (MyPlayer.Size + StandardStats.Size + .1f))
                        {
                            Dir = dir,
                            Speed = StandardStats.Speed,
                            Color = "255, 0, 0",
                            Size = StandardStats.Size,
                            TicksUntilDeletion = StandardStats.GetLifetime(),
                            Damage = StandardStats.Damage,
                            Knockback = StandardStats.Knockback,
                            EntityId = "FireBall"
                        });
                    }
                });
            }
            GoOnCooldown();
        }

        public void SetCooldownMax(int cooldownMax)
        {
            CooldownMax = cooldownMax;
        }
    }
}
