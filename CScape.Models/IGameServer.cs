using System;
using System.Threading.Tasks;
using CScape.Models.Game.Entity;
using CScape.Models.Game.World;
using JetBrains.Annotations;

namespace CScape.Models
{
    public interface IGameServer : IDisposable
    {
        /// <summary>
        /// Returns the IOC service provider.
        /// </summary>
        [NotNull] IServiceProvider Services { get; }

        /// <summary>
        /// Returns the overworld of the server.
        /// </summary>
        [NotNull] IPlaneOfExistence Overworld { get; }

        /// <summary>
        /// Returns the main loop used by this server.
        /// </summary>
        [NotNull]
        IMainLoop Loop { get; }

        /// <summary>
        /// Returns the entity system used by this server.
        /// </summary>
        [NotNull]
        IEntitySystem Entities { get; }

        /// <summary>
        /// Returns whether this server has been disposed or not.
        /// </summary>
        bool IsDisposed { get; }
        /// <summary>
        /// Returns the time, in UTC, when the server was started.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        DateTime UTCStartTime { get; }

        /// <summary>
        /// Returns the state of the server in form of flags.
        /// </summary>
        ServerStateFlags GetState();

        /// <summary>
        /// Initializes the server and returns the infinite tick processing proceedure as a task.
        /// </summary>
        [NotNull]
        Task Start();
    }
}