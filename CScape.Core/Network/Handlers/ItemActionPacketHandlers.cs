﻿using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CScape.Core.Data;
using CScape.Core.Game.Entity;
using CScape.Core.Game.Interface;
using CScape.Core.Game.Item;
using CScape.Core.Injection;
using Microsoft.Extensions.DependencyInjection;

namespace CScape.Core.Network.Handlers
{
    public sealed class ItemActionPacketHandlers : IPacketHandler
    {
        private readonly Dictionary<int, ItemActionType> _opToActionMap = new Dictionary<int, ItemActionType>()
        {
            {41, ItemActionType.Generic1},
            {122, ItemActionType.Generic2},
            {16, ItemActionType.Generic3},
            {87, ItemActionType.Drop},
        };

        public int[] Handles { get; } = {122, 41, 16, 87};

        private IItemDefinitionDatabase _db;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IItemDefinitionDatabase GetDb(Player player)
            => _db ?? (_db = player.Server.Services.GetService<IItemDefinitionDatabase>());
        
        public void Handle(Player player, int opcode, Blob packet)
        {
            // read
            var interf = packet.ReadInt16();
            var idx = packet.ReadInt16();
            var itemId = packet.ReadInt16() + 1;

            player.DebugMsg($"Action: interf: {interf} idx: {idx} id: {itemId}", ref player.DebugCommands);

            // find interf
            var container = player.Interfaces.TryGetById(interf) as IContainerInterface;
            if (container == null)
            {
                player.Log.Warning(this, $"Item action opcode {opcode} was passed unregistered iid: {interf}");
                return;
            }

            // verify idx
            if (0 > idx || idx >= container.Items.Size)
            {
                player.Log.Warning(this, $"Out of range idx in item action (op {opcode}): {idx} max size: {container.Items.Size}");
                return;
            }

            // verify itemId == item pointed at by idx
            var serverSideId = container.Items.Provider.GetId(idx);
            if (itemId != serverSideId)
            {
                player.Log.Warning(this, $"Item action item id did not match the one in the given iid {interf} at given idx {idx} (client {itemId} != server {serverSideId})");
                return;
            }

            // get definition
            var def = GetDb(player).GetAsserted(serverSideId);

            if (def == null)
            {
                player.Log.Warning(this, $"No definition found for item id {serverSideId}");
                return;
            }

            // check if we have defined the action given by the current opcode
            if (!_opToActionMap.ContainsKey(opcode))
            {
                player.Log.Warning(this, $"Undefined item action for action opcode: {opcode}");
                return;
            }

            // opcode is verified, we got all the data, time to execute it.
            player.Interfaces.OnActionOccurred();

            // determine action type by opcode
            var action = _opToActionMap[opcode];
            
            // do drop if action is a drop
            if (action == ItemActionType.Drop)
            {
                // todo : handle dropping upon receiving an itemactiontype drop 
            }

            // execute action
            def.OnAction(player, container.Items, idx, action);
        }
    }
}
 