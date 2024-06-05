// Program.cs
using System;
using System.Threading.Tasks;
using Serilog;
using cooper_ai.Events; // Add this using directive

namespace cooper_ai
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day, buffered: false) // Disable buffering to flush logs frequently
                .CreateLogger();

            try
            {
                var path = args.Length > 0 ? args[0] : "/Users/olivierdebeufderijcker/Documents/GitHub/cooper-ai/cs2/demofile-net/hotu-vs-cybershoke-m1-ancient.dem";
                var bioDataParser = new BioDataParser();
                await bioDataParser.ParseDemoAsync(path);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "An error occurred while parsing the demo");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}