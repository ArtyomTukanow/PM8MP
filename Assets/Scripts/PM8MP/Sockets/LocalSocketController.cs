using PM8MP.Rooms;
using PM8MP.Server;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PM8MP.Sockets
{
    public class LocalSocketController : ISocketController
    {
        private Task receiveTask;
        private bool isClosed = true;

        private Action<System.Exception> onSocketException;

        public readonly IMPRoom Room;

        public LocalSocketController(IMPRoom room, Action<System.Exception> OnSocketException = null)
        {
            Room = room;
            onSocketException = OnSocketException;
        }
        
        public async Task Connect(string hostname, int port)
        {
            isClosed = false;
            await Task.Delay(TimeSpan.FromSeconds(.01f));
        }

        /// <summary>
        /// Функция открытия получения данных
        /// </summary>
        /// <param name="onReceive">Потокобезопасная функция обработки данных</param>
        public void StartReceive(Action<byte[]> onReceive)
        {
            receiveTask = ReceiveInner(onReceive);
        }

        private async Task ReceiveInner(Action<byte[]> onReceive)
        {
            while (!isClosed
#if UNITY_EDITOR
                && EditorApplication.isPlaying
#endif
                )
            {
                lock (serverDataPool)
                {
                    foreach (var msg in serverDataPool)
                    {
                        Debug.Log($"--> {string.Join(",", msg)}");
                        onReceive(msg);
                    }
                    serverDataPool.Clear();
                }

                if (Room.Settings.FakePingDelay > 0)
                    await Task.Delay(TimeSpan.FromSeconds(Room.Settings.FakePingDelay));
                else
                    await Task.Yield();
            }
        }

        
        private readonly List<byte[]> serverDataPool = new List<byte[]>();
        
        internal void ReceiveFromLocalServer(byte[] msg)
        {
            lock (serverDataPool)
            {
                serverDataPool.Add(msg);
            }
        }

        public void Send(byte msg)
        {
            Send(new byte[] { msg });
        }

        public void Send(byte[] msg)
        {
            LocalServer.Instance.ReceiveData(this, msg);
            Debug.Log($"<-- {string.Join(",", msg)}");
        }

        public void Close()
        {
            isClosed = true;
        }
    }
}
