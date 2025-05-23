using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Reflection;
using BepInEx.Logging;
using UnityEngine;


namespace SpiderKiller;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("VAMP")]
[BepInDependency("gg.deca.VampireCommandFramework", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BasePlugin
{
    public static Harmony Harmony;

    public static ManualLogSource LogInstance { get; private set; }
    
    public override void Load()
    {
        LogInstance = Log;
        Harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        Settings.Initialize(Config);

        if (Application.productName == "VRising")
        {
            Log.LogWarning("This plugin is a server-only plugin!");
            return;
        }

        VAMP.Events.OnCoreLoaded += Initialize;
    }
    
    private static void Initialize()
    {
        if (Application.productName == "VRising")
        {
            return;
        }
        
        LogInstance.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        
        LogInstance.LogInfo("Looking if VCF is installed:");
        if (VCFCompat.Commands.Enabled)
        {
            VCFCompat.Commands.Register();
        }
        else
        {
            LogInstance.LogInfo("This mod has extra admin commands. Install VampireCommandFramework to use them.");
        }
    }

    public override bool Unload()
    {
        Harmony?.UnpatchSelf();
        return true;
    }
}