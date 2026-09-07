namespace WizardIslandRestApi.Helpers.Debugging
{
    public class ConsoleErrorLogger : ErrorLogger
    {
        public override void LogError(Exception e, DateTime timestamp)
        {
            Console.WriteLine($"Error at {timestamp}:");
            Console.WriteLine($"{e.Message}");
        }
    }
}
