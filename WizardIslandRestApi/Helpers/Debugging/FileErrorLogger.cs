namespace WizardIslandRestApi.Helpers.Debugging
{
    public class FileErrorLogger : ErrorLogger
    {
        private readonly string _filePath = "Exceptions";
        public override void LogError(Exception e, DateTime timestamp)
        {
            if (!Directory.Exists(_filePath))
                Directory.CreateDirectory(_filePath);
            
            string path = _filePath + "/ErrorLog_" + timestamp.ToString("yyyy-MM-dd") + "_" + timestamp.Ticks + ".txt";
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine($"Error at {timestamp}:");
                writer.WriteLine($"{e.Message}");
                writer.WriteLine($"-------------------------------------");
                writer.WriteLine($"{e.StackTrace}");
                writer.WriteLine();
            }
        }
    }
}
