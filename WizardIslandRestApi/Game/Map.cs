namespace WizardIslandRestApi.Game
{
    public class Map
    {
        public Vector2 Size { get; } = new Vector2(1000, 1000);
        public Vector2 GroundMiddle { get; private set; }
        public float CircleRadius { get; set; } = 50; // radius of ground
        public float CircleInnerRadius { get; set; } = 10; // hole in the middle
        public Map()
        {
            GroundMiddle = new Vector2(Size.x / 2, Size.y / 2);
        }
    }
}
