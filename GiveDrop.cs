using System;
using BepInEx.Logging;
using Bloodstone.API;
using ProjectM;
using Unity.Entities;

namespace SpiderKiller;

public class GiveDrop
{
    private static ManualLogSource _log => Plugin.LogInstance;
    public static void AddItemToInventory(Entity recipient, PrefabGUID guid, int amount)
    {
        try
        {
            var gameData = VWorld.Server.GetExistingSystem<GameDataSystem>();
            var itemSettings = AddItemSettings.Create(VWorld.Server.EntityManager, gameData.ItemHashLookupMap);
            InventoryUtilitiesServer.TryAddItem(itemSettings, recipient, guid, amount);
        }
        catch (Exception e)
        {
            _log.LogError(e.Message);
        }
    }
}