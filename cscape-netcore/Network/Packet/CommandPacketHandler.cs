using System;
using System.Linq;

namespace CScape.Network.Packet
{
    public sealed class CommandPacketHandler : IPacketHandler
    {
        public int[] Handles { get; } = { 103 };

        public void Handle(Game.Entity.Player player, int opcode, Blob packet)
        {
            if (packet.TryReadString(255, out string cmd))
            {
                var args = cmd.Split(' ').ToArray();

                switch (args[0])
                {
                    case "logout":
                        player.Logout(out _);
                        break;
                    case "forcelogout":
                        player.ForcedLogout();
                        break;
                    case "id":
                        player.SendSystemChatMessage($"UniqueEntityId: {player.UniqueEntityId}");
                        break;
                    case "gc":
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        break;
                    case "setpos":
                        var x = ushort.Parse(args[1]);
                        var y = ushort.Parse(args[2]);
                        player.Position.SetPosition(x,y);
                        break;
                    case "pos":
                        player.SendSystemChatMessage($"X: {player.Position.X} Y: {player.Position.Y} Z: {player.Position.Z}");
                        player.SendSystemChatMessage($"LX: {player.Position.LocalX} LY: {player.Position.LocalY}");
                        player.SendSystemChatMessage($"RX: {player.Position.RegionX} + 6 RY: {player.Position.RegionY} + 6");
                        break;
                    default:
                        player.Log.Debug(this, $"Command: {cmd}");
                        break;

                }
            }
                
            else
                player.Log.Warning(this, "Couldn't commmand.");
        }
    }
}