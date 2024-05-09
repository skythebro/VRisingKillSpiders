using System.Linq;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Stunlock.Core;
using UnityEngine;

namespace SpiderKiller.VCFCompat
{
    using ProjectM;
    using Unity.Entities;
    using Unity.Transforms;
    using VampireCommandFramework;
    using Bloodstone.API;

    public static partial class Commands
    {
        private static ManualLogSource _log => Plugin.LogInstance;

        static Commands()
        {
            Enabled = IL2CPPChainloader.Instance.Plugins.TryGetValue("gg.deca.VampireCommandFramework", out var info);
            if (Enabled) _log.LogWarning($"VCF Version: {info.Metadata.Version}");
        }

        public static bool Enabled { get; private set; }

        public static void Register() => CommandRegistry.RegisterAll();

        public record Spider(Entity Entity);

        public class SpiderRemover : CommandArgumentConverter<Spider, ChatCommandContext>
        {
            private const float Radius = 25f;

            public override Spider Parse(ChatCommandContext ctx, string input)
            {
                var spiders = SpiderUtil.ClosestSpiders(ctx.Event.SenderCharacterEntity, Radius);
                var em = VWorld.Server.EntityManager;

                foreach (var spider in spiders)
                {
                    var team = em.GetComponentData<Team>(spider);
                    var isUnit = Team.IsInUnitTeam(team);
                    var isNeutral = Team.IsInNeutralTeam(team);
                    if (isNeutral || !isUnit) continue;
                    return new Spider(spider);
                }

                throw ctx.Error($"Could not find a spider within {Radius:F1}");
            }
            
            public class SpiderCommands
            {
                private static Entity _queenEntity = Entity.Null;

/*
                private static bool _unlocked;
                private static bool _unlockedThisOne;
                private static bool _unlockedThisOtherOne;
                private static int _unlockedCount = 0;
*/
                [Command("downqueen", shortHand: "dqueen", adminOnly: false, description: "Downs Ungora",
                    usage: "Usage: .dqueen")]
                public void downQueen(ChatCommandContext ctx)
                {
                    _queenEntity = SpiderUtil.GetQueen(ctx);
                    var downedQueen = SpiderUtil.DownQueen(_queenEntity);
                    //_queenEntity.WithComponentDataC((ref WoundedConstants wc) => { wc.HealthFactor = 0.01f; });
                    ctx.Reply(downedQueen ? "The queen can't see you and is one hit!" : "Failed to down the queen, are you in range?");
                }

/*
                [Command("unlockungora", shortHand: "uu", adminOnly: true, description: "Unlocks all Ungora unlocks",
                    usage: "Usage: uu(unlockungora)")]
                public void UnlockUngoraUnlocks(ChatCommandContext ctx)
                {
                    if (_unlocked)
                    {
                        ctx.Reply("Already unlocked all Ungora unlocks");
                        return;
                    }

                    var hasProgression = ProgressionUtility.TryGetProgressionEntity(VWorld.Server.EntityManager,
                        ctx.Event.SenderUserEntity, out var progEntity);
                    if (!hasProgression) return;

                    foreach (var unlockedVblood in VWorld.Server.EntityManager.GetBuffer<UnlockedVBlood>(progEntity))
                    {
                        if (new PrefabGUID(-548489519).Equals(unlockedVblood.VBlood))
                        {
                            _unlockedThisOne = true;
                            _unlockedCount++;
                        }
                    }

                    if (!_unlockedThisOne)
                    {
                        var unlockedVBlood = new UnlockedVBlood
                        {
                            VBlood = new PrefabGUID(-548489519)
                        };
                        VWorld.Server.EntityManager.GetBuffer<UnlockedVBlood>(progEntity).Add(unlockedVBlood);
                    }

                    foreach (var uRecipeElement in VWorld.Server.EntityManager.GetBuffer<UnlockedRecipeElement>(
                                 progEntity))
                    {
                        if (new PrefabGUID(-1229432962).Equals(uRecipeElement.UnlockedRecipe) &&
                            uRecipeElement.UserHasRequiredContentFlags)
                        {
                            _unlockedThisOne = true;
                            _unlockedCount++;
                        }

                        if (new PrefabGUID(1294560299).Equals(uRecipeElement.UnlockedRecipe) &&
                            uRecipeElement.UserHasRequiredContentFlags)
                        {
                            _unlockedThisOtherOne = true;
                            _unlockedCount++;
                        }
                    }

                    if (!_unlockedThisOne)
                    {
                        var unlockedRecipeElement = new UnlockedRecipeElement
                        {
                            UnlockedRecipe = new PrefabGUID(-1229432962),
                            UserHasRequiredContentFlags = true
                        };
                        VWorld.Server.EntityManager.GetBuffer<UnlockedRecipeElement>(progEntity)
                            .Add(unlockedRecipeElement);
                    }

                    if (!_unlockedThisOtherOne)
                    {
                        var unlockedRecipeElement2 = new UnlockedRecipeElement
                        {
                            UnlockedRecipe = new PrefabGUID(1294560299),
                            UserHasRequiredContentFlags = true
                        };
                        VWorld.Server.EntityManager.GetBuffer<UnlockedRecipeElement>(progEntity)
                            .Add(unlockedRecipeElement2);
                    }

                    foreach (var uAbilityElement in VWorld.Server.EntityManager.GetBuffer<UnlockedAbilityElement>(
                                 progEntity))
                    {
                        if (new PrefabGUID(69268894).Equals(uAbilityElement.Source) &&
                            new PrefabGUID(-266609153).Equals(uAbilityElement.UnlockedAbility))
                        {
                            _unlockedThisOne = true;
                            _unlockedCount++;
                        }
                    }

                    if (!_unlockedThisOne)
                    {
                        var unlockedAbilityElement = new UnlockedAbilityElement
                        {
                            Source = new PrefabGUID(69268894),
                            UnlockedAbility = new PrefabGUID(-266609153)
                        };
                        VWorld.Server.EntityManager.GetBuffer<UnlockedAbilityElement>(progEntity)
                            .Add(unlockedAbilityElement);
                    }

                    foreach (var uProgressionElement in VWorld.Server.EntityManager
                                 .GetBuffer<UnlockedProgressionElement>(progEntity))
                    {
                        if (new PrefabGUID(69268894).Equals(uProgressionElement.UnlockedPrefab))
                        {
                            _unlockedThisOne = true;
                            _unlockedCount++;
                        }
                    }

                    if (!_unlockedThisOne)
                    {
                        var unlockedProgressionElement = new UnlockedProgressionElement
                        {
                            UnlockedPrefab = new PrefabGUID(69268894)
                        };
                        VWorld.Server.EntityManager.GetBuffer<UnlockedProgressionElement>(progEntity)
                            .Add(unlockedProgressionElement);
                    }

                    if (_unlockedCount == 5)
                    {
                        ctx.Reply("Already unlocked all Ungora unlocks");
                        _unlocked = true;
                        return;
                    }

                    VWorld.Server.EntityManager.GetBuffer<VBloodAbilityOwnerData>(progEntity)
                        .Add(new VBloodAbilityOwnerData
                        {
                            VBloodAbilityBuff = new PrefabGUID(69268894)
                        });
                    _unlocked = true;
                    ctx.Reply("Unlocked all Ungora unlocks");
                }

                [Command("killqueen", shortHand: "kq", adminOnly: true,
                    description: "Kills Ungora but you won't be able to drink her blood",
                    usage: "Usage: kq(killqueen)")]
                public void SpiderQueenKill(ChatCommandContext ctx)
                {
                    _queenEntity = SpiderUtil.GetQueen(ctx);
                    if (_queenEntity == Entity.Null)
                    {
                        ctx.Reply("Queen not found");
                        return;
                    }

                    StatChangeUtility.KillEntity(VWorld.Server.EntityManager, _queenEntity,
                        ctx.Event.SenderCharacterEntity, Time.time, true);
                    //DeathUtilities.Kill(VWorld.Server.EntityManager, _queenEntity, dead, deathEvent);
                    ctx.Reply($"Killed the queen");
                }
*/
                [Command("teleportToMeSpiders", shortHand: "sptp", adminOnly: true,
                    description: "Teleports all spiders(Entities with FactionIndex 20) to you in a defined range",
                    usage: "Usage: .sptp [range]")]
                public void TeleportToPlayer(ChatCommandContext ctx, float range = 10f, int factionIndex = 20)
                {
                    if (range > 50f) range = 50f;
                    var spiders = SpiderUtil.ClosestSpiders(ctx.Event.SenderCharacterEntity, range, factionIndex);
                    var count = spiders.Count;
                    var userPos = VWorld.Server.EntityManager
                        .GetComponentData<Translation>(ctx.Event.SenderUserEntity).Value;
                    var remaining = count;
                    foreach (var spider in spiders.TakeWhile(_ => remaining != 0))
                    {
                        spider.WithComponentData((ref Translation t) => { t.Value = userPos; });
                        remaining--;
                    }

                    ctx.Reply($"Teleported {count} spiders.");
                }

                [Command("killSpiders", shortHand: "spik", adminOnly: false,
                    description: "Kills all spiders(Entities with FactionIndex 20) in a defined range",
                    usage: "Usage: .spik [range]")]
                public void KillEnemy(ChatCommandContext ctx, float range = 10f, int factionIndex = 20)
                {
                    if (range > 50f) range = 50f;
                    var spiders = SpiderUtil.ClosestSpiders(ctx.Event.SenderCharacterEntity, range, factionIndex);
                    var count = spiders.Count;
                    var remaining = count;
                    foreach (var spider in spiders.TakeWhile(_ => remaining != 0))
                    {
                        var deathEvent = new DeathEvent
                        {
                            Died = spider,
                            Killer = ctx.Event.SenderCharacterEntity,
                            Source = ctx.Event.SenderCharacterEntity
                        };
                        var dead = new Dead
                        {
                            ServerTimeOfDeath = Time.time,
                            DestroyAfterDuration = 5f,
                            Killer = ctx.Event.SenderCharacterEntity,
                            KillerSource = ctx.Event.SenderCharacterEntity,
                            DoNotDestroy = false
                        };
                        var deathreason = new DeathReason
                        {
                        };
                        DeathUtilities.Kill(VWorld.Server.EntityManager, spider, dead, deathEvent, deathreason);
                        remaining--;
                    }

                    ctx.Reply($"Killed {count} spiders.");
                }
            }
        }
    }
}