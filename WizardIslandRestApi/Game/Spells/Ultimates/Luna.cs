using WizardIslandRestApi.Game.Spells.BasicSpells;
using WizardIslandRestApi.Game.Spells.SpellHelpers;
namespace WizardIslandRestApi.Game.Spells.Ultimates
{
    public class Luna : Spell
    {
        public Luna(Player player) : base(player)
        {
            Type = SpellType.Ultimate;
            StandardStats.Damage = 5;
            StandardStats.Knockback = 1.75f;
            StandardStats.Range = 35;
            StandardStats.Size = 1;
            StandardStats.Speed = 1.35f;

            ProjectileHelper.SetProjectileStats(this, angle: 1.5f);

            Tags.Add(SpellTags.Projectile);
            Tags.Add(SpellTags.Static);
        }

        public override int CooldownMax { get; protected set; } = 20 * Game._updatesPerSecond;

        public override void OnCast(Vector2 pos, Vector2 mousePos)
        {
            Vector2 dir = mousePos - pos;
            float size = StandardStats.Size;
            float range = dir.Length();
            if (range != 0)
                dir /= range;
            float minRange = 3f;
            range = MathF.Max(MathF.Min(range, StandardStats.Range), minRange);
            var ticksUntilDeletion = Math.Max((int)(range / StandardStats.Speed), 1);
            Vector2 dirNormal = dir.Normal();
            Vector2 endPos = pos + dir * range;
            float amountToSide = 1.0f;
            int projectileCount = StandardStats.OtherStatsInt[SpellSpecificStats.ProjectileQuantity];
            float projectileEndpointDifference = 2 * StandardStats.Size * StandardStats.OtherStatsFloat[SpellSpecificStats.ProjectileAngle];
            ProjectileHelper.CastSpellWithBurst(this, pos, (spawnPos, iteration) =>
            {
                for (int i = 0; i < projectileCount; i++)
                {
                    float rangeDiff = (i - projectileCount / 2) * projectileEndpointDifference;
                    Vector2 crescentMoonEndpos = spawnPos + dir * (range + rangeDiff);
                    GetCurrentGame().Entities.Add(new CrescentMoonEntity(MyPlayer, spawnPos, crescentMoonEndpos, amountToSide)
                    {
                        Color = "100, 100, 255",
                        Size = size,
                        TicksUntilDeletionMax = ticksUntilDeletion,
                        Damage = StandardStats.Damage,
                        Knockback = StandardStats.Knockback,
                        AmountToSideMultiplier = amountToSide,
                    });
                    GetCurrentGame().Entities.Add(new CrescentMoonEntity(MyPlayer, spawnPos, crescentMoonEndpos, -amountToSide)
                    {
                        Color = "100, 100, 255",
                        Size = size,
                        TicksUntilDeletionMax = ticksUntilDeletion,
                        Damage = StandardStats.Damage,
                        Knockback = StandardStats.Knockback,
                    });
                }
            });
            GetCurrentGame().ScheduleAction(ticksUntilDeletion, () => 
            {
                // when the two cresent moons hit each other
                GetCurrentGame().Entities.Add(new MeteorEntity(MyPlayer, endPos, GetCurrentGame())
                {
                    Color = "50, 50, 200",
                    FallTime = 5,
                    KnockbackMin = StandardStats.Knockback * 1.4f,
                    KnockbackMax = StandardStats.Knockback * 1.4f,
                    Damage = StandardStats.Damage,
                    EntityId = "Moon",
                    Size = size * 5f,
                });
            });
            GoOnCooldown();
        }
    }
}
