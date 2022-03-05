using System;
using System.Collections.Generic;
using System.Linq;
using PM8MP.Command;
using PM8MP.Sockets;
using UnityEngine;

namespace PM8MP.Rooms
{
    public class MPDistributor : IDistributor
    {
        public IMPRoom Room => room;
        private readonly IMPRoom room;

        private Queue<byte[]> commands;
        private ISocketController socket;

        internal MPDistributor(IMPRoom room)
        {
            commands = new Queue<byte[]>(100);
            this.room = room;

            CreateSocket();
        }

        private void CreateSocket()
        {
            socket?.Close();
            switch (room.Settings.ConnectionType)
            {
                case ConnectionType.UDP:
                    socket = new UDPSocketController(OnSocketException);
                    break;
                case ConnectionType.TCP:
                    socket = new TCPSocketController(OnSocketException);
                    break;
                case ConnectionType.Local:
                    socket = new LocalSocketController(room, OnSocketException);
                    break;
                default:
                    throw new System.Exception($"Please add connection type {room.Settings.ConnectionType} to distributor!");
            }
        }

        public async void Connect(Action onConnected = null)
        {
            await socket.Connect(room.Settings.Hostname, room.Settings.Port);
            socket.StartReceive(ReadBytes);
            // Debug.Log("[Socket] Connected: " + Room.Settings.Hostname + ":" + Room.Settings.Port);
            onConnected?.Invoke();
        }

        private List<byte[]> commandsToSend = new List<byte[]>();

        public void SendData(byte[] msg)
        {
            commandsToSend.Add(msg);
            // Debug.Log("[Socket] Send: " + msg.ToStringBytes());
            // 
        }
        
        void IDistributor.SendData(byte[] msg) => SendData(msg);

        private void OnSocketException(System.Exception ex)
        {
            Debug.Log(ex.Message);
            
            Room.Reconnect();
        }

        private void ReadBytes(byte[] msg)
        {
            lock (commands)
            {
                commands.Enqueue(msg);
            }
        }

        private bool TryParceAndRunSpecialCommand(byte[] msg)
        {
            if (msg.Length < 1)
                return false;

            switch (msg.GetCommandType())
            {
                case SpecialCmdDisconnectMe.TYPE:
                    SendData(new SpecialCmdDisconnectMe().Serialize());
                    Room.Reconnect();
                    return true;
            }

            return false;
        }

        internal void NotifySystems()
        {
            lock (commands)
            {
                while (commands.Count > 0)
                {
                    byte[] nextBytes = commands.Dequeue();
                    if (TryParceAndRunSpecialCommand(nextBytes))
                    {
                        continue;
                    }
                    else if (nextBytes.IsByteCommand())
                    {
                        room.CreateOrGetCommandSystem(nextBytes.GetCommandPlayer())?.AddCommandFromServer(nextBytes);
                    }
                    else
                    {
                        Debug.LogError("[Socket] UNKNOWN command: " + nextBytes.ToStringBytes());
                    }
                    // Debug.Log("[Socket] Resived command: " + nextBytes.ToStringBytes());
                }
            }

            SendCommands();
        }
        void IDistributor.NotifySystems() => NotifySystems();

        private void SendCommands()
        {
            if(commandsToSend.Count == 0)
                return;

            var length = commandsToSend.Sum(m => m.Length);
            var msg = new byte[length];
            
            var i = 0;
            foreach (var command in commandsToSend)
                for (var j = 0; j < command.Length; j++)
                    msg[i++] = command[j];
            socket.Send(msg);

            // foreach (var cmd in commandsToSend)
                // socket.Send(cmd);
            
            commandsToSend.Clear();
        }
        

        private bool disposeOnce;
        protected virtual void DisposeOnce()
        {
            if (!disposeOnce)
            {
                commands?.Clear();
                socket.Close();
                disposeOnce = true;
            }
        }

        ~MPDistributor()
        {
            DisposeOnce();
        }

        internal void Dispose()
        {
            DisposeOnce();
            GC.SuppressFinalize(this);
        }
        void IDistributor.Dispose() => Dispose();
    }
}
