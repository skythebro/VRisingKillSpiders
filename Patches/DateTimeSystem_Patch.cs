using System;
using System.Linq;
using BepInEx.Logging;
using Bloodstone.API;
using HarmonyLib;
using Il2CppSystem.Threading;
using ProjectM;
using ProjectM.Gameplay;
using ProjectM.Network;
using ProjectM.Scripting;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using SpiderKiller.extensions;
using Stunlock.Core;

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
    private static EntityManager em = VWorld.Server.EntityManager;

    public static void Prefix(ProjectM.DateTimeSystem __instance)
    {
        // Aimovesystem_server didnt want to work anymore so I had to find a replacement SunSystem is the closest thing I could find for constant updates for now
        try
        {
            if (!Settings.ENABLE_CULLING.Value) return;
            if (!VWorld.IsServer) return;
            if (_noUpdateBefore > DateTime.Now)
            {
                return;
            }

            _noUpdateBefore = DateTime.Now.AddSeconds(Settings.CULL_WAIT_TIME.Value);
            // hopefully this works because changes to AiMoveSystem_Server makes it so I cannot use the createntityquery method
            //var emp = __instance._AiMoveQuery.ToEntityArray(Allocator.Temp);
#if DEBUG
            _log.LogMessage("Reached query");
#endif

            var emp = InitializePlayer_Patch.playerEntityIndices;

            foreach (var playerIndex in emp)
            {
                var player = em.GetEntityByEntityIndex(playerIndex);
#if DEBUG
                _log.LogMessage("Player found");
#endif
                if (Settings.CULL_QUEEN.Value)
                {
                    var dayNightCycle = VWorld.Server.GetExistingSystemManaged<ServerScriptMapper>()._ServerGameManager
                        .DayNightCycle;
                    var now = dayNightCycle.GameDateTimeNow;
                    double dayDurationInSeconds = dayNightCycle.DayDurationInSeconds;
                    double secondsPerInGameHour = dayDurationInSeconds / 24;
                    double hoursForTenMinutes = (9 * 60) / secondsPerInGameHour;
#if DEBUG
                    _log.LogMessage( "now.day "+now.Day+" now.Hour: " + now.Hour + " lastDownedTime.Day " + lastDownedTime.Day + " lastDownedTime.Hour + hoursForFiveMinutes " + Math.Floor(lastDownedTime.Hour + hoursForTenMinutes));
#endif
                    if (!_queenDowned || Time.time - lastDownedTimesecondcheck >= 10f * 60f || now.Year > lastDownedTime.Year || (now.Year == lastDownedTime.Year && now.Month > lastDownedTime.Month) || (now.Year == lastDownedTime.Year && now.Month == lastDownedTime.Month && now.Day > lastDownedTime.Day) || (now.Year == lastDownedTime.Year && now.Month == lastDownedTime.Month && now.Day == lastDownedTime.Day && now.Hour >= Math.Floor(lastDownedTime.Hour + hoursForTenMinutes))) {
#if DEBUG
                        _log.LogMessage( "Queen not downed or time passed");
#endif
                        
                        _queenEntity = SpiderUtil.GetQueen(player, Settings.CULL_RANGE.Value);
                        if (_queenEntity != Entity.Null)
                        {
#if DEBUG
                            _log.LogMessage("Queen found");
#endif
                            SpiderUtil.DownQueen(_queenEntity);

                            _queenDowned = true;
                            lastDownedTimesecondcheck = Time.time;
                            lastDownedTime = dayNightCycle.GameDateTimeNow;
                            _queenEntity = Entity.Null;
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
        }
        catch (Exception e)
        {
            _log.LogError(e.Message);
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
#if DEBUG
        Plugin.LogInstance.LogMessage("A spider got killed");
#endif
        DeathUtilities.Kill(VWorld.Server.EntityManager, spider, dead, deathEvent, deathReason);
        // Destroys the entity without giving any drops 
        // DestroyUtility.CreateDestroyEvent(VWorld.Server.EntityManager, spider, DestroyReason.Default, DestroyDebugReason.None);
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
        var threshold = 1; //changing this to any higher than 1 will cause you to most of the time not get anything due to the way the loop works
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
}