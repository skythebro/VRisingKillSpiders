using System.Collections.Generic;
using System.Linq;
using ProjectM;
using ProjectM.Physics;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using VAMP;
using VampireCommandFramework;

namespace SpiderKiller;

internal static class SpiderUtil
{
    private static NativeArray<Entity> GetSpiders()
    {
         // var entityManager = Core.Server.EntityManager;
         // var entities = entityManager.GetAllEntities().ToArray();
         //
         // var subset = entities.Where(x => entityManager.HasComponent<Team>(x) && entityManager.GetComponentData<Team>(x).FactionIndex == 21 && !entityManager.HasComponent<Dead>(x) && !entityManager.HasComponent<DestroyTag>(x)).ToList();

        
        var spiderQuery = Core.Server.EntityManager.CreateEntityQuery(new EntityQueryDesc()
        {
            All = new[] {
                ComponentType.ReadOnly<LocalToWorld>(),
                ComponentType.ReadOnly<Team>(),
                ComponentType.ReadOnly<AiMoveSpeeds>(),
                ComponentType.ReadOnly<AiMove_Server>()
                
            },
            None = new[] { ComponentType.ReadOnly<ServantConvertable>(), ComponentType.ReadOnly<BlueprintData>(), ComponentType.ReadOnly<PhysicsRubble>(), ComponentType.ReadOnly<Dead>(), ComponentType.ReadOnly<DestroyTag>() },
            Options = EntityQueryOptions.IncludeDisabled
        });
        
        return spiderQuery.ToEntityArray(Allocator.Temp);
        //not efficient enough
        //return subset;
    }

    internal static List<Entity> ClosestSpiders(Entity e, float radius,int team = 25)
    {
        var spiders = GetSpiders();
        var results = new List<Entity>();
        if (Core.Server.EntityManager.TryGetComponentData<LocalToWorld>(e, out var localToWorld))
        {
            var origin = localToWorld.Position;
            foreach (var spider in spiders)
            {
                var position = Core.Server.EntityManager.GetComponentData<LocalToWorld>(spider).Position;
                var distance = UnityEngine.Vector3.Distance(origin, position); // wait really?
                var em = Core.Server.EntityManager;
                if (!em.HasComponent<Team>(spider))
                {
                    continue;
                }
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

        return new List<Entity>();
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

        queen.WithComponentDataC((ref AggroConsumer ac) =>
        {
            ac.Active._Value = false;
        });
        
        queen.WithComponentDataC((ref UnitLevel lvl) => { lvl.Level = new ModifiableInt(1); });
        
        queen.WithComponentDataC((ref Vision vs) => { vs.Range = new ModifiableFloat(0); });
        
        queen.WithComponentDataC((ref UnitStats us) =>
        {
            us.PhysicalPower = new ModifiableFloat(0);
            us.PassiveHealthRegen = new ModifiableFloat(0);
            us.HealthRecovery = new ModifiableFloat(0);
            us.SpellPower = new ModifiableFloat(0); });
        
        
#if DEBUG
        Plugin.LogInstance.LogMessage("Queen got downed");
#endif
        return true;
    }
}