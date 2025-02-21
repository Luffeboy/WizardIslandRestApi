namespace WizardIslandRestApi.Game
{
    public interface IUpdateable
    {
        /// <summary>
        /// A method to update a given entity
        /// </summary>
        /// <returns>True, if the entity should be destroyed</returns>
        bool Update();
    }
}
