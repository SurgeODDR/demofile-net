// EcoData.cs
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DemoFile;
using DemoFile.Sdk;
using Serilog;

public class EcoData
{
    private readonly DemoParser _demoParser;
    private readonly List<object> _events;
    private readonly Dictionary<ulong, PlayerEcoData> _playerEcoData;

    public EcoData(DemoParser demoParser, List<object> events)
    {
        _demoParser = demoParser;
        _events = events;
        _playerEcoData = new Dictionary<ulong, PlayerEcoData>();
        AttachEventHandlers();
    }

    private void AttachEventHandlers()
    {
        _demoParser.Source1GameEvents.RoundStart += OnRoundStart;
        _demoParser.Source1GameEvents.RoundEnd += OnRoundEnd;
        _demoParser.Source1GameEvents.RoundFreezeEnd += OnRoundFreezeEnd;
    }

    private void OnRoundStart(Source1RoundStartEvent e)
    {
        foreach (var player in _demoParser.Players)
        {
            if (player == null || player.SteamID == 0) continue;

            var playerEcoData = new PlayerEcoData
            {
                PlayerName = player.PlayerName,
                PlayerId = player.SteamID,
                RoundStartEquipmentValue = player.PlayerPawn?.RoundStartEquipmentValue ?? 0
            };

            _playerEcoData[player.SteamID] = playerEcoData;
        }

        Log.Information("Round started at {Timestamp}", _demoParser.CurrentGameTime.Value);
    }

    private void OnRoundFreezeEnd(Source1RoundFreezeEndEvent e)
    {
        foreach (var player in _demoParser.Players)
        {
            if (player == null || player.SteamID == 0) continue;

            if (_playerEcoData.TryGetValue(player.SteamID, out var playerEcoData))
            {
                playerEcoData.FreezetimeEndEquipmentValue = player.PlayerPawn?.FreezetimeEndEquipmentValue ?? 0;
            }
        }

        Log.Information("Freeze time ended at {Timestamp}", _demoParser.CurrentGameTime.Value);
    }

    private void OnRoundEnd(Source1RoundEndEvent e)
    {
        foreach (var playerEcoData in _playerEcoData.Values)
        {
            playerEcoData.RoundEndEquipmentValue = _demoParser.Players.FirstOrDefault(p => p.SteamID == playerEcoData.PlayerId)?.PlayerPawn?.CurrentEquipmentValue ?? 0;
            _events.Add(playerEcoData);
        }

        Log.Information("Round ended at {Timestamp}", _demoParser.CurrentGameTime.Value);
    }

    private class PlayerEcoData
    {
        public string PlayerName { get; set; }
        public ulong PlayerId { get; set; }
        public ushort RoundStartEquipmentValue { get; set; }
        public ushort FreezetimeEndEquipmentValue { get; set; }
        public ushort RoundEndEquipmentValue { get; set; }
    }
}