# KillSpiders
For a full list of changes check on my github page in the [releases](https://github.com/skythebro/VRisingKillSpiders/releases) section.

## Installation (Manual)

* Install [BepInEx](https://docs.bepinex.dev/master/articles/user_guide/installation/index.html)
* Install [Bloodstone](https://v-rising.thunderstore.io/package/deca/Bloodstone) into (VRising server folder)/BepInEx/plugins
* (optional) Install [VampireCommandFramework](https://v-rising.thunderstore.io/package/deca/VampireCommandFramework/) into (VRising server folder)/BepInEx/plugins
* (optional) Install [ServerLaunchFix](https://v-rising.thunderstore.io/package/Mythic/ServerLaunchFix/) into (VRising server folder)/BepInEx/plugins
* Extract [KillSpiders.dll](https://thunderstore.io/package/download/Skies/SpiderKiller/1.2.0/) into (VRising server folder)/BepInEx/plugins

## How to use

* If you didn't change anything in the config the mod will work by killing all spiders (except for the VBlood Ungora Queen) in a 50 (10 tile) range every 0.5 seconds around all the players in the server.

Features:
- Kills spiders within a certain range of the player and drops their loot on the ground.
- Optional config setting to add extra silkworm drops to inventory.
- Gives you Ungora's VBlood rewards if you use the command. (WIP need to restart server for it to take effect)
- Kill Ungora command for drops. (May show red un-interactable shim if you are still tracking her(not sure if that's a game feature))
- Stops tiny spider critters from spawning. (random event(hopefully))

Optional admin commands (requires [VampireCommandFramework](https://v-rising.thunderstore.io/package/deca/VampireCommandFramework/)):
- Use the `.sKillUtil k(ill) [range]` command to kill spiders manually if you don't want to let the mod run automatically.
- Use the `.sKillUtil tp(teleportToMe) [range]` command to teleport spiders to you. (if you would want that...)
- Use the `.sKillUtil uu(unlockungora)` command to unlock all of Ungora's VBlood rewards. (WIP need to restart server for it to take effect)
- Use the `.sKillUtil kq(killqueen)` command to kill Ungora this will drop loot but you wont be able to drink her blood. (May show red un-interactable shim if you are still tracking her I think that's a game feature)
- Use the `.sKillUtil iq(injurequeen)` command to bring Ungora to almost 0 health so 1 hit should kill her.

### Configuration
The config will generate in _(VRising folder)/VRising_Server/BepInEx/config/KillSpiders.cfg_ after first boot of the server.

```
[Server]

## Enable culling of spiders
# Setting type: Boolean
# Default value: true
enableCulling = true

## Enables the extra cull reward of silkworms
# Setting type: Boolean
# Default value: false
enableExtraCullReward = false

## Enable culling of Ungora The Spider Queen VBlood boss (WIP will keep killing boss and keep dropping loot if you're in range)
# Setting type: Boolean
# Default value: false
enableQueenCull = false

## Time in seconds to wait before culling spiders again
# Setting type: Double
# Default value: 0.5
cullWaitTime = 0.5

## Amount of spiders to cull before rewarding extra silkworm(s)
# Setting type: Int32
# Default value: 5
cullAmountThreshold = 5

## Range to check for spiders to cull (5=1tile)
# Setting type: Single
# Default value: 50
cullRange = 50

## Amount of silkworms to give when culling 'cullAmountThreshold' worth of spiders
# Setting type: Int32
# Default value: 1
silkwormGiveAmount = 1
```

### Troubleshooting
- Make sure you install the mod on the server. If you are in a singleplayer world use [ServerLaunchFix](https://v-rising.thunderstore.io/package/Mythic/ServerLaunchFix/)
- Check your BepInEx logs on the server to make sure the latest version of both KillSpiders and Bloodstone were loaded (optionally VampireCommandFramework too).

### Support
- Open an issue on [github](https://github.com/skythebro/VRisingKillSpiders/issues)
- Ask in the V Rising Mod Community [discord](https://vrisingmods.com/discord)

### Contributors
- skythebro/skyKDG: `@realskye` on Discord
- V Rising Mod Community discord for helpful resources to mod this game and code inspiration.
- VExtensions for VExtensions.dll which was slightly changed and reused in my project due to AOT (not sure if that was the right thing to do).