using WizardIslandRestApi.Game.Interfaces;
using WizardIslandRestApi.Game.Spells.ExtraEntities;
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
            StandardStats.Size = .5f;
            StandardStats.Speed = 2;
            StandardStats.Range = 3 * StandardStats.Speed;
        }
        public override void OnCast(Vector2 pos, Vector2 mousePos)
        {
            float distanceBetweenFireballs = 8;
            int fireballs = 8;
            Vector2 fwd = (mousePos - MyPlayer.Pos).Normalized();
            Vector2 normal = (mousePos - MyPlayer.Pos).Normal().Normalized();

            for (int i = 0; i < fireballs; i++)
            {
                float half = i - fireballs / 2 + .5f;
                Vector2 fireballStartPos = normal * distanceBetweenFireballs * half + MyPlayer.Pos;
                // move it back a little
                fireballStartPos -= fwd * (MathF.Abs(half) * distanceBetweenFireballs / 4);
                fireballStartPos += fwd * (MyPlayer.Size + StandardStats.Size + .6f);
                var dir = (mousePos - fireballStartPos).Normalized();
                GetCurrentGame().Entities.Add(new SimpleSpellEntity(MyPlayer, fireballStartPos)
                {
                    Dir = dir,
                    Speed = StandardStats.Speed,
                    Color = "255, 0, 0",
                    Size = StandardStats.Size,
                    TicksUntilDeletion = StandardStats.GetLifetime(),
                    Damage = StandardStats.Damage,
                    Knockback = StandardStats.Knockback,
                    EntityId = "FireBall",
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
