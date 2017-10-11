using System;
using System.Collections.Generic;
using CScape.Core.Network;

namespace CScape.Core.Game.Entities.Interface
{
    public interface IGameInterface : IEquatable<IGameInterface>
    {
        int Id { get; }

        IPacket GetShowPacket();
        IPacket GetClosePacket();
        IEnumerable<IPacket> GetUpdatePackets();

        void ReceiveMessage(EntityMessage msg);
    }
}