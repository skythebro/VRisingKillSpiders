using System.Linq;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
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
            const float Radius = 25f;

            public override Spider Parse(ChatCommandContext ctx, string input)
            {
                var spiders = SpiderUtil.ClosestSpiders(ctx.Event.SenderCharacterEntity, Radius);
                var em = VWorld.Server.EntityManager;
                ComponentDataFromEntity<Team> getTeam = em.GetComponentDataFromEntity<Team>();

                foreach (var spider in spiders)
                {
                    var team = getTeam[spider];
                    var isUnit = Team.IsInUnitTeam(team);
                    var isNeutral = Team.IsInNeutralTeam(team);
                    if (isNeutral || !isUnit) continue;
                    return new Spider(spider);
                }

                throw ctx.Error($"Could not find a spider within {Radius:F1} units named like \"{input}\"");
            }

            [CommandGroup("spider", "sp")]
            public class SpiderCommands
            {
                [Command("teleport", shortHand: "tp", adminOnly: true)]
                public void TeleportToPlayer(ChatCommandContext ctx, float radius = 5f, int team = 20)
                {

                    var spiders = SpiderUtil.ClosestSpiders(ctx.Event.SenderCharacterEntity, radius, team);
                    var count = spiders.Count;
                    float3 userPos = VWorld.Server.EntityManager
                        .GetComponentData<Translation>(ctx.Event.SenderUserEntity).Value;
                    var remaining = count;
                    foreach (var spider in spiders.TakeWhile(spider => remaining != 0))
                    {
                        spider.WithComponentData((ref Translation t) => { t.Value = userPos; });
                        remaining--;
                    }

                    ctx.Reply($"Teleported {count} spiders.");
                }

                [Command("kill", shortHand: "k", adminOnly: true)]
                public void KillEnemy(ChatCommandContext ctx, float radius = 5f, int team = 20)
                {
                    var spiders = SpiderUtil.ClosestSpiders(ctx.Event.SenderCharacterEntity, radius, team);
                    var count = spiders.Count;
                    var remaining = count;
                    foreach (var spider in spiders.TakeWhile(spider => remaining != 0))
                    {
                        spider.WithComponentData((ref Health t) =>
                        {
                            t.Value = -10000;
                            t.TimeOfDeath = Time.time;
                            t.IsDead = true;
                        });
                        //StatChangeUtility.KillEntity(VWorld.Server.EntityManager,  spider, ctx.Event.SenderCharacterEntity, 0);
                        remaining--;
                    }

                    ctx.Reply($"Killed {count} spiders.");
                }
            }
        }
    }
}