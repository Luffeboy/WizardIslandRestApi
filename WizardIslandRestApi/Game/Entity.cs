using WizardIslandRestApi.Game.Physics;
namespace WizardIslandRestApi.Game
{
    public abstract class Entity : IUpdateable
    {
        public Vector2 Pos { get; set; } = new Vector2();
        public string Color { get; set; } = "0, 0, 0";
        public float Size { get; set; } = 1.0f;
        public Collider MyCollider { get; } = new Collider();
        public Entity(Player owner)
        {
            MyCollider.Size = Size;
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
