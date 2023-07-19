using BepInEx.Configuration;


namespace SpiderKiller
{
	public static class Settings
	{
		public static ConfigEntry<bool> ENABLE_CULLING { get; private set; }
		public static ConfigEntry<bool> ENABLE_EXTRA_CULL_REWARD { get; private set; }
		public static ConfigEntry<bool> CULL_QUEEN { get; private set; }
		public static ConfigEntry<double> CULL_WAIT_TIME { get; private set; }
		public static ConfigEntry<int> EXTRA_CULL_REWARD_THRESHOLD { get; private set; }
		public static ConfigEntry<float> CULL_RANGE { get; private set; }
		public static ConfigEntry<int> SILKWORM_GIVE_AMOUNT { get; private set; }

		internal static void Initialize(ConfigFile config)
		{
			ENABLE_CULLING = config.Bind<bool>("Server", "enableCulling", true, "Enable culling of spiders");
			ENABLE_EXTRA_CULL_REWARD = config.Bind<bool>("Server", "enableExtraCullReward", false, "Enables the extra cull reward of silkworms");
			CULL_QUEEN = config.Bind<bool>("Server", "enableQueenCull", true, "Enable culling of Ungora The Spider Queen VBlood boss (this will turn off her aggro and she will die in one hit))");
			CULL_WAIT_TIME = config.Bind<double>("Server", "cullWaitTime", 0.5, "Time in seconds to wait before culling spiders again");
			EXTRA_CULL_REWARD_THRESHOLD = config.Bind<int>("Server", "cullAmountThreshold", 5, "Amount of spiders to cull before rewarding extra silkworm(s)");
			CULL_RANGE = config.Bind<float>("Server", "cullRange", 50f, "Range to check for spiders to cull (5=1tile)");
			SILKWORM_GIVE_AMOUNT = config.Bind<int>("Server", "silkwormGiveAmount", 1, "Amount of silkworms to give when culling 'cullAmountThreshold' worth of spiders");
		}
	}
}
