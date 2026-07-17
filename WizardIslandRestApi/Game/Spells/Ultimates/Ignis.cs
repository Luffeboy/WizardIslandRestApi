using WizardIslandRestApi.Game.Interfaces;

namespace WizardIslandRestApi.Game.Spells.Ultimates
{
    public class Ignis : Spell
    {
        private Spell[]? _previousSpells = null;
        private Spell[] _fireSpells;
        public Ignis(Player player) : base(player)
        {
            Type = SpellType.Ultimate;
            if (MyPlayer is null)
                return;
            _fireSpells = new Spell[GetCurrentGame().AllowedSpellCount];
            if (_fireSpells.Length == 0) // should not happen
                return;
            _fireSpells[0] = new BasicSpells.FireBurst(player);
            for (int i = 1; i < _fireSpells.Length - 1; i++)
                _fireSpells[i] = new BasicSpells.FireBall(player);
            _fireSpells[_fireSpells.Length - 1] = this;
            for (int i = 1; i < _fireSpells.Length - 1; i++)
                (_fireSpells[i] as ISetCooldownMax)?.SetCooldownMax((int)(2.5f * _fireSpells[i].CooldownMax));

            // make sure all stats are shared between the spells
            for (int i = 1; i < _fireSpells.Length; i++)
            {
                var statsToCopy = _fireSpells[i].StandardStats;
                StandardStats.Damage = MathF.Max(StandardStats.Damage, statsToCopy.Damage);
                StandardStats.Knockback = MathF.Max(StandardStats.Knockback, statsToCopy.Knockback);
                StandardStats.Size = MathF.Max(StandardStats.Size, statsToCopy.Size);
                StandardStats.Speed = MathF.Max(StandardStats.Speed, statsToCopy.Speed);
                StandardStats.Range = MathF.Max(StandardStats.Range, statsToCopy.Range);

                foreach (var otherFloat in statsToCopy.OtherStatsFloat)
                    if (StandardStats.OtherStatsFloat.ContainsKey(otherFloat.Key))
                        StandardStats.OtherStatsFloat[otherFloat.Key] = MathF.Max(StandardStats.OtherStatsFloat[otherFloat.Key], otherFloat.Value);
                    else
                        StandardStats.OtherStatsFloat.Add(otherFloat.Key, otherFloat.Value);
                foreach (var otherInt in statsToCopy.OtherStatsInt)
                    if (StandardStats.OtherStatsInt.ContainsKey(otherInt.Key))
                        StandardStats.OtherStatsInt[otherInt.Key] = Math.Max(StandardStats.OtherStatsInt[otherInt.Key], otherInt.Value);
                    else
                        StandardStats.OtherStatsInt.Add(otherInt.Key, otherInt.Value);

                _fireSpells[i].SetStandardStats(StandardStats);
            }

            Tags.Add(SpellTags.Projectile);
        }

        public override int CooldownMax { get; protected set; } = 1 * Game._updatesPerSecond;

        public override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            if (_previousSpells is null) // first activation
            {
                _previousSpells = MyPlayer.GetSpells();
                MyPlayer.SetSpells(_fireSpells);
                GoOnCooldown();
            }
            else // second activation / go back to normal spells
            {
                MyPlayer.SetSpells(_previousSpells);
                _previousSpells = null;
                GoOnCooldown();
            }

        }
        public override void FullReset()
        {
            _previousSpells = null;
            base.FullReset();
        }
    }
}
