using WizardIslandRestApi.Game.Spells.BasicSpells;
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

            Tags.Add(SpellTags.Projectile);
            Tags.Add(SpellTags.Static);
        }

        public override int CooldownMax { get; protected set; } = 20 * Game._updatesPerSecond;

        public override void OnCast(Vector2 pos, Vector2 mousePos)
        {
            float size = StandardStats.Size;
            Vector2 endPos = mousePos;
            Vector2 dir = mousePos - pos;
            if (dir.LengthSqr() > StandardStats.Range * StandardStats.Range)
            {
                dir = dir.Normalized() * StandardStats.Range;
                endPos = pos + dir;
            }
            var ticksUntilDeletion = (int)(dir.Length() * .75f);
            dir.Normalize();
            Vector2 dirNormal = dir.Normal();
            Vector2 startPos = pos +
                               dir * (MyPlayer.Size + size + .1f) +
                               dirNormal * (MyPlayer.Size + size + .1f);
            float amountToSide = 1.0f;
            GetCurrentGame().Entities.Add(new CrescentMoonEntity(MyPlayer, startPos, endPos, amountToSide)
            {
                Color = "100, 100, 255",
                Size = size,
                TicksUntilDeletionMax = ticksUntilDeletion,
                Damage = StandardStats.Damage,
                Knockback = StandardStats.Knockback,
                AmountToSideMultiplier = amountToSide,
            });
            GetCurrentGame().Entities.Add(new CrescentMoonEntity(MyPlayer, startPos, endPos, -amountToSide)
            {
                Color = "100, 100, 255",
                Size = size,
                TicksUntilDeletionMax = ticksUntilDeletion,
                Damage = StandardStats.Damage,
                Knockback = StandardStats.Knockback,
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
