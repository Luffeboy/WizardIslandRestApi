using WizardIslandRestApi.Game.Spells.ExtraEntities;

namespace WizardIslandRestApi.Game.Spells.Movement
{
    public class Rewind : Spell
    {
        public override SpellType Type { get; set; } = SpellType.Movement;

        public override int CooldownMax { get; protected set; } = (int)(15 * Game._updatesPerSecond);

        public override string Name => _rewindData == null ? "Rewind" : "Rewind (activate)";

        private RewindData? _rewindData = null;

        public Rewind(Player player) : base(player)
        {
            StandardStats.SummonLifetime = (int)(7.5f * Game._updatesPerSecond);
        }

        protected override void OnCast(Vector2 pos, Vector2 mousePos)
        {
            if (_rewindData == null)
            {
                CreateRewindData(pos);
                return;
            }
            DoRewind();
            DeleteRewindData();
            GoOnCooldown();
        }

        private void DeleteRewindData()
        {
            if (_rewindData == null)
                return;
            _rewindData.RewindEntityPosition.TicksUntilDeletion = -1;
            _rewindData.RewindEntityTimer.SetRemainingTicks(-1);
            _rewindData = null;
        }

        private void CreateRewindData(Vector2 pos)
        {
            float timerSize = 1f;
            _rewindData = new RewindData()
            {
                RewindEntityPosition = new ShadowEntity()
                {
                    EntityId = "RewindPosition",
                    VisableTo = MyPlayer.Id,
                    Color = "100,100,255",
                    Pos = pos,
                    Size = timerSize,
                    TicksUntilDeletion = StandardStats.SummonLifetime,
                },
                RewindEntityTimer = new TimerEntity(pos, timerSize, StandardStats.SummonLifetime)
                {
                    EntityId = "RewindTimer",
                    VisableTo = MyPlayer.Id,
                },
                Vel = MyPlayer.Vel,
                Health = MyPlayer.Stats.Health,
            };
            _rewindData.RewindEntityPosition.Observers.Expired += (entity, reason) =>
            {
                DeleteRewindData();
            };
            GetCurrentGame().Entities.Add(_rewindData.RewindEntityPosition);
            GetCurrentGame().Entities.Add(_rewindData.RewindEntityTimer);
        }

        private void DoRewind()
        {
            if (MyPlayer.IsDead || _rewindData == null)
                return;
            MyPlayer.TeleportTo(_rewindData.RewindEntityPosition.Pos);
            MyPlayer.Vel = _rewindData.Vel;
            if (MyPlayer.Stats.Health < _rewindData.Health)
                MyPlayer.Stats.Health = _rewindData.Health;
        }

        private class RewindData
        {
            public ShadowEntity RewindEntityPosition { get; set; }
            public TimerEntity RewindEntityTimer { get; set; }
            public Vector2 Vel { get; set; }
            public int Health { get; set; }
        }
    }

    public class TimerEntity : Entity
    {
        private float _maxSize;
        private int _ticksRemaining;
        private int _ticksMax;
        private int _sizeApexTicks;

        public TimerEntity(Vector2 startPos, float maxSize, int ticksDuration) : base(null, startPos)
        {
            MyCollider = null;
            EntityId = "Timer";
            Color = "0,0,255";
            Size = 0.01f;
            _maxSize = maxSize;
            SetRemainingTicks(ticksDuration);
        }

        public void SetRemainingTicks(int ticks)
        {
            _ticksMax = Math.Max(ticks, 2);
            _ticksRemaining = _ticksMax;
            _sizeApexTicks = Math.Min(_ticksRemaining - _ticksRemaining * 5 / 6, Game._updatesPerSecond / 2);
        }
        public override bool Update()
        {
            float size;
            if (_ticksRemaining < _sizeApexTicks)
                size = (float)_ticksRemaining / _sizeApexTicks * _maxSize;
            else
                size = (float)(_ticksMax - _ticksRemaining) / (_ticksMax - _sizeApexTicks) * _maxSize;
            Size = MathF.Max(size, .01f);
            return --_ticksRemaining < 0;
        }

        public override bool OnCollision(Player other)
        {
            return false;
        }

        public override void ReTarget(Vector2 pos)
        {
        }
    }
}
