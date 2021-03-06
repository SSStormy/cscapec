﻿using CScape.Models.Game.Entity;

namespace CScape.Core.Game.Entity.Message
{
    public sealed class ButtonClickMessage : IGameMessage
    {
        public int ButtonId { get; }
        public int InterfaceId { get; }

        public ButtonClickMessage(int buttonId, int interfaceId)
        {
            ButtonId = buttonId;
            InterfaceId = interfaceId;
        }

        public int EventId => (int)MessageId.ButtonClicked;
    }
}