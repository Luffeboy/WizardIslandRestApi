namespace WizardIslandRestApi.Helpers.Debugging
{
    public abstract class ErrorLogger
    {
        public static readonly ErrorLogger Instance =
#if DEBUG
            new ConsoleErrorLogger();
#else
            new FileErrorLogger();
#endif
        public void LogError(Exception e)
            => LogError(e, DateTime.Now);
        public abstract void LogError(Exception e, DateTime timestamp);
    }
}
