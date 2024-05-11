using System.Collections.Generic;
using System.Linq;
using Bloodstone.API;
using ProjectM;
using ProjectM.Gameplay.Scripting;
using SpiderKiller.extensions;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using VampireCommandFramework;

namespace SpiderKiller;

internal static class SpiderUtil
{
    private static NativeArray<Entity> GetSpiders()
    {
        var spiderQuery = VWorld.Server.EntityManager.CreateEntityQuery(new EntityQueryDesc()
        {
            All = new[] {
                ComponentType.ReadOnly<LocalToWorld>(),
                ComponentType.ReadOnly<Team>()
            },
            None = new[] { ComponentType.ReadOnly<Dead>(), ComponentType.ReadOnly<DestroyTag>() }
        });

        return spiderQuery.ToEntityArray(Allocator.Temp);
    }

    internal static List<Entity> ClosestSpiders(Entity e, float radius,int team = 21)
    {
        var spiders = GetSpiders();
        var results = new List<Entity>();
        var origin = VWorld.Server.EntityManager.GetComponentData<LocalToWorld>(e).Position;

        foreach (var spider in spiders)
        {
            var position = VWorld.Server.EntityManager.GetComponentData<LocalToWorld>(spider).Position;
            var distance = UnityEngine.Vector3.Distance(origin, position); // wait really?
            var em = VWorld.Server.EntityManager;
            if (distance < radius && em.GetComponentData<Team>(spider).FactionIndex == team)
            {
#if DEBUG
                Plugin.LogInstance.LogMessage("A spider found");
#endif  
                results.Add(spider);
            }
        }

        return results;
    }
    
    internal static Entity GetQueen(ChatCommandContext ctx)
    {
        var spiderQueen = new PrefabGUID(-548489519);
        var spiders = ClosestSpiders(ctx.Event.SenderCharacterEntity, Settings.CULL_RANGE.Value);
        var count = spiders.Count;
        var remaining = count;

        foreach (var spider in spiders.TakeWhile(_ => remaining != 0))
        {
            var isQueen = spider.ComparePrefabGuidString(spiderQueen);
            if (isQueen)
            {
#if DEBUG
                Plugin.LogInstance.LogMessage("Queen found");
#endif                
                return spider;
            }
            
            remaining--;
        }
        return Entity.Null;
    }
    
    internal static Entity GetQueen(Entity player, float range)
    {
        var spiderQueen = new PrefabGUID(-548489519);
        var spiders = ClosestSpiders(player, range);
        var count = spiders.Count;
        var remaining = count;

        foreach (var spider in spiders.TakeWhile(_ => remaining != 0))
        {
            var isQueen = spider.ComparePrefabGuidString(spiderQueen);
            if (isQueen)
            {
                return spider;
            }
            
            remaining--;
        }
        return Entity.Null;
    }
    
    internal static bool DownQueen(Entity queen)
    {
        if (queen == Entity.Null)
        {
            return false;
        }

        queen.WithComponentDataC((ref Health h) =>
        {
            h.Value = 0.1f;
            h.MaxRecoveryHealth = 0.1f;
            h.MaxHealth._Value = 0.1f;
        });

        queen.WithComponentDataC((ref AggroConsumer ac) => { ac.Active._Value = false; });
        
        queen.WithComponentDataC((ref UnitLevel lvl) => { lvl.Level = new ModifiableInt(1); });
        
        queen.WithComponentDataC((ref Vision vs) => { vs.Range = new ModifiableFloat(0); });
        
        queen.WithComponentDataC((ref UnitStats us) =>
        {
            us.PhysicalPower = new ModifiableFloat(0);
            us.PassiveHealthRegen = new ModifiableFloat(0);
            us.HealthRecovery = new ModifiableFloat(0);
            us.ShieldAbsorbModifier = new ModifiableFloat(0);
            us.SpellPower = new ModifiableFloat(0); });
        
        queen.WithComponentDataC((ref Script_ApplyBuffUnderHealthThreshold_DataServer abuhtds) =>
        {
            abuhtds.HealthFactor = new ModifiableFloat(0.1f);
            abuhtds.ThresholdMet = false;
        });
        
        
        return true;
    }
}