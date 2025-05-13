# KillSpiders
For a full list of changes check on my GitHub page in the [releases](https://github.com/skythebro/VRisingKillSpiders/releases) section.

Updated to 1.1 version of V Rising.

## Installation (Manual)

* Install [BepInEx](https://github.com/CrimsonMods/BepInEx/releases)
* Install [VAMP](https://thunderstore.io/c/v-rising/p/skytech6/VAMP/)
* (optional) Install [VampireCommandFramework](https://github.com/decaprime/VampireCommandFramework/releases) into (VRising server folder)/BepInEx/plugins
* (optional) Install [ServerLaunchFix](https://v-rising.thunderstore.io/package/Mythic/ServerLaunchFix/) into (VRising server folder)/BepInEx/plugins
* Extract _**KillSpiders.dll**_ into (VRising server folder)/BepInEx/plugins

## How to use
* If you didn't change anything in the config the mod will work by killing all spiders in a 50 (10 tile) range every 2 seconds around all the players in the server.
* It will not affect Ungora the spider queen by default, but if enabled and within 2 tiles of the player it'll make her one hit and make her unable to see you.
* If `enableUngoraUnlock` is set to true the Ungora progression rewards will be instantly unlocked to anyone on the server.
* If `enableUngoraUnlock` is set to false users will need to manually use the command `.uqueen` to unlock her progression rewards, 

## Updating
* Make sure to check your config after running the updated mod to see if you need to make any changes!

Features:
- Kills spiders within a certain range of the player and drops their loot on the ground. **WARNING**: if breaking cocoons spiders will spawn but will be killed the moment the cullWaitTime passed, If i find a way to block them from spawning I will update the mod.
- Unlocks Ungora progression rewards, so no need to get up close and personal. **WARNING** This will also give you the spider form. I will, if I can find a way, remove it.
- Auto "downs" Ungora The Spider Queen. This will turn off her aggro and make her one hit.
- Stops the spawning of spiderlings from vermin nests. (this will still use up resources it will just not spawn them!)
- ~~Stops tiny spider critters from spawning. (random event(hopefully))~~ WIP
- Optional config setting to add extra silkworm drops to inventory.

Optional commands (requires [VampireCommandFramework](https://v-rising.thunderstore.io/package/deca/VampireCommandFramework/)):
- Use the `.kspi [range]` command to kill spiders manually if you don't want to let the mod run automatically.
- Use the `.ttm [range]` command to teleport spiders to you. (if you would want that..., this was used to test if my command would get all spiders in the area)
- Use the `.dqueen` command to turn off ungora's aggro and make her one hit (make sure you can see her)
- Use the `.uqueen` command to unlock ungora's progression unlocks


### Configuration
The config will generate in _(VRising folder)/VRising_Server/BepInEx/config/KillSpiders.cfg_ after first boot of the server.

```
[Server]

## Enable automatically unlocking the ungora boss progression (no need to kill her to get the unlocks)
# Setting type: Boolean
# Default value: true
enableUngoraUnlock = true

## Enable culling of spiders
# Setting type: Boolean
# Default value: true
enableCulling = true

## Enables the extra cull reward of silkworms
# Setting type: Boolean
# Default value: false
enableExtraCullReward = false

## Enable culling of Ungora The Spider Queen VBlood boss (this will turn off her aggro and she will die in one hit))
# Setting type: Boolean
# Default value: false
enableQueenCull = false

## Time in seconds to wait before culling spiders again
# Setting type: Double
# Default value: 2
cullWaitTime = 2

## Range to check for spiders to cull (5=1tile)
# Setting type: Single
# Default value: 50
cullRange = 50

## Amount of silkworms to for each spider
# Setting type: Int32
# Default value: 1
silkwormGiveAmount = 1

## Disallow spawning of spiderlings from vermin nests WARNING: this will still use up resources it will just not spawn them!
# Setting type: Boolean
# Default value: true
disallowSpiderlingVerminNest = true
```

### Troubleshooting
- Make sure you install the mod on the server. If you are in a singleplayer world use [ServerLaunchFix](https://v-rising.thunderstore.io/package/Mythic/ServerLaunchFix/)
- Check your BepInEx logs on the server to make sure the latest version of both KillSpiders and VAMP were loaded (optionally VampireCommandFramework too).

### Support
- Open an issue on [GitHub](https://github.com/skythebro/VRisingKillSpiders/issues)
- Ask in the V Rising Mod Community [discord](https://vrisingmods.com/discord)

### Contributors
- skythebro/skyKDG: `@realskye` on Discord
- skytech6 for his `V Rising API Modding Platform`
- V Rising Mod Community discord for helpful resources to mod this game and code inspiration.
- VExtensions for VExtensions code which was slightly changed and reused in my project to fix AOT issues.
