using System.Linq;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using ProjectM.UI;
using SpiderKiller.extensions;
using UnityEngine;

namespace SpiderKiller.VCFCompat
{
    using ProjectM;
    using Unity.Entities;
    using Unity.Transforms;
    using VampireCommandFramework;
    using Bloodstone.API;
    using Unity.Mathematics;

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
                var getTeam = em.GetComponentDataFromEntity<Team>();

                foreach (var spider in spiders)
                {
                    var team = getTeam[spider];
                    var isUnit = Team.IsInUnitTeam(team);
                    var isNeutral = Team.IsInNeutralTeam(team);
                    if (isNeutral || !isUnit) continue;
                    return new Spider(spider);
                }

                throw ctx.Error($"Could not find a spider within {Radius:F1}");
            }

            [CommandGroup("spider", "sp")]
            public class SpiderCommands
            {
                private static Entity _queenEntity = Entity.Null;
                
                private static bool _unlocked;
                private static bool _unlockedThisOne;
                private static bool _unlockedThisOtherOne;
                
                [Command("unlockungora", shortHand: "uu", adminOnly: true)]
                public void UnlockUngoraUnlocks(ChatCommandContext ctx)
                {
                    if (_unlocked)
                    {
                        ctx.Reply("Already unlocked all Ungora unlocks");
                        return;
                    }
                    
                    var hasProgression = ProgressionUtility.TryGetProgressionEntity(VWorld.Server.EntityManager,ctx.Event.SenderUserEntity, out var progEntity);
                    if (!hasProgression) return;

                    foreach (var unlockedVblood in VWorld.Server.EntityManager.GetBuffer<UnlockedVBlood>(progEntity))
                    {
                        if (new PrefabGUID(-548489519).Equals(unlockedVblood.VBlood))
                        {
                            _unlockedThisOne = true;
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
                    
                    foreach (var uRecipeElement in VWorld.Server.EntityManager.GetBuffer<UnlockedRecipeElement>(progEntity))
                    {
                        if (new PrefabGUID(-1229432962).Equals(uRecipeElement.UnlockedRecipe) && uRecipeElement.UserHasRequiredContentFlags)
                        {
                            _unlockedThisOne = true;
                        }

                        if (new PrefabGUID(-1294560299).Equals(uRecipeElement.UnlockedRecipe) && uRecipeElement.UserHasRequiredContentFlags)
                        {
                            _unlockedThisOtherOne = true;
                        }
                    }
                    if (!_unlockedThisOne)
                    {
                        var unlockedRecipeElement = new UnlockedRecipeElement
                        {
                            UnlockedRecipe = new PrefabGUID(-1229432962),
                            UserHasRequiredContentFlags = true
                        };
                        VWorld.Server.EntityManager.GetBuffer<UnlockedRecipeElement>(progEntity).Add(unlockedRecipeElement);
                        
                    }

                    if (!_unlockedThisOtherOne)
                    {
                        var unlockedRecipeElement2 = new UnlockedRecipeElement
                        {
                            UnlockedRecipe = new PrefabGUID(1294560299),
                            UserHasRequiredContentFlags = true
                        };
                        VWorld.Server.EntityManager.GetBuffer<UnlockedRecipeElement>(progEntity).Add(unlockedRecipeElement2);
                    }
                    
                    foreach (var uAbilityElement in VWorld.Server.EntityManager.GetBuffer<UnlockedAbilityElement>(progEntity))
                    {
                        if (new PrefabGUID(69268894).Equals(uAbilityElement.Source) && new PrefabGUID(-266609153).Equals(uAbilityElement.UnlockedAbility))
                        {
                            _unlockedThisOne = true;
                        }
                    }

                    if (!_unlockedThisOne)
                    {
                        var unlockedAbilityElement = new UnlockedAbilityElement
                        {
                            Source = new PrefabGUID(69268894),
                            UnlockedAbility = new PrefabGUID(-266609153)
                        };
                        VWorld.Server.EntityManager.GetBuffer<UnlockedAbilityElement>(progEntity).Add(unlockedAbilityElement);
                    }

                    foreach (var uProgressionElement in VWorld.Server.EntityManager.GetBuffer<UnlockedProgressionElement>(progEntity))
                    {
                        if (new PrefabGUID(69268894).Equals(uProgressionElement.UnlockedPrefab))
                        {
                            _unlockedThisOne = true;
                        }
                    }

                    if (!_unlockedThisOne)
                    {
                        var unlockedProgressionElement = new UnlockedProgressionElement
                        {
                            UnlockedPrefab = new PrefabGUID(69268894)
                        
                        };
                        VWorld.Server.EntityManager.GetBuffer<UnlockedProgressionElement>(progEntity).Add(unlockedProgressionElement);
                    }
                    _unlocked = true;
                    ctx.Reply("Unlocked all Ungora unlocks");
                }

                [Command("killqueen", shortHand: "kq", adminOnly: true)]
                public void SpiderQueenKill(ChatCommandContext ctx)
                {
                    var spiderQueen = new PrefabGUID(-548489519);
                    var spiders = SpiderUtil.ClosestSpiders(ctx.Event.SenderCharacterEntity, Settings.CULL_RANGE.Value);
                    var count = spiders.Count;
                    var remaining = count;

                    foreach (var spider in spiders.TakeWhile(_ => remaining != 0))
                    {
                        var isQueen = spider.ComparePrefabGuidString(spiderQueen);
                        if (isQueen)
                        {
                            _queenEntity = spider;
                            break;
                        }

                        remaining--;
                    }

                    if (_queenEntity == Entity.Null)
                    {
                        ctx.Reply("Queen not found");
                        return;
                    }

                    StatChangeUtility.KillEntity(VWorld.Server.EntityManager, _queenEntity, ctx.Event.SenderCharacterEntity, Time.time, true);
                    //DeathUtilities.Kill(VWorld.Server.EntityManager, _queenEntity, dead, deathEvent);
                    var healthcomponent = VWorld.Server.EntityManager.GetComponentData<Health>(_queenEntity);
                    _log.LogMessage(healthcomponent.IsDead ? "Queen is dead!" : "Queen isn't dead!");
                    ctx.Reply($"Killed the queen");
                }
            }

            [Command("teleport", shortHand: "tp", adminOnly: true)]
            public void TeleportToPlayer(ChatCommandContext ctx, float radius = 10f, int team = 20)
            {
                var spiders = SpiderUtil.ClosestSpiders(ctx.Event.SenderCharacterEntity, radius, team);
                var count = spiders.Count;
                float3 userPos = VWorld.Server.EntityManager
                    .GetComponentData<Translation>(ctx.Event.SenderUserEntity).Value;
                var remaining = count;
                foreach (var spider in spiders.TakeWhile(_ => remaining != 0))
                {
                    spider.WithComponentData((ref Translation t) => { t.Value = userPos; });
                    remaining--;
                }

                ctx.Reply($"Teleported {count} spiders.");
            }

            [Command("kill", shortHand: "k", adminOnly: true)]
            public void KillEnemy(ChatCommandContext ctx, float radius = 10f, int team = 20)
            {
                var spiders = SpiderUtil.ClosestSpiders(ctx.Event.SenderCharacterEntity, radius, team);
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
                    DeathUtilities.Kill(VWorld.Server.EntityManager, spider, dead, deathEvent);
                    remaining--;
                }

                ctx.Reply($"Killed {count} spiders.");
            }
        }
    }
}