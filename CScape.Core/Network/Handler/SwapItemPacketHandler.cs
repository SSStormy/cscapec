﻿using CScape.Core.Data;
using CScape.Core.Game.Entities.Component;
using CScape.Core.Game.Entity;
using CScape.Core.Game.Interfaces;
using CScape.Core.Injection;

namespace CScape.Core.Network.Handler
{
    public sealed class SwapItemPacketHandler : IPacketHandler
    {
        public byte[] Handles { get; } = { 214 };

        public void Handle(Game.Entities.Entity entity, PacketMetadata packet)
        {
            var interfaceIdx = packet.Data.ReadInt16();
            var magic = packet.Data.ReadByte();
            var fromIdx = packet.Data.ReadInt16();
            var toIdx = packet.Data.ReadInt16();

            // swapping item A with item A is a no-op, skip.
            if (fromIdx == toIdx) return;

            entity.SystemMessage($"Swap {fromIdx} -> {toIdx} (magic: {magic} )");
            
            // get inventory
            var interfaces = entity.Components.Get<InterfaceComponent>();
            if (interfaces == null)
                return;

            if (!interfaces.All.TryGetValue(interfaceIdx, out var interfMetadata))
                return;

            var interf = interfMetadata.Interface as IItemGameInterface;

            var inventory = interf?.Container as ISwappableItemContainer;

            if (inventory == null)
                return;

            // check if idx are in range
            bool IsNotInRange(int val) =>  0 > val || val >= inventory.Provider.Count;

            if (IsNotInRange(fromIdx)) return;
            if (IsNotInRange(toIdx)) return;


            inventory.Swap(fromIdx, toIdx);
        }
    }
}
