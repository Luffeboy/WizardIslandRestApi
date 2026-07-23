using WizardIslandRestApi.Game.Interfaces;
using WizardIslandRestApi.Game.Spells.ExtraEntities;
using WizardIslandRestApi.Game.Spells.SpellHelpers;
namespace WizardIslandRestApi.Game.Spells.BasicSpells
{
    public class FireBurst : Spell, ISetCooldownMax
    {
        public override string Name { get { return "Fire burst"; } }
        public override int CooldownMax { get; protected set; } = (int)(7.5f * Game._updatesPerSecond);
        public FireBurst(Player player) : base(player)
        {
            StandardStats.Damage = 1;
            StandardStats.Knockback = 1.1f;
            StandardStats.Speed = 2;
            StandardStats.Range = 3 * StandardStats.Speed;
            StandardStats.Size = .5f;

            StandardStats.OtherStatsInt.Add(SpellSpecificStats.ProjectileQuantity, 8);
            StandardStats.OtherStatsInt.Add(SpellSpecificStats.ProjectileBurst, 1);
            StandardStats.OtherStatsInt.Add(SpellSpecificStats.BurstDelay, Game._updatesPerSecond / 4);
            StandardStats.OtherStatsInt.Add(SpellSpecificStats.TargetHitCount, 2);

            Tags.Add(SpellTags.Projectile);
        }
        public override void OnCast(Vector2 pos, Vector2 mousePos)
        {
            float size = StandardStats.Size;
            float distanceBetweenFireballs = 5f + size * 2;
            int fireballs = StandardStats.OtherStatsInt[SpellSpecificStats.ProjectileQuantity];
            Vector2 fwd = (mousePos - MyPlayer.Pos).Normalized();
            Vector2 normal = (mousePos - MyPlayer.Pos).Normal().Normalized();
            ProjectileHelper.CastSpellWithBurst(this, pos, (spawnPos, iteration) =>
            {
                Dictionary<Player, int> spellTargetsHit = new Dictionary<Player, int>();
                for (int i = 0; i < fireballs; i++)
                {
                    float half = i - fireballs / 2 + .5f;
                    Vector2 fireballStartPos = normal * distanceBetweenFireballs * half + spawnPos;
                    // move it back a little
                    fireballStartPos -= fwd * (MathF.Abs(half) * distanceBetweenFireballs / 4);
                    fireballStartPos += fwd * (MyPlayer.Size + size + .6f);
                    var dir = (mousePos - fireballStartPos).Normalized();
                    GetCurrentGame().Entities.Add(new SimpleSpellEntityWithListOfPlayersToIgnore(MyPlayer, fireballStartPos, spellTargetsHit)
                    {
                        Dir = dir,
                        Speed = StandardStats.Speed,
                        Color = "255, 0, 0",
                        Size = size,
                        TicksUntilDeletion = StandardStats.GetLifetime(),
                        Damage = StandardStats.Damage,
                        Knockback = StandardStats.Knockback,
                        EntityId = "FireBall",
                        CantHitSameTypeOfEntityFromSamePlayer = false,
                        MaxHitCountPerPlayer = StandardStats.OtherStatsInt[SpellSpecificStats.TargetHitCount],
                    });
                }
            });

            GoOnCooldown();
        }

        public void SetCooldownMax(int cooldownMax)
        {
            CooldownMax = cooldownMax;
        }
    }
}
