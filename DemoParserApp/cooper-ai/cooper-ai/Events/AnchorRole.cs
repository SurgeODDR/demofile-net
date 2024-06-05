using System;
using System.Collections.Generic;
using DemoFile;
using Serilog;

public class AnchorRole
{
    private readonly DemoParser _demoParser;
    private readonly List<object> _events;

    public AnchorRole(DemoParser demoParser, List<object> events)
    {
        _demoParser = demoParser;
        _events = events;
        AttachEventHandlers();
    }

    private void AttachEventHandlers()
    {
        _demoParser.Source1GameEvents.EnterBombzone += OnEnterBombzone;
        _demoParser.Source1GameEvents.ExitBombzone += OnExitBombzone;
    }

    private void OnEnterBombzone(Source1EnterBombzoneEvent e)
    {
        var logEntry = new
        {
            EventType = "EnteredBombZone",
            Player = e.Player?.PlayerName,
            PlayerId = e.Player?.SteamID,
            HasBomb = e.Hasbomb,
            IsPlanted = e.Isplanted,
            Timestamp = _demoParser.CurrentGameTime.Value,
            PlayerPosition = e.Player?.PlayerPawn?.Origin,
            BombZoneName = e.Player?.PlayerPawn?.LastPlaceName // Include the bomb zone name
        };

        _events.Add(logEntry);
        Log.Information("Player {Player} entered bombzone {BombZoneName}. HasBomb: {HasBomb}, IsPlanted: {IsPlanted}, Time: {Time}", e.Player?.PlayerName, e.Player?.PlayerPawn?.LastPlaceName, e.Hasbomb, e.Isplanted, _demoParser.CurrentGameTime.Value);
    }

    private void OnExitBombzone(Source1ExitBombzoneEvent e)
    {
        var logEntry = new
        {
            EventType = "ExitedBombZone",
            Player = e.Player?.PlayerName,
            PlayerId = e.Player?.SteamID,
            HasBomb = e.Hasbomb,
            IsPlanted = e.Isplanted,
            Timestamp = _demoParser.CurrentGameTime.Value,
            PlayerPosition = e.Player?.PlayerPawn?.Origin,
            BombZoneName = e.Player?.PlayerPawn?.LastPlaceName // Include the bomb zone name
        };

        _events.Add(logEntry);
        Log.Information("Player {Player} exited bombzone {BombZoneName}. HasBomb: {HasBomb}, IsPlanted: {IsPlanted}, Time: {Time}", e.Player?.PlayerName, e.Player?.PlayerPawn?.LastPlaceName, e.Hasbomb, e.Isplanted, _demoParser.CurrentGameTime.Value);
    }
}