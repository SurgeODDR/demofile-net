// PlayerEvents.cs
using System;
using System.Collections.Generic;
using System.Reflection;
using DemoFile;
using DemoFile.Sdk;
using Serilog;

public class PlayerEvents
{
    private readonly Dictionary<ulong, bool> _playerBlindedState = new Dictionary<ulong, bool>();
    private readonly List<Source1PlayerBlindEvent> _recentPlayerBlindEvents = new List<Source1PlayerBlindEvent>();

    public PlayerEvents(DemoFile.DemoParser demo, List<object> events)
    {
        demo.Source1GameEvents.PlayerDeath += e =>
        {
            var attackerTeam = TeamNumberToString(e.Attacker?.CSTeamNum);
            var victimTeam = TeamNumberToString(e.Player?.CSTeamNum);

            // Log the teams for debugging
            Log.Information("PlayerDeath Event: Attacker={Attacker}, AttackerTeam={AttackerTeam}, Victim={Victim}, VictimTeam={VictimTeam}", e.Attacker?.PlayerName, attackerTeam, e.Player?.PlayerName, victimTeam);

            events.Add(new
            {
                EventType = "Kill",
                Timestamp = demo.CurrentGameTime.Value,
                Attacker = e.Attacker?.PlayerName,
                AttackerId = e.Attacker?.SteamID,
                AttackerTeam = attackerTeam,
                Victim = e.Player?.PlayerName,
                VictimId = e.Player?.SteamID,
                VictimTeam = victimTeam,
                Weapon = e.Weapon,
                Headshot = e.Headshot,
                ThroughWall = e.Penetrated,
                Blind = e.Attackerblind,
                ThroughSmoke = e.Thrusmoke,
                AttackerPosition = e.Attacker?.PlayerPawn?.Origin,
                VictimPosition = e.Player?.PlayerPawn?.Origin,
                LastPlaceName = e.Player?.PlayerPawn?.LastPlaceName, // Include the last place name
                CurrentEquipmentValue = e.Attacker?.PlayerPawn?.CurrentEquipmentValue,
                RoundStartEquipmentValue = e.Attacker?.PlayerPawn?.RoundStartEquipmentValue,
                FreezetimeEndEquipmentValue = e.Attacker?.PlayerPawn?.FreezetimeEndEquipmentValue
            });
        };

        demo.Source1GameEvents.PlayerHurt += e =>
        {
            var attackerTeam = TeamNumberToString(e.Attacker?.CSTeamNum);
            var victimTeam = TeamNumberToString(e.Player?.CSTeamNum);

            // Log the teams for debugging
            Log.Information("PlayerHurt Event: Attacker={Attacker}, AttackerTeam={AttackerTeam}, Victim={Victim}, VictimTeam={VictimTeam}", e.Attacker?.PlayerName, attackerTeam, e.Player?.PlayerName, victimTeam);

            events.Add(new
            {
                EventType = "Damage",
                Timestamp = demo.CurrentGameTime.Value,
                Attacker = e.Attacker?.PlayerName,
                AttackerId = e.Attacker?.SteamID,
                AttackerTeam = attackerTeam,
                Victim = e.Player?.PlayerName,
                VictimId = e.Player?.SteamID,
                VictimTeam = victimTeam,
                Weapon = e.Weapon,
                Damage = e.DmgHealth,
                ArmorDamage = e.DmgArmor,
                Hitgroup = e.Hitgroup,
                AttackerPosition = e.Attacker?.PlayerPawn?.Origin,
                VictimPosition = e.Player?.PlayerPawn?.Origin,
                LastPlaceName = e.Player?.PlayerPawn?.LastPlaceName, // Include the last place name
                CurrentEquipmentValue = e.Attacker?.PlayerPawn?.CurrentEquipmentValue,
                RoundStartEquipmentValue = e.Attacker?.PlayerPawn?.RoundStartEquipmentValue,
                FreezetimeEndEquipmentValue = e.Attacker?.PlayerPawn?.FreezetimeEndEquipmentValue
            });
        };

        // Add event handler for PlayerBlind
        demo.Source1GameEvents.PlayerBlind += e =>
        {
            var attackerTeam = TeamNumberToString(e.Attacker?.CSTeamNum);
            var victimTeam = TeamNumberToString(e.Player?.CSTeamNum);

            // Log the teams for debugging
            Log.Information("PlayerBlind Event: Attacker={Attacker}, AttackerTeam={AttackerTeam}, Victim={Victim}, VictimTeam={VictimTeam}", e.Attacker?.PlayerName, attackerTeam, e.Player?.PlayerName, victimTeam);

            events.Add(new
            {
                EventType = "PlayerBlind",
                Timestamp = demo.CurrentGameTime.Value,
                Player = e.Player?.PlayerName,
                PlayerId = e.Player?.SteamID,
                PlayerTeam = victimTeam,
                Attacker = e.Attacker?.PlayerName,
                AttackerId = e.Attacker?.SteamID,
                AttackerTeam = attackerTeam,
                BlindDuration = e.BlindDuration,
                PlayerPosition = e.Player?.PlayerPawn?.Origin,
                AttackerPosition = e.Attacker?.PlayerPawn?.Origin,
                LastPlaceName = e.Player?.PlayerPawn?.LastPlaceName, // Include the last place name
                CurrentEquipmentValue = e.Player?.PlayerPawn?.CurrentEquipmentValue,
                RoundStartEquipmentValue = e.Player?.PlayerPawn?.RoundStartEquipmentValue,
                FreezetimeEndEquipmentValue = e.Player?.PlayerPawn?.FreezetimeEndEquipmentValue
            });
        };

        // Other event handlers...

    }

    public Dictionary<ulong, bool> GetPlayerBlindedState()
    {
        return _playerBlindedState;
    }

    public List<Source1PlayerBlindEvent> GetRecentPlayerBlindEvents()
    {
        return _recentPlayerBlindEvents;
    }

    private static string TeamNumberToString(CSTeamNumber? csTeamNumber) => csTeamNumber switch
    {
        CSTeamNumber.Terrorist => "Terrorist",
        CSTeamNumber.CounterTerrorist => "Counter-Terrorist",
        _ => "Unknown",
    };
}