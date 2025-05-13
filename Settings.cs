using BepInEx.Configuration;


namespace SpiderKiller
{
	public static class Settings
	{
		public static ConfigEntry<bool> ENABLE_UNGORA_UNLOCK { get; private set; }
		public static ConfigEntry<bool> ENABLE_CULLING { get; private set; }
		public static ConfigEntry<bool> ENABLE_EXTRA_CULL_REWARD { get; private set; }
		public static ConfigEntry<bool> CULL_QUEEN { get; private set; }
		public static ConfigEntry<double> CULL_WAIT_TIME { get; private set; }
		public static ConfigEntry<float> CULL_RANGE { get; private set; }
		public static ConfigEntry<int> SILKWORM_GIVE_AMOUNT { get; private set; }
		public static ConfigEntry<bool> DISALLOW_SPIDERLING_VERMINNEST { get; private set; }

		internal static void Initialize(ConfigFile config)
		{
			ENABLE_UNGORA_UNLOCK = config.Bind<bool>("Server", "enableUngoraUnlock", true, "Enable automatically unlocking the ungora boss progression (no need to kill her to get the unlocks)");
			ENABLE_CULLING = config.Bind<bool>("Server", "enableCulling", true, "Enable culling of spiders");
			ENABLE_EXTRA_CULL_REWARD = config.Bind<bool>("Server", "enableExtraCullReward", false, "Enables the extra cull reward of silkworms");
			CULL_QUEEN = config.Bind<bool>("Server", "enableQueenCull", false, "Enable culling of Ungora The Spider Queen VBlood boss (this will turn off her aggro and she will die in one hit))");
			CULL_WAIT_TIME = config.Bind<double>("Server", "cullWaitTime", 2, "Time in seconds to wait before culling spiders again");
			CULL_RANGE = config.Bind<float>("Server", "cullRange", 50f, "Range to check for spiders to cull (5=1tile)");
			SILKWORM_GIVE_AMOUNT = config.Bind<int>("Server", "silkwormGiveAmount", 1, "Amount of silkworms to for each spider");
			DISALLOW_SPIDERLING_VERMINNEST = config.Bind<bool>("Server", "disallowSpiderlingVerminNest", true, "Disallow spawning of spiderlings from vermin nests WARNING: this will still use up resources it will just not spawn them!");
		}
	}
}
