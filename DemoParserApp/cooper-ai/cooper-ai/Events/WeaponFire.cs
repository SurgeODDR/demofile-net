// WeaponFire.cs
using System.Collections.Generic;
using DemoFile;
using DemoFile.Sdk;
using Serilog;

public class WeaponFire
{
    private readonly DemoParser _demoParser;
    private readonly List<object> _events;
    private readonly Dictionary<ulong, PlayerWeaponStats> _playerWeaponStats;

    public WeaponFire(DemoParser demoParser, List<object> events)
    {
        _demoParser = demoParser;
        _events = events;
        _playerWeaponStats = new Dictionary<ulong, PlayerWeaponStats>();
        AttachEventHandlers();
    }

    private void AttachEventHandlers()
    {
        _demoParser.Source1GameEvents.WeaponFire += OnWeaponFire;
        _demoParser.Source1GameEvents.PlayerHurt += OnPlayerHurt;
        _demoParser.Source1GameEvents.RoundEnd += OnRoundEnd;
    }

    private void OnWeaponFire(Source1WeaponFireEvent e)
    {
        if (e.Player == null) return;

        if (!_playerWeaponStats.ContainsKey(e.Player.SteamID))
        {
            _playerWeaponStats[e.Player.SteamID] = new PlayerWeaponStats
            {
                PlayerName = e.Player.PlayerName,
                PlayerId = e.Player.SteamID
            };
        }

        _playerWeaponStats[e.Player.SteamID].ShotsFired++;
        Log.Information("Player {PlayerName} fired a shot with {Weapon}", e.Player.PlayerName, e.Weapon);

        _events.Add(new
        {
            EventType = "WeaponFire",
            Player = e.Player.PlayerName,
            PlayerId = e.Player.SteamID,
            Weapon = e.Weapon,
            Timestamp = _demoParser.CurrentGameTime.Value,
            PlayerPosition = e.Player.PlayerPawn?.Origin,
            LastPlaceName = e.Player.PlayerPawn?.LastPlaceName, // Include the last place name
            AttackerTeam = TeamNumberToString(e.Player.CSTeamNum), // Include the attacker's team
            CurrentEquipmentValue = e.Player.PlayerPawn?.CurrentEquipmentValue,
            RoundStartEquipmentValue = e.Player.PlayerPawn?.RoundStartEquipmentValue,
            FreezetimeEndEquipmentValue = e.Player.PlayerPawn?.FreezetimeEndEquipmentValue
        });
    }

    private void OnPlayerHurt(Source1PlayerHurtEvent e)
    {
        if (e.Attacker == null) return;

        if (!_playerWeaponStats.ContainsKey(e.Attacker.SteamID))
        {
            _playerWeaponStats[e.Attacker.SteamID] = new PlayerWeaponStats
            {
                PlayerName = e.Attacker.PlayerName,
                PlayerId = e.Attacker.SteamID
            };
        }

        _playerWeaponStats[e.Attacker.SteamID].ShotsHit++;
        Log.Information("Player {PlayerName} hit a shot with {Weapon}", e.Attacker.PlayerName, e.Weapon);

        _events.Add(new
        {
            EventType = "PlayerHurt",
            Attacker = e.Attacker.PlayerName,
            AttackerId = e.Attacker.SteamID,
            Victim = e.Player?.PlayerName,
            VictimId = e.Player?.SteamID,
            Weapon = e.Weapon,
            Timestamp = _demoParser.CurrentGameTime.Value,
            AttackerPosition = e.Attacker.PlayerPawn?.Origin,
            VictimPosition = e.Player?.PlayerPawn?.Origin,
            LastPlaceName = e.Player?.PlayerPawn?.LastPlaceName, // Include the last place name
            AttackerTeam = TeamNumberToString(e.Attacker.CSTeamNum), // Include the attacker's team
            VictimTeam = TeamNumberToString(e.Player?.CSTeamNum), // Include the victim's team
            CurrentEquipmentValue = e.Attacker.PlayerPawn?.CurrentEquipmentValue,
            RoundStartEquipmentValue = e.Attacker.PlayerPawn?.RoundStartEquipmentValue,
            FreezetimeEndEquipmentValue = e.Attacker.PlayerPawn?.FreezetimeEndEquipmentValue
        });
    }

    private void OnRoundEnd(Source1RoundEndEvent e)
    {
        foreach (var playerStats in _playerWeaponStats.Values)
        {
            var accuracy = playerStats.ShotsFired > 0
                ? (double)playerStats.ShotsHit / playerStats.ShotsFired * 100
                : 0;

            Log.Information("Player {PlayerName} accuracy: {Accuracy}%", playerStats.PlayerName, accuracy);

            _events.Add(new
            {
                EventType = "RoundEnd",
                Player = playerStats.PlayerName,
                PlayerId = playerStats.PlayerId,
                ShotsFired = playerStats.ShotsFired,
                ShotsHit = playerStats.ShotsHit,
                Accuracy = accuracy,
                Timestamp = _demoParser.CurrentGameTime.Value
            });
        }

        // Reset stats for the next round
        _playerWeaponStats.Clear();
    }

    private class PlayerWeaponStats
    {
        public string PlayerName { get; set; }
        public ulong PlayerId { get; set; }
        public int ShotsFired { get; set; }
        public int ShotsHit { get; set; }
    }

    private static string TeamNumberToString(CSTeamNumber? csTeamNumber) => csTeamNumber switch
    {
        CSTeamNumber.Terrorist => "Terrorist",
        CSTeamNumber.CounterTerrorist => "Counter-Terrorist",
        _ => "Unknown",
    };
}