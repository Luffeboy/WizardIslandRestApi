namespace WizardIslandRestApi.Game.Physics
{
    /// <summary>
    /// All colliders are circle colliders
    /// </summary>
    public class Collider
    {
        public float Size { get; set; }
        public Vector2 Pos { get; set; }
        public Player Owner {  get; set; }
        public bool CheckCollision(Collider other)
        {
            Vector2 diff = Pos - other.Pos;
            float size = Size + other.Size;
            return diff.x * diff.x + diff.y * diff.y < size * size;
        }
    }
}
