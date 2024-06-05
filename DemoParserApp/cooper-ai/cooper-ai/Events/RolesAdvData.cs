// RolesAdvData.cs
using System;
using System.Collections.Generic;
using DemoFile;
using DemoFile.Sdk;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;

public class RolesAdvData
{
    private readonly DemoParser _demoParser;
    private readonly List<object> _events;
    private readonly ILogger _eventSizeLogger;
    private readonly JsonSerializerSettings _jsonSettings;

    public RolesAdvData(DemoParser demoParser, List<object> events)
    {
        _demoParser = demoParser;
        _events = events;
        _eventSizeLogger = new LoggerConfiguration()
            .WriteTo.File("logs/event_sizes.txt", rollingInterval: RollingInterval.Day, buffered: false) // Disable buffering to flush logs frequently
            .CreateLogger();

        _jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.None, // Use None to reduce memory usage
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore, // Ignore self-referencing loops
            ContractResolver = new CustomContractResolver(),
            MaxDepth = 10 // Limit the depth of serialization
        };
    }

    public void Initialize()
    {
        AttachEventHandlers();
    }

    private void AttachEventHandlers()
    {
        _demoParser.Source1GameEvents.PlayerScore += OnPlayerScore;
        _demoParser.Source1GameEvents.RoundEnd += OnRoundEnd;
        _demoParser.Source1GameEvents.RoundStart += OnRoundStart; // Add RoundStart event handler
        // Removed MolotovDetonate and GrenadeThrown event handlers
    }

    private void OnPlayerScore(Source1PlayerScoreEvent e)
    {
        LogEventSize(e, nameof(OnPlayerScore));
        _events.Add(e);
    }

    private void OnRoundEnd(Source1RoundEndEvent e)
    {
        LogEventSize(e, nameof(OnRoundEnd));
        _events.Add(e);
    }

    private void OnRoundStart(Source1RoundStartEvent e) // Add OnRoundStart method
    {
        LogEventSize(e, nameof(OnRoundStart));
        _events.Add(e);
    }

    private void LogEventSize(object e, string eventName)
    {
        try
        {
            var json = JsonConvert.SerializeObject(e, _jsonSettings);
            var size = System.Text.Encoding.UTF8.GetByteCount(json);
            _eventSizeLogger.Information($"{eventName} event size: {size} bytes");
        }
        catch (Exception ex)
        {
            _eventSizeLogger.Error(ex, $"Error serializing {eventName} event");
        }
    }

    public List<object> GetCollectedEvents()
    {
        return _events;
    }

    public class CustomContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(System.Reflection.MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (property.PropertyType == typeof(DemoFile.Sdk.CCSPlayerController))
            {
                property.ShouldSerialize = instance => false;
            }
            // Exclude other large or unnecessary properties here
            if (property.PropertyName == "LargeProperty1" || property.PropertyName == "LargeProperty2")
            {
                property.ShouldSerialize = instance => false;
            }
            return property;
        }
    }
}