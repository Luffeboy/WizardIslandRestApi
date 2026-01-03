namespace WizardIslandRestApi.Game.Interfaces
{
    public interface ISetCooldownMax
    {
        /// <summary>
        /// A method to update a given spells cooldown max... since it is normally protected
        /// </summary>
        /// <returns>True, if the entity should be destroyed</returns>
        public void SetCooldownMax(int cooldownMax);
    }
}
