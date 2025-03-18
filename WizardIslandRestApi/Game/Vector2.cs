namespace WizardIslandRestApi.Game
{
    public struct Vector2
    {
        public float x { get; set; }
        public float y { get; set; }
        public Vector2() { x = 0; y = 0; }
        public Vector2(float x, float y) { this.x = x; this.y = y; }

        public float LengthSqr()
        {
            return x * x + y * y;
        }
        public float Length()
        {
            float lenSqr = x * x + y * y;
            //if (lenSqr == 0) return 0;
            return MathF.Sqrt(lenSqr);
        }
        public float Dot(Vector2 other) 
        {
            return x * other.x + y * other.y;
        }

        public Vector2 Normal()
        {
            return new Vector2(-y, x);
        }
        public void Normalize()
        {
            if (x == 0 && y == 0)
                return;
            float len = Length();
            x /= len;
            y /= len;
        }
        public Vector2 Normalized()
        {
            if (x == 0 && y == 0)
                return new Vector2();
            float len = Length();
            return new Vector2(x / len, y / len);
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
            => new Vector2(a.x + b.x, a.y + b.y);
        public static Vector2 operator -(Vector2 a, Vector2 b)
            => new Vector2(a.x - b.x, a.y - b.y);
        public static Vector2 operator *(Vector2 a, float b)
            => new Vector2(a.x * b, a.y * b);

        public override string ToString()
        {
            return "x: " + x + ", y: " + y;
        }

        public static Vector2 Lerp(Vector2 startPoint, Vector2 endPoint, float t)
        {
            return startPoint + (endPoint - startPoint) * t;
        }
        public static Vector2 CalculatePointOnSpline(Vector2 startPoint, Vector2 endPoint, Vector2 controlPoint, float t)
        {
            // Bézier curve formula
            float oneMinusT = 1 - t;
            float x = (oneMinusT * oneMinusT) * startPoint.x + 2 * oneMinusT * t * controlPoint.x + (t * t) * endPoint.x;
            float y = (oneMinusT * oneMinusT) * startPoint.y + 2 * oneMinusT * t * controlPoint.y + (t * t) * endPoint.y;

            return new Vector2(x, y);
        }
    }
}
