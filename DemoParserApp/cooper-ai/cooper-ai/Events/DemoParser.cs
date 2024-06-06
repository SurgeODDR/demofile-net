// DemoParser.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DemoFile;
using Newtonsoft.Json;
using Serilog;

namespace cooper_ai.Events
{
    [JsonObject(IsReference = true)]
    public class BioData
    {
        public string PlayerName { get; set; }
        public int PlayerId { get; set; }

        public BioData()
        {
            PlayerName = string.Empty;
        }
    }

    public class BioDataParser
    {
        private readonly DemoParser _demoParser;
        private readonly BioData _bioData;
        private readonly List<object> _events;
        private readonly Movement _movement;
        private readonly string _outputDirectory;

        public BioDataParser(string outputDirectory)
        {
            _events = new List<object>();
            _demoParser = new DemoParser();
            _bioData = new BioData();
            _movement = new Movement(_demoParser, Path.Combine(outputDirectory, "movement.json"), Path.Combine(outputDirectory, "mapname.txt"));
            _outputDirectory = outputDirectory;

            // Initialize event handlers
            var playerEvents = new PlayerEvents(_demoParser, _events);
            new RolesAdvData(_demoParser, _events).Initialize();
            new AnchorRole(_demoParser, _events);
            new BombEvents(_demoParser, _events);
            new ItemEvents(_demoParser, _events);
            new RolesData(_demoParser, _events); // Pass _events to RolesData
            new RoundEvents(_demoParser, _events); // Ensure RoundEvents is initialized
            new GrenadeEvents(_demoParser, _events); // Ensure this line is present
            new EcoData(_demoParser, _events); // Initialize EcoData
            new WeaponFire(_demoParser, _events); // Initialize WeaponFire
        }

        public async Task ParseDemoAsync(string path)
        {
            try
            {
                await using var stream = File.OpenRead(path);
                await _demoParser.ReadAllAsync(stream);
                await SaveBioDataToFileAsync();
                await _movement.SaveMovementDataAsync(); // Save movement data
            }
            catch (Exception ex) when (ex.Message.Contains("Attempted to create unknown entity"))
            {
                Log.Warning("Encountered unknown entity while parsing the demo file: {Path}. Continuing with parsing.", path);
                Log.Warning(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while parsing the demo file: {Path}", path);
            }
        }

        private async Task SaveBioDataToFileAsync()
        {
            // Log the contents of the _events list before serialization
            Log.Information("Events list before serialization: {@Events}", _events);

            var output = new
            {
                BioData = _bioData,
                Events = _events
            };

            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore, // Ignore self-referencing loops
                MaxDepth = 64 // Set a reasonable max depth to avoid performance issues
            };

            // Serialize BioData and Events
            var json = JsonConvert.SerializeObject(output, jsonSettings);
            var outputFilePath = Path.Combine(_outputDirectory, "bio_data_output.json");
            await File.WriteAllTextAsync(outputFilePath, json);
            Log.Information("Finished processing and output saved to {OutputFilePath}", outputFilePath);
        }
    }
}