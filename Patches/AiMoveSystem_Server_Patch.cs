using System;
using System.Linq;
using BepInEx.Logging;
using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using SpiderKiller.extensions;

namespace SpiderKiller.Patches;

[HarmonyPatch(typeof(AiMoveSystem_Server), "OnUpdate")]
// ReSharper disable once InconsistentNaming
public class AiMoveSystem_Server_Patch
{
    // ReSharper disable once InconsistentNaming
    private static ManualLogSource _log => Plugin.LogInstance;

    private static DateTime _noUpdateBefore = DateTime.MinValue;

    private static int _totalcullamount;

    private static Entity _queenEntity = Entity.Null;

    private static PrefabGUID _spiderQueen = new(-548489519);

    private static bool _queenDowned;

    private static float lastDownedTime = 0f;

    public static void Prefix(AiMoveSystem_Server __instance)
    {
        try
        {
            if (!Settings.ENABLE_CULLING.Value) return;
            if (!VWorld.IsServer) return;
            if (_noUpdateBefore > DateTime.Now)
            {
                return;
            }

            _noUpdateBefore = DateTime.Now.AddSeconds(Settings.CULL_WAIT_TIME.Value);


            var emp = __instance.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<PlayerCharacter>())
                .ToEntityArray(Allocator.Temp);

            foreach (var player in emp)
            {
                if (Settings.CULL_QUEEN.Value)
                {
                    if (!_queenDowned || Time.time - lastDownedTime >= 10f * 60f)
                    {
                        _queenEntity = SpiderUtil.GetQueen(player, 10f);
                        if (_queenEntity != Entity.Null)
                        {
                            SpiderUtil.DownQueen(_queenEntity);
                            _log.LogInfo("Queen downed");
                            _queenDowned = true;
                            lastDownedTime = Time.time;
                            _queenEntity = Entity.Null;
                        }
                    }
                }

                var spiders = SpiderUtil.ClosestSpiders(player, Settings.CULL_RANGE.Value);
                var count = spiders.Count;
                var remaining = count;

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
            }

            CheckForCritters(__instance);
        }
        catch (Exception e)
        {
            _log.LogError(e.Message);
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
        DeathUtilities.Kill(VWorld.Server.EntityManager, spider, dead, deathEvent);
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
        var threshold = Settings.EXTRA_CULL_REWARD_THRESHOLD.Value;
        var dropAmount = Settings.SILKWORM_GIVE_AMOUNT.Value;
        var silkworm = new PrefabGUID(-11246506);

        var currentCullAmount = GetCullAmount();
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
            GiveDrop.AddItemToInventory(player, silkworm, dropAmount * i);
            ResetCullAmount();
            break;
        }
    }
}