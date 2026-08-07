namespace WizardIslandRestApi.Game.Spells.ExtraEntities
{
    public class FollowPlayerEntity : Entity
    {
        private Player _player;
        public int TicksTillDeletion { get; set; }
        public Vector2 Offset { get; set; }
        public FollowPlayerEntity(Player player, Vector2 offset, int ticksTillDeletion = Game._gameDuration, int visableTo = -1) : base(player, new Vector2())
        {
            MyCollider = null;
            Offset = offset;
            _player = player;
            Pos = _player.Pos + Offset;
            TicksTillDeletion = ticksTillDeletion;
            VisableTo = visableTo;
            Color = "50,50,50";
            EntityId = "FollowPlayer";
        }

        public override bool OnCollision(Player other)
        {
            return false;
        }

        public override void ReTarget(Vector2 pos)
        { }

        public override bool Update()
        {
            Pos = _player.Pos + Offset;
            return --TicksTillDeletion < 0;
        }
    }
}
