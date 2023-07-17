using System.Collections.Generic;
using Bloodstone.API;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

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

    internal static List<Entity> ClosestSpiders(Entity e, float radius,int team = 20)
    {
        var spiders = GetSpiders();
        var results = new List<Entity>();
        var origin = VWorld.Server.EntityManager.GetComponentData<LocalToWorld>(e).Position;

        foreach (var spider in spiders)
        {
            var position = VWorld.Server.EntityManager.GetComponentData<LocalToWorld>(spider).Position;
            var distance = UnityEngine.Vector3.Distance(origin, position); // wait really?
            var em = VWorld.Server.EntityManager;
            var getTeam = em.GetComponentDataFromEntity<Team>();
            if (distance < radius && getTeam[spider].FactionIndex == team)
            {
                results.Add(spider);
            }
        }

        return results;
    }
}