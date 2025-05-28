using WizardIslandRestApi.Game.Spells.Debuffs;

namespace WizardIslandRestApi.Game.Spells.Movement
{
    public class Phoenix : Spell
    {
        private int _currentCooldown;
        public override int CooldownMax { get; protected set; } = 35 * Game._updatesPerSecond;
        public override int CurrentCooldown { get => (MyPlayer.IsInLava && MyPlayer.Stats.Health < MyPlayer.Stats.MaxHealth / 3) ? _currentCooldown : GetCurrentGameTick() + Game._updatesPerSecond*100/3; set => _currentCooldown = value; }
        public Phoenix(Player player) : base(player)
        {
            Type = SpellType.Movement;
        }


        public override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            const int ticksTillRemoval = 3 * Game._updatesPerSecond;
            MyPlayer.ApplyDebuff(new Invulnerability(MyPlayer) { TicksTillRemoval = ticksTillRemoval });
            const int regenAmount = 5;
            const int ticksBetweenRegen = 5;
            MyPlayer.ApplyDebuff(new RegenerationBuff(MyPlayer, ticksTillRemoval, regenAmount, ticksBetweenRegen));
            const float speedMultiplier = 1.5f;
            MyPlayer.ApplyDebuff(new SpeedBuff(MyPlayer)
            {
                TicksTillRemoval = ticksTillRemoval,
                SpeedMultiplier = speedMultiplier,
            });
            GoOnCooldown();
        }
    }
}
