using System;
using BepInEx.Logging;
using ProjectM;
using Stunlock.Core;
using Unity.Entities;
using VAMP;

namespace SpiderKiller
{
    public static class VExtensionsSpider
    {
        private static ManualLogSource _log => Plugin.LogInstance;
        public static void WithComponentDataC<T>(this Entity entity, ActionRefs<T> action) where T : struct
        {
            Core.Server.EntityManager.TryGetComponentData<T>(entity, out T componentData);
            action(ref componentData);
            Core.Server.EntityManager.SetComponentData<T>(entity, componentData);
        }

        public static bool ComparePrefabGuidString(this Entity entity, PrefabGUID comparingvalue)
        {
            try
            {
                Core.Server.EntityManager.TryGetComponentData<PrefabGUID>(entity, out var componentData);
                return componentData.ToString()!.Equals(comparingvalue.ToString());
                
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception e)
            {
                _log.LogError("Couldn't compare component data: " + e.Message);
                return false;
            }
            
        }
        
        public static String GetPrefabGuidNameString(this Entity entity)
        {
            try
            {
                var exists = Core.Server.EntityManager.TryGetComponentData<PrefabGUID>(entity, out var componentData);
                if (!exists) return "";
                var name = Core.Server.GetExistingSystemManaged<PrefabCollectionSystem>()._PrefabLookupMap.GetName(componentData);
                return name;
                
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception e)
            {
                _log.LogError("Couldn't compare component data: " + e.Message);
                return "";
            }
            
        }
        
        public static PrefabGUID GetPrefabGuid(this Entity entity)
        {
            try
            {
                var exists = Core.Server.EntityManager.TryGetComponentData<PrefabGUID>(entity, out var componentData);
                return !exists ? PrefabGUID.Empty : componentData;
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception e)
            {
                _log.LogError("Couldn't compare component data: " + e.Message);
                return PrefabGUID.Empty;
            }
            
        }
        
        public static String GetNamefromPrefabGuid(this PrefabGUID prefab)
        {
            try
            {
                var name = Core.Server.GetExistingSystemManaged<PrefabCollectionSystem>()._PrefabLookupMap.GetName(prefab);
                return name;
                
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception e)
            {
                _log.LogError("Couldn't compare component data: " + e.Message);
                return "";
            }
            
        }
        
        public delegate void ActionRefs<T>(ref T item);
    }
}