﻿using System.Collections.Generic;
using CScape.Core.Game.Entities.Message;
using CScape.Models.Data;
using JetBrains.Annotations;

namespace CScape.Core.Network
{
    public interface IPacketParser
    {
        IEnumerable<PacketMessage> Parse([NotNull] CircularBlob stream);
    }
}