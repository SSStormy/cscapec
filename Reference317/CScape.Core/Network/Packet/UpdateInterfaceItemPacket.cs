using System.Collections.Generic;
using System.Linq;
using CScape.Models.Data;
using CScape.Models.Game.Interface;
using CScape.Models.Game.Item;

namespace CScape.Core.Network.Packet
{
    public sealed class UpdateInterfaceItemPacket : IPacket
    {
        private readonly IGameInterface _interf;
        private readonly IItemContainer _container;
        private readonly HashSet<int> _indicies;

        public const int Id = 34;

        public UpdateInterfaceItemPacket(
            IGameInterface interf,
            IItemContainer container, 
            ICollection<int> indicies)
        {
            _interf = interf;
            _container = container;
            _indicies = new HashSet<int>(indicies);
        }

        public void Send(OutBlob stream)
        {
            // don't write the packet if no indicies have been passed.
            if (!_indicies.Any())
                return;

            stream.BeginPacket(Id);

            stream.Write16((short)_interf.Id);

            foreach (var i in _indicies)
            {
                // write index
                if (i < 128)
                    stream.Write((byte)i);
                else
                    stream.Write16((short)i);

                var item = _container.Provider[i];
                // write id
                stream.Write16((short)item.Id.ItemId);

                // write size as byte-int32 smart
                stream.WriteByteInt32Smart(item.Amount);
            }

            stream.EndPacket();
        }
    }
}