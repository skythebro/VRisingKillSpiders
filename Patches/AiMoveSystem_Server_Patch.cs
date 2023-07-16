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
                var spiders = SpiderUtil.ClosestSpiders(player, Settings.CULL_RANGE.Value);
                var count = spiders.Count;
                var remaining = count;
                foreach (var spider in spiders.TakeWhile(s => remaining != 0))
                {
                    spider.WithComponentDataC((ref Health t) =>
                    {
                        t.Value = -10000;
                        t.TimeOfDeath = Time.time;
                        t.IsDead = true;
                    });
                    //StatChangeUtility.KillEntity(__instance.EntityManager, spider, player, Time.time); 
                    remaining--;
                }

                if (count <= 0) continue;
                _log.LogDebug($"Killed {count} spiders.");
                if (!Settings.ENABLE_EXTRA_CULL_REWARD.Value) continue;
                GiveExtraCullReward(count, player);
            }
            
            
            
            _log.LogInfo($"MaxActiveCritterSpawns before: {GlobalCritterSpawnManager.MaxActiveCritterSpawns}");
            GlobalCritterSpawnManager.MaxActiveCritterSpawns = 0;
            _log.LogInfo($"MaxActiveCritterSpawns after: {GlobalCritterSpawnManager.MaxActiveCritterSpawns}");
            var em = __instance.EntityManager;
            
            
            var critters = em.CreateEntityQuery(ComponentType.ReadOnly<Critter>())
                .ToEntityArray(Allocator.Temp);
            if (critters.Length == 0) return;
            foreach (var critter in critters)
            {
                _log.LogInfo($"CritterGroup:");
                critter.WithComponentData((ref CritterGroup cg) =>
                {
                    _log.LogInfo($"cg.NumCritters: {cg.NumCritters}");
                    _log.LogInfo($"cg.State: {cg.State}");
                    _log.LogInfo($"cg.BaseCritterGuid: {cg.BaseCritterGuid}");
                });
            }
        }
        catch (Exception e)
        {
            _log.LogError(e.Message);
        }
    }

    private struct CullReward
    {
        public int Threshold;
        public int DropAmount;
    }

    private static readonly CullReward[] CullRewards =
    {
        new()
        {
            Threshold = Settings.EXTRA_CULL_REWARD_THRESHOLD.Value, DropAmount = Settings.SILKWORM_GIVE_AMOUNT.Value
        },
        new()
        {
            Threshold = Settings.EXTRA_CULL_REWARD_THRESHOLD.Value * 2,
            DropAmount = Settings.SILKWORM_GIVE_AMOUNT.Value * 2
        },
        new()
        {
            Threshold = Settings.EXTRA_CULL_REWARD_THRESHOLD.Value * 4,
            DropAmount = Settings.SILKWORM_GIVE_AMOUNT.Value * 4
        },
        new()
        {
            Threshold = Settings.EXTRA_CULL_REWARD_THRESHOLD.Value * 8,
            DropAmount = Settings.SILKWORM_GIVE_AMOUNT.Value * 8
        },
        new()
        {
            Threshold = Settings.EXTRA_CULL_REWARD_THRESHOLD.Value * 16,
            DropAmount = Settings.SILKWORM_GIVE_AMOUNT.Value * 16
        },
        new()
        {
            Threshold = Settings.EXTRA_CULL_REWARD_THRESHOLD.Value * 32,
            DropAmount = Settings.SILKWORM_GIVE_AMOUNT.Value * 32
        }
    };

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

    private static void GiveExtraCullReward(int count, Entity player)
    {
        var silkworm = new PrefabGUID(-11246506);
        AddCullAmount(count);
        var currentCullAmount = GetCullAmount();

        foreach (var reward in CullRewards)
        {
            if (currentCullAmount <= reward.Threshold) continue;
            GiveDrop.AddItemToInventory(player, silkworm, reward.DropAmount);
            ResetCullAmount();
            break;
        }
    }
}