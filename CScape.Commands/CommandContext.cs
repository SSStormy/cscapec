using System;
using CScape.Models.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Commands
{
    public sealed class CommandContext
    {
        [NotNull] public IEntity Callee { get; }
        [NotNull] public Command CommandModel { get; }
        [CanBeNull] public string Data { get; }
        [NotNull] public string UnparsedData { get; }

        public CommandContext(
            [NotNull] IEntity callee, 
            [NotNull] Command commandModel, 
            [CanBeNull] string data, 
            [NotNull] string unparsedData)
        {
            Callee = callee ?? throw new ArgumentNullException(nameof(callee));
            CommandModel = commandModel ?? throw new ArgumentNullException(nameof(commandModel));
            Data = data;
            UnparsedData = unparsedData;
        }
    }
}