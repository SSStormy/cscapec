using System;
using System.Threading.Tasks;
using CScape.Core.Game.Entities;
using CScape.Core.Game.World;
using CScape.Core.Network;
using CScape.Models;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Factory;
using CScape.Models.Game.World;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace CScape.Core
{
    public sealed class GameServer : IGameServer
    {
        public IServiceProvider Services { get; }
        public IPlaneOfExistence Overworld { get; }
        public IEntitySystem Entities { get; }

        public bool IsDisposed { get; private set; }
        public DateTime UTCStartTime { get; private set; }

        private ILogger Log { get; }
        public IMainLoop Loop { get; }

        private IPlayerFactory Players { get; }

        public GameServer([NotNull] IServiceCollection services)
        {
            // register internal dependencies
            services.AddSingleton<IGameServer>(_ => this);
            services.AddSingleton(s => new SocketAndPlayerDatabaseDispatch(s.ThrowOrGet<IGameServer>().Services));

            // build service provider
            Services = services?.BuildServiceProvider() ?? throw new ArgumentNullException(nameof(services));

            Overworld = new PlaneOfExistence(this, "Overworld");

            Loop = Services.ThrowOrGet<IMainLoop>();
            Log = Services.ThrowOrGet<ILogger>();;
            Entities = new EntitySystem(this);
            Players = Services.ThrowOrGet<IPlayerFactory>();
        }

        public ServerStateFlags GetState()
        {
            ServerStateFlags ret = 0;

            if (Players.All.Count >= Services.GetService<IGameServerConfig>().MaxPlayers)
                ret |= ServerStateFlags.PlayersFull;

            if (!Services.GetService<SocketAndPlayerDatabaseDispatch>().IsEnabled)
                ret |= ServerStateFlags.LoginDisabled;

            return ret;
        }

        public async Task Start()
        {
            UTCStartTime = DateTime.UtcNow;

            Log.Normal(this, "Starting server...");

            await Loop.Run();
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;

                // destroy entities
                foreach(var ent in Entities.All.Keys)
                    Entities.Destroy(ent);
                    
                // block as we're saving.
                Services.GetService<IPlayerDatabase>().Save().GetAwaiter().GetResult();

                (Services as IDisposable)?.Dispose();
            }
        }
    }
}
