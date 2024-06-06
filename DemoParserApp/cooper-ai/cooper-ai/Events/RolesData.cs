// RolesData.cs
using System;
using System.Collections.Generic;
using DemoFile;
using DemoFile.Sdk;
using Serilog;

public class RolesData
{
    private readonly DemoParser _demoParser;
    private readonly List<object> _events;

    public RolesData(DemoParser demoParser, List<object> events)
    {
        _demoParser = demoParser;
        _events = events;
        AttachEventHandlers();
    }

    private void AttachEventHandlers()
    {
        _demoParser.Source1GameEvents.PlayerDeath += OnPlayerDeath;
        _demoParser.Source1GameEvents.WeaponFire += OnWeaponFire;
        _demoParser.Source1GameEvents.RoundStart += OnRoundStart;
        _demoParser.Source1GameEvents.RoundEnd += OnRoundEnd;
        _demoParser.Source1GameEvents.PlayerFootstep += OnPlayerFootstep;
        _demoParser.Source1GameEvents.PlayerJump += OnPlayerJump;
        _demoParser.Source1GameEvents.PlayerFalldamage += OnPlayerFalldamage;

        // Attach event handler for bombsite events
        _demoParser.EntityEvents.CCSPlayerPawn.AddChangeCallback(pl => pl.WhichBombZone, (playerPawn, oldBombZone, newBombZone) =>
        {
            var player = playerPawn.Controller;
            var eventType = newBombZone == 0 ? "LeftBombZone" : "EnteredBombZone";
            var eventData = new
            {
                EventType = eventType,
                Player = player?.PlayerName,
                PlayerId = player?.SteamID,
                BombZone = newBombZone == 0 ? oldBombZone : newBombZone,
                BombZoneName = playerPawn.LastPlaceName, // Include the bomb zone name
                Timestamp = _demoParser.CurrentGameTime.Value,
                PlayerPosition = playerPawn.Origin
            };
            _events.Add(eventData);
            Log.Information("Player {Player} {Action} bomb zone {BombZone} ({BombZoneName})", player?.PlayerName, eventType == "LeftBombZone" ? "left" : "entered", newBombZone == 0 ? oldBombZone : newBombZone, playerPawn.LastPlaceName);
        });
    }

    private void OnPlayerDeath(Source1PlayerDeathEvent e)
    {
        // Handle player death event
    }

    private void OnWeaponFire(Source1WeaponFireEvent e)
    {
        // Handle weapon fire event
    }

    private void OnRoundStart(Source1RoundStartEvent e)
    {
        // Handle round start event
    }

    private void OnRoundEnd(Source1RoundEndEvent e)
    {
        // Handle round end event
    }

    private void OnPlayerFootstep(Source1PlayerFootstepEvent e)
    {
        // Handle player footstep event
    }

    private void OnPlayerJump(Source1PlayerJumpEvent e)
    {
        // Handle player jump event
    }

    private void OnPlayerFalldamage(Source1PlayerFalldamageEvent e)
    {
        // Handle player fall damage event
    }
}