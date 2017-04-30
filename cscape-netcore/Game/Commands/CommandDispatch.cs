using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CScape.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Game.Commands
{
    public sealed class CommandDispatch
    {
        private readonly Dictionary<string, Command> _cmds = new Dictionary<string, Command>(StringComparer.OrdinalIgnoreCase);

        public void RegisterAssembly(Assembly asm)
        {
            foreach (var cls in asm.GetTypes())
            {
                var clsInfo = cls.GetTypeInfo();
                if (clsInfo.GetCustomAttribute<CommandsClassAttribute>() == null)
                    continue;

                var clsPredicates = clsInfo.GetCustomAttributes<PredicateAttribute>().ToList();

                foreach (var method in cls.GetRuntimeMethods())
                {
                    var cmdAttrib = method.GetCustomAttribute<CommandMethodAttribute>();
                    if (cmdAttrib == null)
                        continue;

                    var id = cmdAttrib.Identifier ?? method.Name.ToLowerInvariant();
                    var instance = Activator.CreateInstance(cls);

                    // verify signature of method
                    var args = method.GetParameters();
                    Action noArgExec = null;
                    Action<CommandContext> exec = null;

                    if (args.Length == 1)
                    {
                        if (args[0].ParameterType != typeof(CommandContext))
                            throw new InvalidOperationException(
                                "Command method can either have no args or must take a CommandContext.");

                        exec = (Action<CommandContext>)method.CreateDelegate(typeof(Action<CommandContext>), instance);
                    }
                    else if (args.Length != 0)
                        throw new InvalidOperationException(
                            "Command method can either have no args or must take a CommandContext.");
                    else
                        noArgExec = (Action)method.CreateDelegate(typeof(Action), instance);

                    RegisterCommand(new Command(id, noArgExec, exec, clsPredicates.Concat(method.GetCustomAttributes<PredicateAttribute>())));
                }
            }
        }

        public void RegisterCommand([NotNull] Command command)
        {
            if (_cmds.ContainsKey(command.Identifier))
                throw new InvalidOperationException($"Duplicate command with identifier {command.Identifier}");

            _cmds.Add(command.Identifier, command);
        }

        [CanBeNull]
        public Command GetCommand(string id)
        {
            if (!_cmds.ContainsKey(id)) return null;
            return _cmds[id];
        }

        /// <summary>
        /// Tries to find and dispatch a command matching the given input and callee.
        /// </summary>
        /// <returns>True if command to dispatch was found, false if no command was found.</returns>
        public bool Dispatch([NotNull] Player callee, [NotNull] string input)
        {
            if (callee == null) throw new ArgumentNullException(nameof(callee));
            if (input == null) throw new ArgumentNullException(nameof(input));

            try
            {
                // find command in input
                var identifier = "";
                foreach (var word in input.Split(' ').Select(s => s.Trim()))
                {
                    if (string.IsNullOrEmpty(word))
                        continue;

                    identifier += word;

                    var cmd = GetCommand(identifier);
                    if (cmd == null)
                    {
                        identifier += " ";
                        continue;
                    }
                    // cmd found

                    // check if predicates say its ok to proceed.
                    if (cmd.Predicates.Any(pred => !pred.CanExecute(callee, cmd)))
                        break;

                    // parse data if needed
                    string data = null;
                    if (cmd.NoArgExecTarg == null)
                        data = input.Substring(input.IndexOf(word, StringComparison.Ordinal)).TrimStart();

                    try
                    {
                        // dispatch cmd
                        if (cmd.NoArgExecTarg != null)
                            cmd.NoArgExecTarg();
                        else if (cmd.ExecTarg != null)
                            cmd.ExecTarg(new CommandContext(callee, cmd, data, input));
                        else
                            throw new NotSupportedException("Cmd has no exec target.");
                    }
                    catch (Exception)
                    {
                        if (callee.DebugCommands)
                            throw;

                        callee.SendSystemChatMessage($"Command error. Make sure inputs are valid.");
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                if (callee.DebugCommands)
                    callee.SendSystemChatMessage($"cmd fail: {ex.Message} ({ex.GetType().Name})");
                else
                    callee.SendSystemChatMessage($"Command parse error.");

                callee.SendSystemChatMessage($"Dispatch fail: callee: {callee} data: {input} ex: {ex}");
                return true;
            }

            return false;
        }
    }
}