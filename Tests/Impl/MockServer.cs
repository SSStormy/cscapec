using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CScape.Core.Game.Entity;
using CScape.Core.Game.World;
using Microsoft.Extensions.DependencyInjection;

namespace CScape.Dev.Tests.Impl
{
    public class MockServer : IGameServer
    {
        public AggregateEntityPool<IWorldEntity> Entities { get; } = new AggregateEntityPool<IWorldEntity>();
        public IEntityRegistry<short, Player> Players { get; }
        public IEntityRegistry<int, Npc> Npcs { get; }

        public void Dispose() { }

        public IServiceProvider Services { get; }
        public PlaneOfExistence Overworld { get; }

        public bool IsDisposed { get; }
        public DateTime StartTime { get; }

        public MockServer() : this(new ServiceCollection()) { }

        public MockServer(IServiceCollection services)
        {
            var dirBuild = Path.GetDirectoryName(GetType().GetTypeInfo().Assembly.Location);

            services.AddSingleton<IGameServerConfig>(_ => new MockConfig());
            services.AddSingleton<IItemDefinitionDatabase>(_ => new MockItemDb());
            services.AddSingleton<IInterfaceIdDatabase>(_ => InterfaceDb.FromJson(Path.Combine(dirBuild, "interface-ids.json")));
            services.AddSingleton<IMainLoop>(_ => new MockLoop());
            services.AddSingleton<IIdPool>(_ => new IdPool());
            services.AddSingleton<ILogger>(_ => new TestLogger());
            services.AddSingleton<IGameServer>(_ => this);
            Services = services.BuildServiceProvider();

            Overworld = new PlaneOfExistence(this, "Mock overworld");

            Players = new PlayerRegistry(Services);
            Npcs = new NpcRegistry(Services);
        }

        public ServerStateFlags GetState() => 0;
        public Task Start() => null;
    }
}