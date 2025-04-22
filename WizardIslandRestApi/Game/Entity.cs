using WizardIslandRestApi.Game.Physics;
namespace WizardIslandRestApi.Game
{
    public enum EntityHeight
    {
        Normal,
        Ground
    }
    public abstract class Entity : IUpdateable
    {
        private float _size;
        private Vector2 _pos;
        public EntityHeight Height {  get; protected set; } = EntityHeight.Normal;
        public Vector2 Pos { get { return _pos; } set { _pos = value; if (MyCollider != null) MyCollider.Pos = value; } }
        public string Color { get; set; } = "0, 0, 0";
        public float Size { get { return _size; } set { _size = value; if (MyCollider != null) MyCollider.Size = _size; } }
        public Collider MyCollider { get; protected set; } = new Collider(); // may be null :)
        public bool Visable { get; set; } = true;
        public string EntityId { get; set; } = "";
        public Entity(Player owner)
        {
            MyCollider.Owner = owner;
        }
        public void TeleportTo(Vector2 pos)
        {
            Pos = pos;
            Pos = pos; // also set the previous pos
        }

        public abstract void ReTarget(Vector2 pos);

        /// <summary>
        /// returns true, if this entity should be deleted
        /// </summary>
        /// <returns></returns>
        public abstract bool Update();
        /// <summary>
        /// returns true, if this entity should be deleted
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual bool OnCollision(Entity other)
        {
            return other.Height != EntityHeight.Ground;
        }
        /// <summary>
        /// returns true, if this entity should be deleted
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract bool OnCollision(Player other);
    }
}
