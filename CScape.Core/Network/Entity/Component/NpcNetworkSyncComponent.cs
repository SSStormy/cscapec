﻿using System.Collections.Generic;
using CScape.Core.Game.Entities;
using CScape.Core.Network.Entity.Flag;
using CScape.Core.Network.Packet;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Component
{
    public sealed class NpcNetworkSyncComponent : EntityNetworkSyncComponent
    {
        public override int Priority { get; }

        public NpcNetworkSyncComponent([NotNull] Game.Entities.Entity parent) : base(parent)
        {
        }

        protected override bool IsHandleableEntity(EntityHandle h)
        {
            if (h.IsDead())
                return false;

            return h.Get().Components.Get<NpcComponent>() != null;
        }

        protected override void SetInitialFlags(IUpdateWriter writer, Game.Entities.Entity ent)
        {
            writer.SetFlag(new InteractingEntityUpdateFlag(ent.GetTransform().InteractingEntity));
            writer.SetFlag(new FacingCoordinateUpdateFlag(ent.GetTransform().FacingData));
        }

        protected override void Sync()
        {
            var updates = new List<IUpdateWriter>();

            var sync = GetSyncSegments(updates, f => new NpcUpdateWriter(f));
            var init = GetInitSegments(updates, f => new NpcUpdateWriter(f));

            Network.SendPacket(
                new NpcUpdatePacket(
                    sync,
                    init,
                    updates));
        }
    }
}