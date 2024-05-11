using BepInEx.Logging;
using Bloodstone.API;
using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using Unity.Entities;
using Exception = System.Exception;

namespace SpiderKiller;

public class GiveDrop
{
    private static ManualLogSource _log => Plugin.LogInstance;

    public static void AddItemToInventory(Entity recipient, PrefabGUID guid, int amount)
    {
        try
        {
            var gameData = VWorld.Server.GetExistingSystemManaged<GameDataSystem>();

            // doesnt seem to work either
            GiveItemCommandUtility.RunGiveItemCommand(guid, amount, false);
        }
        catch (Exception e)
        {
            _log.LogError(e.Message);
            _log.LogError(e.StackTrace);
        }
    }
}