using WizardIslandRestApi.Game.Spells.Debuffs;

namespace WizardIslandRestApi.Game.Spells.Ultimates
{
    interface IGiveCooldownReductionOnCast
    {
        public void Activate(float cooldownMultiplier, int buffDuration);
    }
    public class ReloadSpells : Spell, IGiveCooldownReductionOnCast
    {
        private float _cooldownMultiplier = 1f;
        public override string Name => "Reloaded";
        public ReloadSpells(Player player) : base(player)
        {
            Type = SpellType.Ultimate;
            Tags.Add(SpellTags.CanGiveCooldownReductionOnCast);
        }
        public override int CooldownMax { get; protected set; } = 999 * Game._updatesPerSecond;
        public override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            var spells = MyPlayer.GetSpells();
            var cooldownsSummed = 0;
            foreach (var spell in spells)
            {
                if (spell.CanCast || spell.Type == SpellType.Ultimate)
                    continue;
                cooldownsSummed += spell.CurrentCooldown - GetCurrentGameTick();
                spell.CurrentCooldown = -999;
            }

            CooldownMax = Math.Max(cooldownsSummed, Game._updatesPerSecond); // min 1 sec cooldown
            GoOnCooldown();

            if (_cooldownMultiplier < 1f)
                MyPlayer.ApplyDebuff(new CooldownMultiplierBuff(MyPlayer)
                {
                    CooldownMultiplier = _cooldownMultiplier,
                    TicksTillRemoval = StandardStats.BuffAndDebuffTime,
                });
        }

        public void Activate(float cooldownMultiplier, int buffDuration)
        {
            StandardStats.BuffAndDebuffTime = Math.Max(StandardStats.BuffAndDebuffTime, buffDuration);
            _cooldownMultiplier *= cooldownMultiplier;

        }
    }
}
