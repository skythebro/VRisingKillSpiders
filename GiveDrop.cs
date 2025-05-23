using BepInEx.Logging;
using ProjectM.Scripting;
using Stunlock.Core;
using Unity.Entities;
using VAMP;
using Exception = System.Exception;

namespace SpiderKiller;

public class GiveDrop
{
    private static ManualLogSource _log => Plugin.LogInstance;

    public static bool AddItemToInventory(Entity recipient, PrefabGUID guid, int amount)
    {
        try
        {
            ServerGameManager serverGameManager =
                Core.Server.GetExistingSystemManaged<ServerScriptMapper>()._ServerGameManager;
            var inventoryResponse = serverGameManager.TryAddInventoryItem(recipient, guid, amount);
#if DEBUG
            _log.LogMessage($"AddItemToInventory: {inventoryResponse.Success}");
#endif
            return inventoryResponse.Success;
        }
        catch (Exception e)
        {
            _log.LogError(e);
        }

        return false;
    }
}