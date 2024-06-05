using System.Collections.Generic;
using DemoFile;
using DemoFile.Sdk;

public class BombEvents
{
    public BombEvents(DemoFile.DemoParser demo, List<object> events)
    {
        demo.Source1GameEvents.BombPlanted += e =>
        {
            events.Add(new
            {
                EventType = "BombPlant",
                Timestamp = demo.CurrentGameTime.Value,
                Player = e.Player?.PlayerName,
                PlayerId = e.Player?.SteamID,
                Site = e.Site,
                PlayerPosition = e.Player?.PlayerPawn?.Origin,
                LastPlaceName = e.Player?.PlayerPawn?.LastPlaceName, // Include the last place name
                IsPlanted = true // Bomb is planted
            });
        };

        demo.Source1GameEvents.BombDefused += e =>
        {
            events.Add(new
            {
                EventType = "BombDefuse",
                Timestamp = demo.CurrentGameTime.Value,
                Player = e.Player?.PlayerName,
                PlayerId = e.Player?.SteamID,
                Site = e.Site,
                PlayerPosition = e.Player?.PlayerPawn?.Origin,
                LastPlaceName = e.Player?.PlayerPawn?.LastPlaceName, // Include the last place name
                IsPlanted = false // Bomb is defused
            });
        };
    }
}