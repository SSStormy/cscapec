using System;
using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Message;
using CScape.Models.Game.Command;
using CScape.Models.Game.Entity;
using Microsoft.Extensions.DependencyInjection;

namespace CScape.Core.Network.Handler
{
    public sealed class CommandPacketHandler : IPacketHandler
    {
        private readonly IServiceProvider _services;
        public byte[] Handles { get; } = { 103 };
        public void Handle(IEntity entity, PacketMessage packet)
        {
            if (packet.Data.TryReadString(out string cmd))
            {
                var allFailed = true;

                foreach (var c in _services.GetServices<ICommandHandler>())
                {
                    if (c.Push(entity, cmd))
                        allFailed = false;
                }

                if (allFailed)
                    entity.SystemMessage($"Unknown command: \"{cmd}\"", CoreSystemMessageFlags.Normal);
            }
        }

        public CommandPacketHandler(IServiceProvider services)
        {
            _services = services;
        }
    }
}