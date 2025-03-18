using WizardIslandRestApi.Game.Physics;
namespace WizardIslandRestApi.Game
{
    public abstract class Entity : IUpdateable
    {
        private float _size;
        public Vector2 Pos { get; set; } = new Vector2();
        public string Color { get; set; } = "0, 0, 0";
        public float Size { get { return _size; } set { _size = value; MyCollider.Size = _size; } }
        public Collider MyCollider { get; } = new Collider(); // may be null :)
        public Entity(Player owner)
        {
            MyCollider.Owner = owner;
        }

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
        public abstract bool OnCollision(Entity other);
        /// <summary>
        /// returns true, if this entity should be deleted
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract bool OnCollision(Player other);
    }
}
