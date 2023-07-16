using BepInEx;
using BepInEx.Unity.IL2CPP;
using Bloodstone.API;
using HarmonyLib;
using System.Reflection;
using BepInEx.Logging;

namespace SpiderKiller;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("gg.deca.VampireCommandFramework", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("gg.deca.Bloodstone")]
[Reloadable]
public class Plugin : BasePlugin, IRunOnInitialized
{
    private Harmony _harmony;

    public static ManualLogSource LogInstance { get; private set; }
    
    public override void Load()
    {
        LogInstance = Log;
        Settings.Initialize(Config);
        
        if (!VWorld.IsServer)
        {
            Log.LogWarning("This plugin is a server-only plugin!");
        }
    }
    
    public void OnGameInitialized()
    {
        if (VWorld.IsClient)
        {
            return;
        }
        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        
        Log.LogInfo("Looking if VCF is installed:");
        if (VCFCompat.Commands.Enabled)
        {
            VCFCompat.Commands.Register();
        }
        else
        {
            Log.LogInfo("This mod has extra admin commands. Install VampireCommandFramework to use them.");
        }
    }

    public override bool Unload()
    {
        _harmony?.UnpatchSelf();
        return true;
    }
}