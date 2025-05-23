using System.Linq;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using UnityEngine;
using VAMP;
using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using Unity.Entities;
using Unity.Transforms;
using VampireCommandFramework;

namespace SpiderKiller.VCFCompat
{
    
    public static partial class Commands
    {
        private static ManualLogSource _log => Plugin.LogInstance;

        static Commands()
        {
            Enabled = IL2CPPChainloader.Instance.Plugins.TryGetValue("gg.deca.VampireCommandFramework", out var info);
            if (Enabled) _log.LogInfo($"VCF Version: {info.Metadata.Version}");
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
                var em = Core.EntityManager;

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
                
                [Command("downqueen", shortHand: "dqueen", adminOnly: false, description: "Downs Ungora",
                    usage: "Usage: .dqueen")]
                public void downQueen(ChatCommandContext ctx)
                {
                    _queenEntity = SpiderUtil.GetQueen(ctx);
                    var downedQueen = SpiderUtil.DownQueen(_queenEntity);
                    ctx.Reply(downedQueen ? "The queen can't see you and is one hit!" : "Failed to down the queen, are you in range?");
                }
                
                [Command("unlockQueen", shortHand: "uqueen", adminOnly: false, description: "unlocked Ungora progression",
                    usage: "Usage: .uqueen")]
                public void unlockQueen(ChatCommandContext ctx)
                {
                    DebugEventsSystem debugEventsSystem = Core.Server.GetExistingSystemManaged<DebugEventsSystem>();
                    UnlockVBlood debugEvent = new UnlockVBlood();
                    debugEvent.VBlood = new PrefabGUID(-548489519);
                    UnlockProgressionDebugEvent evt = new UnlockProgressionDebugEvent();
                    evt.PrefabGuid = new PrefabGUID(574648849);
                    debugEventsSystem.UnlockProgression(
                        new FromCharacter { Character = ctx.Event.SenderCharacterEntity, User = ctx.Event.SenderUserEntity }, evt);
                    evt.PrefabGuid = new PrefabGUID(693361325);
                    debugEventsSystem.UnlockProgression(
                        new FromCharacter { Character = ctx.Event.SenderCharacterEntity, User = ctx.Event.SenderUserEntity }, evt);
                    debugEventsSystem.UnlockVBloodEvent(debugEventsSystem, debugEvent,
                        new FromCharacter { Character = ctx.Event.SenderCharacterEntity, User = ctx.Event.SenderUserEntity });
                    ctx.Reply("Succesfully unlocked Ungora progression unlocks, no need to get close to her now.");
                }
                
                [Command("tpToMe", shortHand: "ttm", adminOnly: true,
                    description: "Teleports all spiders(Entities with FactionIndex 25) to you in a defined range",
                    usage: "Usage: .ttm [range]")]
                public void TeleportToPlayer(ChatCommandContext ctx, float range = 10f, int factionIndex = 25)
                {
                    if (range > 50f) range = 50f;
                    var spiders = SpiderUtil.ClosestSpiders(ctx.Event.SenderCharacterEntity, range, factionIndex);
                    var count = spiders.Count;
                    var userPos = Core.Server.EntityManager
                        .GetComponentData<Translation>(ctx.Event.SenderUserEntity).Value;
                    var remaining = count;
                    foreach (var spider in spiders.TakeWhile(_ => remaining != 0))
                    {
                        spider.WithComponentDataC((ref Translation t) => { t.Value = userPos; });
                        remaining--;
                    }

                    ctx.Reply($"Teleported {count} spiders.");
                }

                [Command("killSpiders", shortHand: "kspi", adminOnly: false,
                    description: "Kills all spiders(Entities with FactionIndex 25) in a defined range",
                    usage: "Usage: .kspi [range]")]
                public void KillEnemy(ChatCommandContext ctx, float range = 10f, int factionIndex = 25)
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
                        DeathUtilities.Kill(Core.Server.EntityManager, spider, dead, deathEvent, deathreason);
                        remaining--;
                    }

                    ctx.Reply($"Killed {count} spiders.");
                }
            }
        }
    }
}