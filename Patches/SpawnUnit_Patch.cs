using System.Collections.Generic;
using Stunlock.Core;
using Unity.Mathematics;
using HarmonyLib;
using ProjectM;
using Unity.Entities;
using VAMP;

namespace SpiderKiller.Patches;

[HarmonyPatch(typeof(UnitSpawnerUpdateSystem), nameof(UnitSpawnerUpdateSystem.SpawnUnit))]
public static class SpawnUnit_Patch
{
    private static readonly HashSet<PrefabGUID> BlockedPrefabGUIDs = new HashSet<PrefabGUID>
    {
        new PrefabGUID(-764515001),   // CHAR_Spider_Baneling
        new PrefabGUID(-1004061470), // CHAR_Spider_Baneling_Summon
        new PrefabGUID(342127250),   // CHAR_Spider_Broodmother
        new PrefabGUID(-581295882),  // CHAR_Spider_Forest
        new PrefabGUID(574276383),   // CHAR_Spider_Forestling
        new PrefabGUID(2136899683),  // CHAR_Spider_Melee
        new PrefabGUID(-725251219),  // CHAR_Spider_Melee_GateBoss_Summon
        new PrefabGUID(2119230788),  // CHAR_Spider_Melee_Summon
        new PrefabGUID(-548489519),  // CHAR_Spider_Queen_VBlood
        new PrefabGUID(-943858353),  // CHAR_Spider_Queen_VBlood_GateBoss_Major
        new PrefabGUID(2103131615),  // CHAR_Spider_Range
        new PrefabGUID(1974733695),  // CHAR_Spider_Range_Summon
        new PrefabGUID(1078424589),  // CHAR_Spider_Spiderling
        new PrefabGUID(1767714956),  // CHAR_Spider_Spiderling_VerminNest
        new PrefabGUID(-18289884)    // CHAR_Spiderling_Summon
    };
    //is only for vermin nest so wont work on the main spawning system
    static bool Prefix(Entity stationEntity, PrefabGUID prefabGuid, float3 spawnBasePosition, int count, float minRange, float maxRange, float lifeTime = -1f)
    {
#if DEBUG
        Plugin.LogInstance.LogMessage($"SpawnUnit_Patch: {prefabGuid} {stationEntity} {spawnBasePosition} {count} {minRange} {maxRange} {lifeTime}");
#endif
        if (!Settings.DISALLOW_SPIDERLING_VERMINNEST.Value) return true;
        // Check if the PrefabGUID is in the blocked list
        return !BlockedPrefabGUIDs.Contains(prefabGuid);
#if DEBUG
        string name = Core.Server.GetExistingSystemManaged<PrefabCollectionSystem>()._PrefabLookupMap.GetName(prefabGuid);
        Plugin.LogInstance.LogMessage($"Blocked spawning of PrefabGUID: {prefabGuid} name: {name}");
#endif
    }
}