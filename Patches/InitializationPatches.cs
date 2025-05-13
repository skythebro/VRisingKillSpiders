using System.Reflection;
using HarmonyLib;
using ProjectM;

namespace SpiderKiller.Patches
{
    [HarmonyPatch(typeof(SpawnTeamSystem_OnPersistenceLoad), "OnUpdate")]
    public static class InitializationPatch
    {
        [HarmonyPostfix]
        public static void OneShot_AfterLoad_InitializationPatch()
        {
            Plugin.Harmony.Unpatch((MethodBase) typeof (SpawnTeamSystem_OnPersistenceLoad).GetMethod("OnUpdate"), typeof (InitializationPatch).GetMethod(nameof (OneShot_AfterLoad_InitializationPatch)));
            Plugin.Initialize();
        }
    }
}