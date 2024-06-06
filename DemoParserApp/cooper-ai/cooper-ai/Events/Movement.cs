// Movement.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DemoFile;
using DemoFile.Sdk;
using Newtonsoft.Json;
using Serilog;

public class Movement
{
    private readonly DemoParser _demoParser;
    private readonly List<MovementEvent> _movementEvents;
    private readonly string _outputFilePath;
    private readonly string _mapNameFilePath;
    private readonly HashSet<ulong> _playersExitedBuyzone;
    private bool _roundStarted;

    public Movement(DemoParser demoParser, string outputFilePath = "movement.json", string mapNameFilePath = "mapname.txt")
    {
        _demoParser = demoParser;
        _movementEvents = new List<MovementEvent>();
        _outputFilePath = outputFilePath;
        _mapNameFilePath = mapNameFilePath;
        _playersExitedBuyzone = new HashSet<ulong>();
        AttachEventHandlers();
    }

    private void AttachEventHandlers()
    {
        _demoParser.EntityEvents.CCSPlayerPawn.AddChangeCallback(pl => pl.LastPlaceName, OnLastPlaceNameChanged);
        _demoParser.Source1GameEvents.ExitBuyzone += OnExitBuyzone;
        _demoParser.Source1GameEvents.RoundStart += OnRoundStart;
        _demoParser.Source1GameEvents.RoundEnd += OnRoundEnd;
    }

    private void OnLastPlaceNameChanged(CCSPlayerPawn playerPawn, string oldPlace, string newPlace)
    {
        try
        {
            if (newPlace != null && playerPawn.Controller != null)
            {
                var movementEvent = new MovementEvent
                {
                    PlayerName = playerPawn.Controller.PlayerName,
                    PlayerId = playerPawn.Controller.SteamID,
                    Timestamp = _demoParser.CurrentGameTime.Value,
                    LastPlaceName = oldPlace, // Include the old place name
                    NewPlaceName = newPlace, // Include the new place name
                    PlayerPosition = playerPawn.Origin,
                    Health = playerPawn.Health,
                    Armor = playerPawn.ArmorValue, // Using ArmorValue property
                    Weapon = playerPawn.ActiveWeapon?.GetType().Name, // Using the type name as a placeholder for WeaponName
                    Team = playerPawn.Controller.CSTeamNum.ToString()
                };

                _movementEvents.Add(movementEvent);
                Log.Information("Player {PlayerName} moved from {LastPlaceName} to {NewPlaceName} at {Timestamp}", 
                    movementEvent.PlayerName, 
                    movementEvent.LastPlaceName, 
                    movementEvent.NewPlaceName, 
                    movementEvent.Timestamp);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in OnLastPlaceNameChanged for player {PlayerName}", playerPawn.Controller?.PlayerName);
        }
    }

    private void OnExitBuyzone(Source1ExitBuyzoneEvent e)
    {
        try
        {
            if (_roundStarted && e.Player != null && !_playersExitedBuyzone.Contains(e.Player.SteamID))
            {
                _playersExitedBuyzone.Add(e.Player.SteamID);

                var playerPawn = e.Player.PlayerPawn;
                if (playerPawn != null)
                {
                    var inventoryLogEvent = new InventoryLogEvent
                    {
                        PlayerName = e.Player.PlayerName,
                        PlayerId = e.Player.SteamID,
                        Timestamp = _demoParser.CurrentGameTime.Value,
                        PlayerPosition = playerPawn.Origin,
                        Health = playerPawn.Health,
                        Armor = playerPawn.ArmorValue, // Using ArmorValue property
                        Weapon = playerPawn.ActiveWeapon?.GetType().Name, // Using the type name as a placeholder for WeaponName
                        Team = playerPawn.Controller.CSTeamNum.ToString()
                    };

                    _movementEvents.Add(inventoryLogEvent);
                    Log.Information("Player {PlayerName} exited buyzone at {Timestamp}", 
                        inventoryLogEvent.PlayerName, 
                        inventoryLogEvent.Timestamp);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in OnExitBuyzone for player {PlayerName}", e.Player?.PlayerName);
        }
    }

    private void OnRoundStart(Source1RoundStartEvent e)
    {
        try
        {
            _roundStarted = true;
            _playersExitedBuyzone.Clear();
            Log.Information("Round started at {Timestamp}", _demoParser.CurrentGameTime.Value);

            var roundStartEvent = new RoundEvent
            {
                EventType = "RoundStart",
                Timestamp = _demoParser.CurrentGameTime.Value,
                Timelimit = e.Timelimit,
                Fraglimit = e.Fraglimit,
                Objective = e.Objective
            };

            _movementEvents.Add(roundStartEvent);
            Log.Information("RoundStart event added: {@RoundStartEvent}", roundStartEvent);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in OnRoundStart");
        }
    }

    private void OnRoundEnd(Source1RoundEndEvent e)
    {
        try
        {
            _roundStarted = false;
            Log.Information("Round ended at {Timestamp}", _demoParser.CurrentGameTime.Value);

            var roundEndEvent = new RoundEvent
            {
                EventType = "RoundEnd",
                Timestamp = _demoParser.CurrentGameTime.Value,
                Winner = e.Winner.ToString(),
                Reason = e.Reason.ToString(),
                Message = e.Message,
                Legacy = e.Legacy == 1,
                PlayerCount = e.PlayerCount,
                Nomusic = e.Nomusic == 1
            };

            _movementEvents.Add(roundEndEvent);
            Log.Information("RoundEnd event added: {@RoundEndEvent}", roundEndEvent);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in OnRoundEnd");
        }
    }

    public async Task SaveMovementDataAsync()
    {
        try
        {
            var mapName = _demoParser.FileHeader.MapName; // Retrieve the map name correctly
            await SaveMapNameAsync(mapName); // Save the map name to mapname.txt

            var json = JsonConvert.SerializeObject(_movementEvents, Formatting.Indented);
            await File.WriteAllTextAsync(_outputFilePath, json);
            Log.Information("Movement data saved to {OutputFilePath}", _outputFilePath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error saving movement data");
        }
    }

    private async Task SaveMapNameAsync(string mapName)
    {
        try
        {
            await File.WriteAllTextAsync(_mapNameFilePath, mapName);
            Log.Information("Map name saved to {MapNameFilePath}", _mapNameFilePath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error saving map name");
        }
    }

    private class MovementEvent
    {
        public string PlayerName { get; set; } = string.Empty;
        public ulong PlayerId { get; set; }
        public float Timestamp { get; set; }
        public string LastPlaceName { get; set; } = string.Empty;
        public string NewPlaceName { get; set; } = string.Empty; // New property for the new place name
        public Vector PlayerPosition { get; set; }
        public int Health { get; set; }
        public int Armor { get; set; }
        public string? Weapon { get; set; }
        public string Team { get; set; } = string.Empty;
    }

    private class InventoryLogEvent : MovementEvent
    {
    }

    private class RoundEvent : MovementEvent
    {
        public string EventType { get; set; } = string.Empty;
        public int? Timelimit { get; set; }
        public int? Fraglimit { get; set; }
        public string? Objective { get; set; }
        public string? Winner { get; set; }
        public string? Reason { get; set; }
        public string? Message { get; set; }
        public bool? Legacy { get; set; }
        public int? PlayerCount { get; set; }
        public bool? Nomusic { get; set; }
    }
}