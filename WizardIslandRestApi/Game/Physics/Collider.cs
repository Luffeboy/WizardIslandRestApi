using WizardIslandRestApi.Game.Spells;

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
            float fullSizeSqr = (Size + other.Size) * (Size + other.Size);

            Vector2 moveDir = Pos - PreviousPos;
            float moveDirLengthSqr = moveDir.LengthSqr();

            // Handle stationary case
            if (moveDirLengthSqr == 0)
            {
                return (Pos - other.Pos).LengthSqr() <= fullSizeSqr;
            }

            Vector2 circleToLineStart = other.Pos - PreviousPos;
            float projectionLength = circleToLineStart.Dot(moveDir);
            projectionLength = Math.Max(0, Math.Min(projectionLength, moveDirLengthSqr));

            Vector2 closestPoint = PreviousPos + moveDir * (projectionLength / moveDirLengthSqr);
            float distanceToClosestPoint = (other.Pos - closestPoint).LengthSqr();

            return distanceToClosestPoint <= fullSizeSqr;
        }
    }
}
