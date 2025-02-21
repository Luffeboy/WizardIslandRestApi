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
    }
}
