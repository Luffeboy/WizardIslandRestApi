namespace WizardIslandRestApi.Game.Physics
{
    /// <summary>
    /// All colliders are circle colliders
    /// </summary>
    public class Collider
    {
        public float Size { get; set; }
        private Vector2 _pos = new Vector2();
        public Vector2 Pos { get { return _pos; } set { PreviousPos = _pos; _pos = value;  } }
        public Vector2 PreviousPos { get; private set; }
        public Player? Owner {  get; set; }
        public Collider(Vector2 pos)
        {
            _pos = pos;
            PreviousPos = pos;
        }
        public bool CheckCollision(Collider other)
        {
            // simple check
            Vector2 diff = Pos - other.Pos;
            float size = Size + other.Size;
            if (diff.x * diff.x + diff.y * diff.y < size * size)
                return true;
            // raycast check
            // Step 1: Calculate the closest point on the line to the circle's center
            // Calculate the direction vector of the line (normalized)

            float maxLength = (Pos - PreviousPos).Length();
            Vector2 lineDir = (Pos - PreviousPos) / maxLength;

            // Find the projection of the circle's center onto the line
            // Vector from lineStart to the circle's center
            Vector2 circleToLineStart = other.Pos - PreviousPos;

            // Projection of circleToLineStart onto the line direction
            float projectionLength = circleToLineStart.Dot(lineDir);
            projectionLength = Math.Max(0, Math.Min(projectionLength, maxLength));
            // Calculate the closest point on the line
            Vector2 closestPoint = PreviousPos + lineDir * projectionLength;

            // Step 2: Calculate the distance between the circle's center and the closest point on the line
            float distanceToClosestPoint = (other.Pos - closestPoint).LengthSqr();

            // Step 3: Check if the distance is less than or equal to the circle's radius
            return distanceToClosestPoint < (other.Size + Size) * (other.Size + Size);
        }
    }
}
