// GrenadeEvents.cs
using System.Collections.Generic;
using DemoFile;
using DemoFile.Sdk;
using Serilog;

public class GrenadeEvents
{
    private readonly Dictionary<ulong, List<Source1PlayerBlindEvent>> _playerBlindEventsByFlashbang;
    private readonly List<Source1PlayerBlindEvent> _recentPlayerBlindEvents;
    private readonly List<object> _events;
    private readonly DemoParser _demo;

    public GrenadeEvents(DemoParser demo, List<object> events)
    {
        _demo = demo;
        _playerBlindEventsByFlashbang = new Dictionary<ulong, List<Source1PlayerBlindEvent>>();
        _recentPlayerBlindEvents = new List<Source1PlayerBlindEvent>();
        _events = events;

        _demo.Source1GameEvents.FlashbangDetonate += OnFlashbangDetonate;
        _demo.Source1GameEvents.SmokegrenadeDetonate += OnSmokegrenadeDetonate;
        _demo.Source1GameEvents.HegrenadeDetonate += OnHegrenadeDetonate;
        _demo.Source1GameEvents.MolotovDetonate += OnMolotovDetonate;
        _demo.Source1GameEvents.InfernoStartburn += OnInfernoStartburn;
        _demo.Source1GameEvents.GrenadeThrown += OnGrenadeThrown;
        _demo.Source1GameEvents.PlayerBlind += OnPlayerBlind;
        _demo.Source1GameEvents.WeaponFire += OnWeaponFire; // Add WeaponFire event handler
    }

    private void OnPlayerBlind(Source1PlayerBlindEvent e)
    {
        if (e.Attacker?.SteamID != null)
        {
            if (!_playerBlindEventsByFlashbang.ContainsKey(e.Attacker.SteamID))
            {
                _playerBlindEventsByFlashbang[e.Attacker.SteamID] = new List<Source1PlayerBlindEvent>();
            }
            _playerBlindEventsByFlashbang[e.Attacker.SteamID].Add(e);
        }
        _recentPlayerBlindEvents.Add(e);
    }

    private void OnFlashbangDetonate(Source1FlashbangDetonateEvent e)
    {
        int blindedEnemiesCount = 0;
        if (e.Player?.SteamID != null && _playerBlindEventsByFlashbang.ContainsKey(e.Player.SteamID))
        {
            foreach (var blindEvent in _playerBlindEventsByFlashbang[e.Player.SteamID])
            {
                if (blindEvent.Player?.CSTeamNum != e.Player?.CSTeamNum)
                {
                    blindedEnemiesCount++;
                }
            }
            _playerBlindEventsByFlashbang[e.Player.SteamID].Clear();
        }

        _events.Add(new
        {
            EventType = "Grenade",
            GrenadeType = "Flashbang",
            Timestamp = _demo.CurrentGameTime.Value,
            Player = e.Player?.PlayerName,
            PlayerId = e.Player?.SteamID,
            Location = new { e.X, e.Y, e.Z },
            PlayerPosition = e.Player?.PlayerPawn?.Origin,
            LastPlaceName = e.Player?.PlayerPawn?.LastPlaceName, // Include the last place name
            BlindedEnemiesCount = blindedEnemiesCount,
            AttackerTeam = TeamNumberToString(e.Player?.CSTeamNum), // Include the attacker's team
            CurrentEquipmentValue = e.Player?.PlayerPawn?.CurrentEquipmentValue,
            RoundStartEquipmentValue = e.Player?.PlayerPawn?.RoundStartEquipmentValue,
            FreezetimeEndEquipmentValue = e.Player?.PlayerPawn?.FreezetimeEndEquipmentValue
        });
    }

    private void OnSmokegrenadeDetonate(Source1SmokegrenadeDetonateEvent e)
    {
        Log.Information("SmokegrenadeDetonate event triggered");
        _events.Add(new
        {
            EventType = "Grenade",
            GrenadeType = "Smoke",
            Timestamp = _demo.CurrentGameTime.Value,
            Player = e.Player?.PlayerName,
            PlayerId = e.Player?.SteamID,
            Location = new { e.X, e.Y, e.Z },
            PlayerPosition = e.Player?.PlayerPawn?.Origin,
            LastPlaceName = e.Player?.PlayerPawn?.LastPlaceName, // Include the last place name
            AttackerTeam = TeamNumberToString(e.Player?.CSTeamNum), // Include the attacker's team
            CurrentEquipmentValue = e.Player?.PlayerPawn?.CurrentEquipmentValue,
            RoundStartEquipmentValue = e.Player?.PlayerPawn?.RoundStartEquipmentValue,
            FreezetimeEndEquipmentValue = e.Player?.PlayerPawn?.FreezetimeEndEquipmentValue
        });
    }

    private void OnHegrenadeDetonate(Source1HegrenadeDetonateEvent e)
    {
        _events.Add(new
        {
            EventType = "Grenade",
            GrenadeType = "HE",
            Timestamp = _demo.CurrentGameTime.Value,
            Player = e.Player?.PlayerName,
            PlayerId = e.Player?.SteamID,
            Location = new { e.X, e.Y, e.Z },
            PlayerPosition = e.Player?.PlayerPawn?.Origin,
            LastPlaceName = e.Player?.PlayerPawn?.LastPlaceName, // Include the last place name
            AttackerTeam = TeamNumberToString(e.Player?.CSTeamNum), // Include the attacker's team
            CurrentEquipmentValue = e.Player?.PlayerPawn?.CurrentEquipmentValue,
            RoundStartEquipmentValue = e.Player?.PlayerPawn?.RoundStartEquipmentValue,
            FreezetimeEndEquipmentValue = e.Player?.PlayerPawn?.FreezetimeEndEquipmentValue
        });
    }

    private void OnMolotovDetonate(Source1MolotovDetonateEvent e)
    {
        _events.Add(new
        {
            EventType = "Grenade",
            GrenadeType = "Molotov",
            Timestamp = _demo.CurrentGameTime.Value,
            Player = e.Player?.PlayerName,
            PlayerId = e.Player?.SteamID,
            Location = new { e.X, e.Y, e.Z },
            PlayerPosition = e.Player?.PlayerPawn?.Origin,
            LastPlaceName = e.Player?.PlayerPawn?.LastPlaceName, // Include the last place name
            AttackerTeam = TeamNumberToString(e.Player?.CSTeamNum), // Include the attacker's team
            CurrentEquipmentValue = e.Player?.PlayerPawn?.CurrentEquipmentValue,
            RoundStartEquipmentValue = e.Player?.PlayerPawn?.RoundStartEquipmentValue,
            FreezetimeEndEquipmentValue = e.Player?.PlayerPawn?.FreezetimeEndEquipmentValue
        });
    }

    private void OnInfernoStartburn(Source1InfernoStartburnEvent e)
    {
        _events.Add(new
        {
            EventType = "Inferno",
            Timestamp = _demo.CurrentGameTime.Value,
            EntityId = e.Entityid,
            Location = new { e.X, e.Y, e.Z }
        });
    }

    private void OnGrenadeThrown(Source1GrenadeThrownEvent e)
    {
        string grenadeType = e.Weapon switch
        {
            "molotov" => "Molotov",
            "incgrenade" => "Incendiary",
            "flashbang" => "Flashbang",
            "smokegrenade" => "Smoke",
            "hegrenade" => "HE",
            _ => "Unknown"
        };

        _events.Add(new
        {
            EventType = "GrenadeThrown",
            GrenadeType = grenadeType,
            Timestamp = _demo.CurrentGameTime.Value,
            Player = e.Player?.PlayerName,
            PlayerId = e.Player?.SteamID,
            Weapon = e.Weapon,
            PlayerPosition = e.Player?.PlayerPawn?.Origin,
            LastPlaceName = e.Player?.PlayerPawn?.LastPlaceName, // Include the last place name
            AttackerTeam = TeamNumberToString(e.Player?.CSTeamNum), // Include the attacker's team
            CurrentEquipmentValue = e.Player?.PlayerPawn?.CurrentEquipmentValue,
            RoundStartEquipmentValue = e.Player?.PlayerPawn?.RoundStartEquipmentValue,
            FreezetimeEndEquipmentValue = e.Player?.PlayerPawn?.FreezetimeEndEquipmentValue
        });
    }

    private void OnWeaponFire(Source1WeaponFireEvent e)
    {
        if (e.Weapon.Contains("molotov") || e.Weapon.Contains("incgrenade"))
        {
            string grenadeType = e.Weapon switch
            {
                "molotov" => "Molotov",
                "incgrenade" => "Incendiary",
                _ => "Unknown"
            };

            _events.Add(new
            {
                EventType = "GrenadeThrown",
                GrenadeType = grenadeType,
                Timestamp = _demo.CurrentGameTime.Value,
                Player = e.Player?.PlayerName,
                PlayerId = e.Player?.SteamID,
                Weapon = e.Weapon,
                PlayerPosition = e.Player?.PlayerPawn?.Origin,
                LastPlaceName = e.Player?.PlayerPawn?.LastPlaceName, // Include the last place name
                AttackerTeam = TeamNumberToString(e.Player?.CSTeamNum), // Include the attacker's team
                CurrentEquipmentValue = e.Player?.PlayerPawn?.CurrentEquipmentValue,
                RoundStartEquipmentValue = e.Player?.PlayerPawn?.RoundStartEquipmentValue,
                FreezetimeEndEquipmentValue = e.Player?.PlayerPawn?.FreezetimeEndEquipmentValue
            });

            Log.Information("WeaponFire event for {Weapon} by {Player}", e.Weapon, e.Player?.PlayerName);
        }
    }

    private static string TeamNumberToString(CSTeamNumber? csTeamNumber) => csTeamNumber switch
    {
        CSTeamNumber.Terrorist => "Terrorist",
        CSTeamNumber.CounterTerrorist => "Counter-Terrorist",
        _ => "Unknown",
    };
}