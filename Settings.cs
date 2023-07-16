using BepInEx.Configuration;
using BepInEx.Logging;
using System.Collections.Generic;


namespace SpiderKiller
{
	public static class Settings
	{
		private static ManualLogSource log => Plugin.LogInstance;

		public static ConfigEntry<bool> ENABLE_CULLING;
		public static ConfigEntry<bool> ENABLE_EXTRA_CULL_REWARD;
		public static ConfigEntry<double> CULL_WAIT_TIME;
		public static ConfigEntry<int> EXTRA_CULL_REWARD_THRESHOLD;
		public static ConfigEntry<float> CULL_RANGE;
		public static ConfigEntry<int> SILKWORM_GIVE_AMOUNT;
		
		internal static void Initialize(ConfigFile config)
		{
			ENABLE_CULLING = config.Bind<bool>("Server", "enableCulling", true, "Enable culling of spiders");
			ENABLE_EXTRA_CULL_REWARD = config.Bind<bool>("Server", "enableExtraCullReward", false, "Enables the extra cull reward of silkworms");
			CULL_WAIT_TIME = config.Bind<double>("Server", "cullWaitTime", 0.5, "Time in seconds to wait before culling spiders again");
			EXTRA_CULL_REWARD_THRESHOLD = config.Bind<int>("Server", "cullAmountThreshold", 5, "Amount of spiders to cull before rewarding extra silkworm(s)");
			CULL_RANGE = config.Bind<float>("Server", "cullRange", 50f, "Range to check for spiders to cull (5=1tile)");
			SILKWORM_GIVE_AMOUNT = config.Bind<int>("Server", "silkwormGiveAmount", 1, "Amount of silkworms to give when culling 'cullAmountThreshold' worth of spiders");
		}
	}
}
