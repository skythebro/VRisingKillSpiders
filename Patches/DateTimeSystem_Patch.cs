using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay.Scripting;
using ProjectM.Scripting;
using ProjectM.Terrain;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Stunlock.Core;
using VAMP;
using VAMP.Data;

namespace SpiderKiller.Patches;

[HarmonyPatch(typeof(DateTimeSystem), nameof(DateTimeSystem.OnUpdate))]
// ReSharper disable once InconsistentNaming
public class DateTimeSystem_Patch
{
    // ReSharper disable once InconsistentNaming
    private static ManualLogSource _log => Plugin.LogInstance;

    private static DateTime _noUpdateBefore = DateTime.MinValue;

    private static int _totalcullamount;

    private static Entity _queenEntity = Entity.Null;

    private static PrefabGUID _spiderQueen = new(-548489519);

    private static bool _queenDowned;

    private static float lastDownedTimesecondcheck = 0f;
    private static GameDateTime lastDownedTime = new GameDateTime();
    
    private static readonly HashSet<PrefabGUID> BlockedPrefabGUIDs = new HashSet<PrefabGUID>
    {
        Prefabs.CHAR_Spider_Baneling, 
        Prefabs.CHAR_Spider_Baneling_Summon, 
        Prefabs.CHAR_Spider_Broodmother, 
        Prefabs.CHAR_Spider_Forest, 
        Prefabs.CHAR_Spider_Forestling, 
        Prefabs.CHAR_Spider_Melee, 
        Prefabs.CHAR_Spider_Melee_GateBoss_Summon, 
        Prefabs.CHAR_Spider_Melee_Summon, 
        Prefabs.CHAR_Spider_Queen_VBlood, 
        Prefabs.CHAR_Spider_Queen_VBlood_GateBoss_Major,
        Prefabs.CHAR_Spider_Range, 
        Prefabs.CHAR_Spider_Range_Summon, 
        Prefabs.CHAR_Spider_Spiderling, 
        Prefabs.CHAR_Spider_Spiderling_VerminNest, 
        Prefabs.CHAR_Spiderling_Summon
    };
    
    private static readonly HashSet<PrefabGUID> ReplacementPrefabGUIDs = new HashSet<PrefabGUID>
    {
       Prefabs.CHAR_Cursed_Mosquito, 
       Prefabs.CHAR_Cursed_Wolf, 
       Prefabs.CHAR_Critter_Rat,
       Prefabs.CHAR_Cursed_Bear_Standard
    };

    public static void Prefix(DateTimeSystem __instance)
    {
        try
        {
            if (!Settings.ENABLE_CULLING.Value) return;
            if (Application.productName == "VRising") return;
            if (_noUpdateBefore > DateTime.Now)
            {
                return;
            }

            _noUpdateBefore = DateTime.Now.AddSeconds(Settings.CULL_WAIT_TIME.Value);
            if (!Core.hasInitialized) return;
            EntityManager em = Core.EntityManager;
            EntityQuery querySpawnRegion = em.CreateEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<SpawnRegion>() },
                Any = new[] { ComponentType.ReadOnly<SpawnRegionSpawnSlotEntry>(), ComponentType.ReadOnly<SpawnGroupBuffer>() },
                Options = EntityQueryOptions.IncludeDisabled
            });
            foreach (Entity spawnRegion in querySpawnRegion.ToEntityArray(Allocator.Temp))
            {
                if (em.TryGetBuffer<SpawnRegionSpawnSlotEntry>(spawnRegion, out var spawnSlotEntries))
                {
                    for (int i = 0; i < spawnSlotEntries.Length; i++)
                    {
                        var entry = spawnSlotEntries[i];
                        if (!BlockedPrefabGUIDs.Contains(entry.Entity.GetPrefabGuid())) continue;
#if DEBUG
                        Plugin.LogInstance.LogMessage($"[spawnSlotEntries] current {entry.Entity.GetPrefabGuidNameString()} has spawned?: {entry.HasSpawned}");
#endif
                        var prefabGUID = ReplacementPrefabGUIDs.ElementAt(UnityEngine.Random.Range(0, ReplacementPrefabGUIDs.Count));
                        var prefabEntity = EntityQueryHelper.QueryEntityWithPrefabGUID(em, prefabGUID.GuidHash);

                        if (prefabEntity == Entity.Null) continue;
#if DEBUG
                        Plugin.LogInstance.LogMessage($"[spawnSlotEntries] with {prefabGUID.GetNamefromPrefabGuid()}");
#endif
                        entry.Entity = em.Instantiate(prefabEntity);
                        spawnSlotEntries[i] = entry;
                    }
                }

                if (!em.TryGetBuffer<SpawnGroupBuffer>(spawnRegion, out var spawnGroupBuffer))
                    continue;
                
                foreach (var group in spawnGroupBuffer)
                {
                    if (!em.TryGetBuffer<SpawnGroup_SpawnTableBuffer>(group.SpawnGroup,
                            out var spawnTableBuffer)) continue;
                    
                    for (int i = 0; i < spawnTableBuffer.Length; i++)
                    {
                        var spawnGroup = spawnTableBuffer[i];
                        if (!BlockedPrefabGUIDs.Contains(spawnGroup.Prefab)) continue;
#if DEBUG
                        Plugin.LogInstance.LogMessage($"[spawnGroup] found {spawnTableBuffer[i].Prefab.GetNamefromPrefabGuid()}");
#endif

                        spawnGroup.Prefab = ReplacementPrefabGUIDs.ElementAt(UnityEngine.Random.Range(0, ReplacementPrefabGUIDs.Count));
                        if (spawnGroup.Prefab == ReplacementPrefabGUIDs.ElementAt(3)) // is bear?
                        {
                            spawnGroup.Amount = 1;
                        }
#if DEBUG
                        Plugin.LogInstance.LogMessage($"[spawnGroup] replaced with {spawnGroup.Prefab.GetNamefromPrefabGuid()}");
#endif
                        spawnTableBuffer[i] = spawnGroup;
                    }
                }
            }
            
            
            var emp = InitializePlayer_Patch.playerEntityIndices;

            foreach (var playerIndex in emp)
            {
                var player = em.GetEntityByEntityIndex(playerIndex);
                if (Settings.CULL_QUEEN.Value)
                {
                    var dayNightCycle = Core.ServerGameManager.DayNightCycle;
                    var now = dayNightCycle.GameDateTimeNow;
                    double dayDurationInSeconds = dayNightCycle.DayDurationInSeconds;
                    double secondsPerInGameHour = dayDurationInSeconds / 24;
                    double hoursForTenMinutes = (9 * 60) / secondsPerInGameHour;
                    if (!_queenDowned || Time.time - lastDownedTimesecondcheck >= 10f * 60f ||
                        now.Year > lastDownedTime.Year ||
                        (now.Year == lastDownedTime.Year && now.Month > lastDownedTime.Month) ||
                        (now.Year == lastDownedTime.Year && now.Month == lastDownedTime.Month &&
                         now.Day > lastDownedTime.Day) || (now.Year == lastDownedTime.Year &&
                                                           now.Month == lastDownedTime.Month &&
                                                           now.Day == lastDownedTime.Day &&
                                                           now.Hour >= Math.Floor(lastDownedTime.Hour +
                                                               hoursForTenMinutes)))
                    {

                        _queenEntity = SpiderUtil.GetQueen(player, Settings.CULL_RANGE.Value);
                        if (_queenEntity != Entity.Null)
                        {
                            SpiderUtil.DownQueen(_queenEntity);

                            _queenDowned = true;
                            lastDownedTimesecondcheck = Time.time;
                            lastDownedTime = dayNightCycle.GameDateTimeNow;
                            _queenEntity = Entity.Null;
                        }
                    }
                }

                var spiders = SpiderUtil.ClosestSpiders(player, Settings.CULL_RANGE.Value);
                spiders.RemoveAll(e => e.ComparePrefabGuidString(_spiderQueen)); // not rly needed but just to be sure
                var count = spiders.Count;
                var remaining = count;
                if (count == 0) continue;
                foreach (var spider in spiders.TakeWhile(_ => remaining != 0))
                {
                    remaining--;
                    if (IsQueenSpider(spider))
                    {
                        continue;
                    }

                    KillSpider(spider, player);
                }

                if (!Settings.ENABLE_EXTRA_CULL_REWARD.Value) continue;
                AddCullAmount(count);
                GiveExtraCullReward(player);

                // not working
                // CheckForCritters(__instance);
            }
        }
        catch (Exception e)
        {
            _log.LogError($"Exception in DateTimeSystem_Patch.Prefix: {e.Message}");
            _log.LogError(e.StackTrace);
        }
    }

    private static bool IsQueenSpider(Entity spider)
    {
        return spider.ComparePrefabGuidString(_spiderQueen);
    }

    private static void KillSpider(Entity spider, Entity player)
    {
        var deathEvent = new DeathEvent
        {
            Died = spider,
            Killer = player,
            Source = player
        };
        var dead = new Dead
        {
            ServerTimeOfDeath = Time.time,
            DestroyAfterDuration = 5f,
            Killer = player,
            KillerSource = player,
            DoNotDestroy = false
        };
        var deathReason = new DeathReason();
        DeathUtilities.Kill(Core.EntityManager, spider, dead, deathEvent, deathReason);
        // Destroys the entity without giving any drops 
        // DestroyUtility.CreateDestroyEvent(Core.Server.EntityManager, spider, DestroyReason.Default, DestroyDebugReason.None);
    }

    private static void AddCullAmount(int amount)
    {
        _totalcullamount += amount;
    }

    private static int GetCullAmount()
    {
        return _totalcullamount;
    }

    private static void ResetCullAmount()
    {
        _totalcullamount = 0;
    }

    private static void CheckForCritters(ComponentSystemBase instance)
    {
        var singleSpiderGroupGuid = new PrefabGUID(-80668474);
        var spiderGroupGuid = new PrefabGUID(1076806641);
        var em = instance.EntityManager;
        // ReSharper disable once IdentifierTypo
        var crittergroups = em.CreateEntityQuery(ComponentType.ReadOnly<CritterGroup>())
            .ToEntityArray(Allocator.Temp);
        if (crittergroups.Length == 0) return;
        _log.LogInfo($"Checking for critters: {crittergroups.Length}");
        // ReSharper disable once IdentifierTypo
        foreach (var crittergroup in crittergroups)
        {
            _log.LogInfo($"CritterGroup:");
            var isSingleSpiderGroup = crittergroup.ComparePrefabGuidString(singleSpiderGroupGuid);
            var isSpiderGroup = crittergroup.ComparePrefabGuidString(spiderGroupGuid);
            if (isSingleSpiderGroup || isSpiderGroup)
            {
                crittergroup.WithComponentDataC((ref CritterGroup t) =>
                {
                    t.SpawnZoneRadius = 0;
                    t.NumCritters = 0;
                    t.MaxAliveTime = 0.1f;
                });
            }
        }
    }

    private static void GiveExtraCullReward(Entity player)
    {
        var
            threshold = 1; //changing this to any higher than 1 will cause you to most of the time not get anything due to the way the loop works
        var dropAmount = Settings.SILKWORM_GIVE_AMOUNT.Value;
        var silkworm = new PrefabGUID(-11246506);

        var currentCullAmount = GetCullAmount();
        if (currentCullAmount == 0) return;
        var i = 0;
        while (true)
        {
            if (i == 0)
            {
                i++;
            }
            else
            {
                i *= 2;
            }

            if (currentCullAmount >= threshold * i) continue;
            if (dropAmount * i > 0)
            {
                var succeeded = GiveDrop.AddItemToInventory(player, silkworm, dropAmount * i);
                ResetCullAmount();
#if DEBUG
                if (!succeeded)
                {
                    _log.LogWarning("Failed to give extra cull reward");
                }
#endif
            }

            break;
        }
    }

    public static class EntityQueryHelper
    {
        public static Entity QueryEntityWithPrefabGUID(EntityManager entityManager, int targetPrefabGUID)
        {
            // Create an EntityQuery for entities with the PrefabGUID component
            var query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<PrefabGUID>());

            // Iterate through the entities in the query
            var entities = query.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                // Get the PrefabGUID component data
                var hasGUID = entityManager.TryGetComponentData<PrefabGUID>(entity, out var prefabGUID);
                if (!hasGUID) continue;
                // Check if it matches the target value
                if (prefabGUID.GuidHash == targetPrefabGUID)
                {
                    return entity; // Return the matching entity
                }
            }

            // Return Entity.Null if no matching entity is found
            return Entity.Null;
        }
    }
}