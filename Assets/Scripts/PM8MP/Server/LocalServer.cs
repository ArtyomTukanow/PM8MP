using System.Collections.Generic;
using System.Linq;
using PM8MP.Command;
using PM8MP.Sockets;

namespace PM8MP.Server
{
    public class LocalServer
    {
        private readonly List<LocalSocketController> sockets = new List<LocalSocketController>();

        public static LocalServer Instance { get; } = new LocalServer();

        private void TryAddConnect(LocalSocketController socket)
        {
            if(!sockets.Contains(socket))
                sockets.Add(socket);
        }

        public bool RunIfSpecialCommand(LocalSocketController socket, byte[] msg)
        {
            if (msg == null || msg.Length < 2)
                return false;
            
            switch (msg.GetCommandType())
            {
                case SpecialCmdSendRoomName.TYPE:
                    //todo пока считаю, что комната одна
                    return true;
                case SpecialCmdSendMyUid.TYPE:
                    var newPlayerId = GetNextPlayerId();
                    SendData(GeneratePlayerIdCommand(socket, newPlayerId), includeSocket: socket);
                    SendData(GeneratePlayerUidCommand(socket, newPlayerId), exceptSocket: socket);
                    TrySetAsMaster(socket, newPlayerId);
                    return true;
                case SpecialCmdPing.TYPE:
                    SendData(GeneratePongCommand(socket), includeSocket: socket);
                    return true;
            }
            
            return false;
        }

        public void ReceiveData(LocalSocketController socket, byte[] msg)
        {
            TryAddConnect(socket);

            if (RunIfSpecialCommand(socket, msg))
            {
                
            }
            else if (msg.IsByteCommand())
            {
                SendData(msg, exceptSocket: socket);
            }
        }

        private void TrySetAsMaster(LocalSocketController socket, byte playerId)
        {
            if(sockets.Any(s => s.Room.GetPlayerCommandSystem()?.IsMaster == true))
                return;
            
            SendData(GenerateSetAsMasterCommand(socket, playerId));
        }

        private void SendData(byte[] msg, LocalSocketController exceptSocket = null, LocalSocketController includeSocket = null)
        {
            var exceptSockets = exceptSocket != null ? new List<LocalSocketController> {exceptSocket} : null;
            var includeSockets = includeSocket != null ? new List<LocalSocketController> {includeSocket} : null;
            SendData(msg, exceptSockets, includeSockets);
        }

        private void SendData(byte[] msg, List<LocalSocketController> exceptSockets, List<LocalSocketController> includeSockets)
        {
            foreach (var socket in sockets)
            {
                if(exceptSockets != null && exceptSockets.Contains(socket))
                    continue;
                if(includeSockets != null && !includeSockets.Contains(socket))
                    continue;

                socket.ReceiveFromLocalServer(msg);
            }
        }


        private byte GetNextPlayerId()
        {
            return (byte)((sockets.Max(s => s.Room.PlayerId) ?? 0) + 1);
        }
        

        private byte[] GeneratePlayerIdCommand(LocalSocketController socket, byte playerId)
        {
            var commandSystem = socket.Room.CreateOrGetCommandSystem(playerId);
            
            return new ServerCmdSetMyPlayerId()
                .WithSystem(commandSystem)
                .SerializeCommand();
        }

        private byte[] GeneratePlayerUidCommand(LocalSocketController socket, byte playerId)
        {
            var commandSystem = socket.Room.CreateOrGetCommandSystem(playerId);
            
            return new CmdSetUidFromServer(socket.Room.Settings.Uid)
                .WithSystem(commandSystem)
                .SerializeCommand();
        }

        private byte[] GenerateSetAsMasterCommand(LocalSocketController socket, byte playerId)
        {
            var commandSystem = socket.Room.CreateOrGetCommandSystem(playerId);
            
            return new ServerCmdSetAsMaster()
                .WithSystem(commandSystem)
                .SerializeCommand();
        }

        private byte[] GeneratePongCommand(LocalSocketController socket)
        {
            var commandSystem = socket.Room.GetPlayerCommandSystem();
            
            return new ServerCmdPong()
                .WithSystem(commandSystem)
                .SerializeCommand();
        }
    }
}