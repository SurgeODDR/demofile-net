using System;
using System.IO;
using System.Threading.Tasks;
using Serilog;
using cooper_ai.Events;

namespace cooper_ai
{
    internal class Program
    {
        private static readonly string InputDirectory = "/home/azureuser/data/cs2/demofiles/extracted";
        private static readonly string OutputDirectory = "/home/azureuser/data/cs2/demofiles/processed";
        private static readonly string ProcessedLogFilePath = "processed_directories.log";

        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Error() // Set the minimum logging level to Error
                .WriteTo.Console()
                .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day, buffered: false) // Disable buffering to flush logs frequently
                .WriteTo.File("logs/errorlog.txt", rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error)
                .CreateLogger();

            try
            {
                // Load the list of already processed directories
                var processedDirectories = LoadProcessedDirectories();

                foreach (var directoryPath in Directory.GetDirectories(InputDirectory))
                {
                    var directoryName = Path.GetFileName(directoryPath);
                    if (processedDirectories.Contains(directoryName))
                    {
                        // Log.Information("Directory {DirectoryName} already processed. Skipping.", directoryName);
                        continue;
                    }

                    foreach (var filePath in Directory.GetFiles(directoryPath, "*.dem"))
                    {
                        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                        var outputSubDirectory = Path.Combine(OutputDirectory, directoryName, fileNameWithoutExtension);
                        Directory.CreateDirectory(outputSubDirectory);

                        var bioDataParser = new BioDataParser(outputSubDirectory);
                        await bioDataParser.ParseDemoAsync(filePath);

                        // Log the processed file
                        Log.Information("Processed file: {FilePath}", filePath);
                    }

                    // Log the directory as processed and flush the log
                    LogProcessedDirectory(directoryName);
                    Log.Information("Processed directory: {DirectoryName}", directoryName);
                    Log.CloseAndFlush();

                    // Remove the processed directory
                    Directory.Delete(directoryPath, true);
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "An error occurred while processing the demo files");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static HashSet<string> LoadProcessedDirectories()
        {
            if (!File.Exists(ProcessedLogFilePath))
            {
                return new HashSet<string>();
            }

            var processedDirectories = new HashSet<string>(File.ReadAllLines(ProcessedLogFilePath));
            return processedDirectories;
        }

        private static void LogProcessedDirectory(string directoryName)
        {
            File.AppendAllLines(ProcessedLogFilePath, new[] { directoryName });
        }
    }
}