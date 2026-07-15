using WizardIslandRestApi.Game.Physics;
using WizardIslandRestApi.Game.Spells.BasicSpells;
using WizardIslandRestApi.Game.Spells.ExtraEntities;

namespace WizardIslandRestApi.Game.Spells.Ultimates
{
    public class Railgun : Spell
    {

        public Railgun(Player player) : base(player)
        {
            Type = SpellType.Ultimate;
            StandardStats.Damage = 15;
            StandardStats.Knockback = 4.0f;
            StandardStats.Range = 30;
            StandardStats.OtherStatsInt.Add(SpellSpecificStats.ActivationDelay, Game._updatesPerSecond / 4);
            

            Tags.Add(SpellTags.Projectile);
        }

        public override int CooldownMax { get; protected set; } = (int)(25 * Game._updatesPerSecond);

        public override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            Vector2 spellDir = mousePos - startPos;
            float spellLen = StandardStats.Range;
            int delay = StandardStats.OtherStatsInt[SpellSpecificStats.ActivationDelay];
            //float spellLen = spellDir.Length();
            spellDir.Normalize();
            float forwardAngle = HomingBoltEntity.GetAngleFromDirection(spellDir);
            ShadowEntity[] spells = new ShadowEntity[(int)(spellLen + 1)];
            for (int i = 0; i < spells.Length; i++)
                GetCurrentGame().Entities.Add(spells[i] = new ShadowEntity() 
                { 
                    Color = "255,255,0", Size = .5f, EntityId = "RailgunPartical", 
                    Pos = startPos + spellDir * i, ForwardAngle = forwardAngle, 
                    TicksUntilDeletion = delay + 3,
                });

            GetCurrentGame().ScheduleAction(delay, () =>
            {
                for (int i = 0; i < spells.Length; i++)
                {
                    spells[i].Color = "255,255,255";
                    spells[i].EntityId = "RailgunParticalExplosion";
                }
                var collider = new Collider(startPos);
                collider.Pos = startPos + spellDir * spellLen;
                // check player hits
                foreach (Player player in GetCurrentGame().Players.Values)
                {
                    if (player == MyPlayer)
                        continue;
                    if (collider.CheckCollision(player.MyCollider))
                    {
                        player.TakeDamage(StandardStats.Damage, MyPlayer);
                        player.ApplyKnockback(spellDir, StandardStats.Knockback);
                    }
                }
            });
            GoOnCooldown();
        }
    }
}
