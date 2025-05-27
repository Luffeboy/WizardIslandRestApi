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
        public static Vector2 operator /(Vector2 a, float b)
            => new Vector2(a.x / b, a.y / b);

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
        /// <summary>
        /// Gets the angle between p0 and p1.
        /// </summary>
        /// <param name="p0">Starting point</param>
        /// <param name="p1">Target point</param>
        /// <returns></returns>
        public static float AngleBetween(Vector2 p0, Vector2 p1)
        {
            float yDiff = p1.y - p0.y;
            float xDiff = p1.x - p0.x;
            return MathF.Atan2(yDiff, xDiff);
            /*
            // for testing if it works
            Random r = new Random();
            for (int i = 0; i < 360; i++)
            {
                float rad = MathF.PI * i / 180;
                Vector2 p0 = new Vector2((float)(r.NextDouble() * 1000), (float)(r.NextDouble() * 1000));
                Vector2 p1 = p0 + new Vector2(MathF.Cos(rad), MathF.Sin(rad));
                Console.WriteLine(rad + ": " + Vector2.AngleBetween(p0, p1));
            }
            */
        }
    }
}
