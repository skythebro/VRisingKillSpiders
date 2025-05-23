using BepInEx.Logging;
using Stunlock.Core;
using Unity.Entities;
using VAMP.Utilities;
using Exception = System.Exception;

namespace SpiderKiller;

public class GiveDrop
{
    private static ManualLogSource _log => Plugin.LogInstance;

    public static bool AddItemToInventory(Entity recipient, PrefabGUID guid, int amount)
    {
        try
        {
            var inventoryResponse = ItemUtil.AddItemToInventory(recipient, guid, amount, out var _);
#if DEBUG
            _log.LogMessage($"AddItemToInventory: {inventoryResponse}");
#endif
            return inventoryResponse;
        }
        catch (Exception e)
        {
            _log.LogError(e);
        }

        return false;
    }
}