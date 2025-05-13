using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using Stunlock.Network;
using VAMP;

namespace SpiderKiller.Patches;

[HarmonyPatch]
public static class InitializePlayer_Patch
{
    public static List<int> playerEntityIndices = new();
    
    [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
    [HarmonyPostfix]
    public static void OnUserConnected_Patch(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
    {
        try
        {
            //RespawnAi(Entity fromCharacter, RespawnAiDebugEvent clientEvent);

            var userIndex = Core.Server.GetExistingSystemManaged<ServerBootstrapSystem>()
                ._NetEndPointToApprovedUserIndex[netConnectionId];
            var serverClient = Core.Server.GetExistingSystemManaged<ServerBootstrapSystem>()
                ._ApprovedUsersLookup[userIndex];
            var userEntity = serverClient.UserEntity;
            var user = Core.Server.EntityManager.GetComponentData<User>(userEntity);
            var player = user.LocalCharacter.GetEntityOnServer();

            if (Settings.ENABLE_UNGORA_UNLOCK.Value) // unlock Ungora progression if enabled
            {
                UnlockVBlood debugEvent = new UnlockVBlood();
                debugEvent.VBlood = new PrefabGUID(-548489519);
                UnlockProgressionDebugEvent evt = new UnlockProgressionDebugEvent();
                DebugEventsSystem debugEventsSystem = Core.Server.GetExistingSystemManaged<DebugEventsSystem>();

                evt.PrefabGuid = new PrefabGUID(574648849);
                debugEventsSystem.UnlockProgression(
                    new FromCharacter { Character = user.LocalCharacter._Entity, User = userEntity }, evt);
                evt.PrefabGuid = new PrefabGUID(693361325);
                debugEventsSystem.UnlockProgression(
                    new FromCharacter { Character = user.LocalCharacter._Entity, User = userEntity }, evt);
                debugEventsSystem.UnlockVBloodEvent(debugEventsSystem, debugEvent,
                    new FromCharacter { Character = user.LocalCharacter._Entity, User = userEntity });
            }
            // Add the player to the list of player entities
            playerEntityIndices.Add(player.Index);
        }
        catch (System.Exception ex)
        {
            Plugin.LogInstance.LogError(ex);
        }
    }
}