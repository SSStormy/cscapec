using System;
using CScape.Models.Game.Entity;
using CScape.Models.Game.World;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Message
{
    public sealed class PoeSwitchMessage : IGameMessage
    {
        [CanBeNull]
        public IPlaneOfExistence OldPoe { get; }

        [NotNull]
        public IPlaneOfExistence NewPoe { get; }

        public PoeSwitchMessage([CanBeNull] IPlaneOfExistence oldPoe, [NotNull] IPlaneOfExistence newPoe)
        {
            OldPoe = oldPoe;
            NewPoe = newPoe ?? throw new ArgumentNullException(nameof(newPoe));
        }

        public int EventId => (int)MessageId.PoeSwitch;
    }
}