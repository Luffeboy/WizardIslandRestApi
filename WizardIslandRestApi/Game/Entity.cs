using WizardIslandRestApi.Game.Physics;
namespace WizardIslandRestApi.Game
{
    public abstract class Entity : IUpdateable
    {
        public Collider collider;
        public abstract bool Update();
    }
}
