using Bloodstone.API;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using ProjectM;
using ProjectM.Network;
using Stunlock.Network;
using Unity.Collections;
using Unity.Entities;

namespace SpiderKiller.Patches;


[HarmonyPatch]
public static class InitializePlayer_Patch
{
    
    public static List<int> playerEntityIndices = new ();

    
    [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
    [HarmonyPostfix]
    public static void OnUserConnected_Patch(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
    {
        try
        {
            var userIndex = VWorld.Server.GetExistingSystemManaged<ServerBootstrapSystem>()._NetEndPointToApprovedUserIndex[netConnectionId];
            var serverClient = VWorld.Server.GetExistingSystemManaged<ServerBootstrapSystem>()._ApprovedUsersLookup[userIndex];
            var userEntity = serverClient.UserEntity;
            var user = VWorld.Server.EntityManager.GetComponentData<User>(userEntity);
            var player = user.LocalCharacter.GetEntityOnServer();
            
            // Add the player to the list of player entities
            playerEntityIndices.Add(player.Index);
        }
        catch (System.Exception ex)
        {
            Plugin.LogInstance.LogError(ex);
        }

    }
}