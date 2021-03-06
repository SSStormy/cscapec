using CScape.Core.Extensions;
using CScape.Models.Data;
using CScape.Models.Game.Item;

namespace CScape.Core.Network.Packet
{
    /// <summary>
    /// Encodes an a packet which spawns a ground item.
    /// </summary>
    public class SpawnGroundItemPacket : AbstractBaseGroundItemPacket
    {
        public const int Id = 44;

        public SpawnGroundItemPacket(
            ItemStack item,
            (int x, int y) off)
            : base(item, off.x, off.y)
        {
            
        }

        protected override void InternalSend(OutBlob stream)
        {
            stream.BeginPacket(Id);

            stream.Write16((short)(Item.Id.ItemId-1));
            stream.Write16((short)Item.Amount.Clamp(0,short.MaxValue));
            stream.Write(PackedPos);

            stream.EndPacket();
        }
    }
}
 