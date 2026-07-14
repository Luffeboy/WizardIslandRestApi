namespace WizardIslandRestApi.Game.Spells.SpellHelpers
{
    public static class ProjectileHelper
    {
        /// <summary>
        /// Sets the default stats for a projectile.
        /// </summary>
        /// <param name="spell"></param>
        /// <param name="quantity"></param>
        /// <param name="angle"></param>
        /// <param name="burstCount"></param>
        public static void SetProjectileStats(Spell spell, int quantity = 1, float angle = MathF.PI / 8, int burstCount = 1, int burstDelay = Game._updatesPerSecond / 4)
        {
            spell.StandardStats.OtherStatsInt.Add(SpellSpecificStats.ProjectileQuantity, quantity);
            spell.StandardStats.OtherStatsFloat.Add(SpellSpecificStats.ProjectileAngle, angle);
            spell.StandardStats.OtherStatsInt.Add(SpellSpecificStats.ProjectileBurst, burstCount);
            spell.StandardStats.OtherStatsInt.Add(SpellSpecificStats.BurstDelay, burstDelay);
        }

        /// <summary>
        /// Calculates the direction of a projectile based on the position of the player and the mouse position.
        /// </summary>
        /// <param name="spell">must have "SpellSpecificStats.ProjectileQuantity" and "SpellSpecificStats.ProjectileAngle"</param>
        /// <param name="startDir">Doesn't need to be normalized</param>
        /// <returns></returns>
        public static Vector2[] GetProjectileDirections(Spell spell, Vector2 startDir)
        {
            int projectileQuantity = spell.StandardStats.OtherStatsInt[SpellSpecificStats.ProjectileQuantity];
            Vector2[] dirs = new Vector2[projectileQuantity];
            float projectileAngle = spell.StandardStats.OtherStatsFloat[SpellSpecificStats.ProjectileAngle];
            var angle = MathF.Atan2(startDir.y, startDir.x) - (projectileQuantity - 1) / 2 * projectileAngle;
            for (int i = 0; i < projectileQuantity; i++)
            {
                dirs[i] = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                angle += projectileAngle;
            }
            return dirs;
        }

        public static void CastSpellWithBurst(Spell spell, Vector2 startPos, Action<Vector2> actionWithUpdatedStartPosAndIteration)
        {
            CastSpellWithBurst(spell, startPos, (pos, iteration) => { actionWithUpdatedStartPosAndIteration(pos); });
        }
        public static void CastSpellWithBurst(Spell spell, Vector2 startPos, Action<Vector2, int> actionWithUpdatedStartPosAndIteration)
        {
            var player = spell.MyPlayer;
            var burst = spell.StandardStats.OtherStatsInt[SpellSpecificStats.ProjectileBurst];
            int delayAdd = spell.StandardStats.OtherStatsInt[SpellSpecificStats.BurstDelay];
            int delay = -delayAdd;
            Vector2 spawnPos = startPos;
            Vector2 playerLastPos = player.Pos;
            for (int j = 0; j < burst; j++)
            {
                spell.GetCurrentGame().ScheduleAction(delay += delayAdd, () =>
                {
                    spawnPos += player.Pos - playerLastPos;
                    playerLastPos = player.Pos;
                    actionWithUpdatedStartPosAndIteration(spawnPos, j);
                });
            }
        }
    }
}
