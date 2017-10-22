using System.Collections.Generic;
using CScape.Core.Game.Entities.Component;
using CScape.Core.Network.Entity.Flag;
using CScape.Core.Network.Entity.Segment;
using CScape.Core.Network.Packet;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;

namespace CScape.Core.Network.Entity.Component
{
    /// <summary>
    /// Responsible for syncing every visible player entity to the network.
    /// </summary>
    [RequiresComponent(typeof(NetworkingComponent))]
    [RequiresComponent(typeof(PlayerComponent))]
    public sealed class PlayerNetworkSyncComponent: EntityNetworkSyncComponent
    {
        public override int Priority { get; } = ComponentConstants.PriorityPlayerUpdate;

        public PlayerNetworkSyncComponent(
            Game.Entities.Entity parent)
            :base(parent)
        {
            
        }

        protected override bool IsHandleableEntity(IEntityHandle h)
        {
            if (h.IsDead())
                return false;

            return h.Get().GetPlayer() != null;
        }

        protected override void SetInitialFlags(IUpdateWriter writer, IEntity ent)
        {
            writer.SetFlag(new PlayerAppearanceUpdateFlag());
            writer.SetFlag(new FacingCoordinateUpdateFlag(ent.GetTransform().FacingData));
            writer.SetFlag(new InteractingEntityUpdateFlag(ent.GetTransform().InteractingEntity));
        }

        protected override void Sync()
        {
            var updates = new List<IUpdateWriter>();

            /* Local */
            IUpdateSegment local;
            {
                var flags = Parent.Components.AssertGet<FlagAccumulatorComponent>();
                var updater = new LocalPlayerUpdateWriter(flags);
                var needsUpdate = updater.NeedsUpdate();

                if (flags.Reinitialize)
                {
                    local = new LocalPlayerInitSegment(Parent.AssertGetPlayer(), needsUpdate);
                }
                else
                    local = CommonSegmentResolve(flags, needsUpdate);

                if (needsUpdate)
                    updates.Add(updater);
            }


            var sync = GetSyncSegments(updates, f => new PlayerUpdateWriter(f));
            var init = GetInitSegments(updates, f => new PlayerUpdateWriter(f));

            /* Send packet */
            Parent.Components.AssertGet<NetworkingComponent>().SendPacket(
                new PlayerUpdatePacket(
                    local,
                    sync,
                    init,
                    updates));
        }

    }
}