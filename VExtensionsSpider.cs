using Bloodstone.API;
using Unity.Entities;

namespace SpiderKiller.extensions
{
    public static class VExtensionsSpider
    {
        public static void WithComponentDataC<T>(this Entity entity, ActionRefs<T> action) where T : struct
        {
            VWorld.Game.EntityManager.TryGetComponentData<T>(entity, out T componentData);
            action(ref componentData);
            VWorld.Game.EntityManager.SetComponentData<T>(entity, componentData);
        }

        public delegate void ActionRefs<T>(ref T item);
    }
}