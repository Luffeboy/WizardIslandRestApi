namespace WizardIslandRestApi.Game.Spells
{
    public class LavaPoolEntity : Entity
    {
        public float MinSize { get; set; }
        public float MaxSize { get; set; }

        private int _ticksUntilDeletion;
        private int _ticksUntilDeletionMax;

        private int _lastHealedTick = -1;
        private int _healCooldown = Game._updatesPerSecond / 3;
        public int TicksUntilDeletion
        {
            get { return _ticksUntilDeletion; }
            set
            {
                _ticksUntilDeletionMax = value;
                _ticksUntilDeletion = _ticksUntilDeletionMax;
            }
        }
        public LavaPoolEntity(Player owner, Vector2 pos) : base(owner, pos)
        {
            Color = "255, 150, 0";
            Height = EntityHeight.Ground;
            EntityId = "LavaPool";
        }

        public override bool OnCollision(Player other)
        {
            if (other == MyCollider.Owner)
            {
                var gameTick = other._game.GameTick;
                if (_lastHealedTick + _healCooldown < gameTick && (other.Pos - Pos).LengthSqr() < Size / 10)
                {
                    other.Heal(Game.LavaDamage * 2.0f);
                    _lastHealedTick = gameTick;
                }
            }
            else
                other.TakeDamage(Game.LavaDamage, MyCollider.Owner);
            return false;
        }
        public override bool OnCollision(Entity other)
        {
            return false;
        }

        public override void ReTarget(Vector2 pos)
        {
        }

        public override bool Update()
        {
            // from 0 to 1
            float amountDone = ((float)(_ticksUntilDeletionMax - _ticksUntilDeletion)) / _ticksUntilDeletionMax;
            // from 0 to 1 to 0
            amountDone = amountDone * 2 - ((int)(amountDone * 2) * (amountDone * 2 % 1 * 2.0f));
            Size = (MaxSize - MinSize) * amountDone + MinSize;
            return _ticksUntilDeletion-- <= 0;
        }
    }
}
