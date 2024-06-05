// ItemEvents.cs
using DemoFile;
using DemoFile.Sdk;
using Serilog;
using System.Collections.Generic;

public class ItemEvents
{
    private readonly DemoParser _demo;
    private readonly List<object> _events;
    private readonly Dictionary<uint, (string PlayerName, ulong PlayerId, Vector PlayerPosition, string LastPlaceName)> _droppedItems;

    public ItemEvents(DemoParser demo, List<object> events)
    {
        _demo = demo;
        _events = events;
        _droppedItems = new Dictionary<uint, (string PlayerName, ulong PlayerId, Vector PlayerPosition, string LastPlaceName)>();

        _demo.Source1GameEvents.ItemPickup += OnItemPickup;
        _demo.Source1GameEvents.AmmoPickup += OnAmmoPickup;
        Log.Information("Event handlers for ItemPickup and AmmoPickup attached.");

        // Attach event handler for item dropped
        _demo.EntityEvents.CCSWeaponBase.AddChangeCallback(ent => ent.OwnerEntity, (ent, oldOwner, newOwner) =>
        {
            if (newOwner == null && oldOwner is CCSPlayerPawn playerPawn)
            {
                var player = playerPawn.Controller;
                var entityIndex = ent.EntityIndex.Value; // Extract the uint value
                _droppedItems[entityIndex] = (player?.PlayerName, player?.SteamID ?? 0, playerPawn.Origin, playerPawn.LastPlaceName);

                var eventData = new
                {
                    EventType = "ItemDropped",
                    Player = player?.PlayerName,
                    PlayerId = player?.SteamID,
                    Item = ent.EconItem.Name,
                    Timestamp = _demo.CurrentGameTime.Value,
                    PlayerPosition = playerPawn.Origin,
                    LastPlaceName = playerPawn.LastPlaceName, // Include the last place name
                    CurrentEquipmentValue = playerPawn.CurrentEquipmentValue,
                    RoundStartEquipmentValue = playerPawn.RoundStartEquipmentValue,
                    FreezetimeEndEquipmentValue = playerPawn.FreezetimeEndEquipmentValue
                };
                _events.Add(eventData);
                Log.Information("Item {Item} dropped by {Player}", ent.EconItem.Name, player?.PlayerName);
            }
            else if (newOwner is CCSPlayerPawn newPlayerPawn)
            {
                var newPlayer = newPlayerPawn.Controller;
                var entityIndex = ent.EntityIndex.Value; // Extract the uint value

                if (_droppedItems.TryGetValue(entityIndex, out var dropInfo))
                {
                    var eventData = new
                    {
                        EventType = "ItemPickedUp",
                        Player = newPlayer?.PlayerName,
                        PlayerId = newPlayer?.SteamID,
                        Item = ent.EconItem.Name,
                        Timestamp = _demo.CurrentGameTime.Value,
                        PlayerPosition = newPlayerPawn.Origin,
                        LastPlaceName = newPlayerPawn.LastPlaceName, // Include the last place name
                        DroppedBy = dropInfo.PlayerName,
                        DroppedById = dropInfo.PlayerId,
                        DroppedPosition = dropInfo.PlayerPosition,
                        DroppedLastPlaceName = dropInfo.LastPlaceName, // Include the last place name of the dropper
                        CurrentEquipmentValue = newPlayerPawn.CurrentEquipmentValue,
                        RoundStartEquipmentValue = newPlayerPawn.RoundStartEquipmentValue,
                        FreezetimeEndEquipmentValue = newPlayerPawn.FreezetimeEndEquipmentValue
                    };
                    _events.Add(eventData);
                    Log.Information("Item {Item} picked up by {Player}, dropped by {DroppedBy}", ent.EconItem.Name, newPlayer?.PlayerName, dropInfo.PlayerName);

                    // Remove the item from the dictionary after it has been picked up
                    _droppedItems.Remove(entityIndex);
                }
            }
        });
    }

    private void OnItemPickup(Source1ItemPickupEvent e)
    {
        if (e.Player == null)
        {
            Log.Warning("ItemPickup event received with null Player.");
            return;
        }
        Log.Information("Player {PlayerName} picked up {Item}", e.Player.PlayerName, e.Item);
        // Add to events list
        _events.Add(new
        {
            EventType = "ItemPickup",
            Player = e.Player.PlayerName,
            PlayerId = e.Player.SteamID,
            Item = e.Item,
            Timestamp = _demo.CurrentGameTime.Value,
            PlayerPosition = e.Player.PlayerPawn?.Origin,
            LastPlaceName = e.Player.PlayerPawn?.LastPlaceName, // Include the last place name
            CurrentEquipmentValue = e.Player.PlayerPawn?.CurrentEquipmentValue,
            RoundStartEquipmentValue = e.Player.PlayerPawn?.RoundStartEquipmentValue,
            FreezetimeEndEquipmentValue = e.Player.PlayerPawn?.FreezetimeEndEquipmentValue
        });
    }

    private void OnAmmoPickup(Source1AmmoPickupEvent e)
    {
        if (e.Player == null)
        {
            Log.Warning("AmmoPickup event received with null Player.");
            return;
        }
        Log.Information("Player {PlayerName} picked up ammo {Item}", e.Player.PlayerName, e.Item);
        // Add to events list
        _events.Add(new
        {
            EventType = "AmmoPickup",
            Player = e.Player.PlayerName,
            PlayerId = e.Player.SteamID,
            Item = e.Item,
            Timestamp = DateTime.UtcNow,
            PlayerPosition = e.Player.PlayerPawn?.Origin,
            LastPlaceName = e.Player.PlayerPawn?.LastPlaceName, // Include the last place name
            CurrentEquipmentValue = e.Player.PlayerPawn?.CurrentEquipmentValue,
            RoundStartEquipmentValue = e.Player.PlayerPawn?.RoundStartEquipmentValue,
            FreezetimeEndEquipmentValue = e.Player.PlayerPawn?.FreezetimeEndEquipmentValue
        });
    }
}