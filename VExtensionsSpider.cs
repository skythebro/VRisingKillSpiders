using System;
using BepInEx.Logging;
using Bloodstone.API;
using Stunlock.Core;
using Unity.Entities;

namespace SpiderKiller.extensions
{
    public static class VExtensionsSpider
    {
        private static ManualLogSource _log => Plugin.LogInstance;
        public static void WithComponentDataC<T>(this Entity entity, ActionRefs<T> action) where T : struct
        {
            VWorld.Game.EntityManager.TryGetComponentData<T>(entity, out T componentData);
            action(ref componentData);
            VWorld.Game.EntityManager.SetComponentData<T>(entity, componentData);
        }

        public static bool ComparePrefabGuidString(this Entity entity, PrefabGUID comparingvalue)
        {
            try
            {
                VWorld.Game.EntityManager.TryGetComponentData<PrefabGUID>(entity, out var componentData);
                return componentData.ToString()!.Equals(comparingvalue.ToString());
                
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception e)
            {
                _log.LogError("Couldn't compare component data: " + e.Message);
                return false;
            }
            
        }
        
        public delegate void ActionRefs<T>(ref T item);
    }
}