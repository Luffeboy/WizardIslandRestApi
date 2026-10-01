using WizardIslandRestApi.Game.Spells.BasicSpells.TrapSeplls;

namespace WizardIslandRestApi.Game.Spells.Movement
{
    public class TeleportTrap : Spell
    {
        public override SpellType Type { get; set; } = SpellType.Movement;

        public override int CooldownMax { get; protected set; } = (int)(15 * Game._updatesPerSecond);

        public override string Name => "Teleport trap";

        private List<MineEntity> _mineEntities = [];

        public TeleportTrap(Player player) : base(player)
        {
            StandardStats.Size = .75f;
            StandardStats.OtherStatsInt[SpellSpecificStats.SummonQuantity] = 5;
        }

        protected override void OnCast(Vector2 pos, Vector2 mousePos)
        {
            int maxMines = Math.Max(1, StandardStats.OtherStatsInt[SpellSpecificStats.SummonQuantity]);
            if (_mineEntities.Count >= maxMines)
            {
                _mineEntities[0].ForceExpire();
                _mineEntities.RemoveAt(0);
            }
            
            MineEntity mineEntity = new MineEntity(MyPlayer, pos)
            {
                Color = "100, 0, 100",
                CanBeTriggeredByOwner = false,
                Size = StandardStats.Size,
                EntityId = "TeleportMine",
            };
            mineEntity.SetHeight(EntityHeight.Ground);
            mineEntity.Observers.Expired += (sender, reason) =>
            {
                MineEntity mine = (sender as MineEntity)!;
                _mineEntities.Remove(mine);
                if (reason != EntityExpiredReason.CollisionWithPlayer)
                    return;
                MyPlayer.TeleportTo(mine.Pos);
            };

            _mineEntities.Add(mineEntity);
            AddEntityToGame(mineEntity);

            GoOnCooldown();
        }
    }
}
