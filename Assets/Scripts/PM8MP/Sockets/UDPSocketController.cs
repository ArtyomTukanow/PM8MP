using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PM8MP.Sockets
{
    public class UDPSocketController : ISocketController
    {
        private UdpClient client;

        private Action<System.Exception> onSocketException;

        public UDPSocketController(Action<System.Exception> OnSocketException = null)
        {
            onSocketException = OnSocketException;
        }
        
        public Task Connect(string hostname, int port)
        {
            if (client != null)
                Close();

            client = new UdpClient();
            client.Connect(hostname, port);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Функция открытия получения данных
        /// </summary>
        /// <param name="onReceive">Потокобезопасная функция обработки данных</param>
        public void StartReceive(Action<byte[]> onReceive)
        {
            if (client != null)
                ReceiveInner(onReceive);
        }

        private async Task ReceiveInner(Action<byte[]> onReceive)
        {
            while (
                client != null
#if UNITY_EDITOR
                && EditorApplication.isPlaying
#endif
                )
            {
                try
                {
                    UdpReceiveResult udpReceive = await client.ReceiveAsync();
                    var messages = ParseMessages(udpReceive.Buffer);
                    foreach (var msg in messages)
                    {
                        UnityEngine.Debug.Log($"[{messages.Count}] --> {string.Join(",", msg)}");
                        onReceive(msg);
                    }
                }
                catch (System.Exception exception)
                {
                    onSocketException?.Invoke(exception);
                    onSocketException = null;
                    break;
                }
            }
        }

        private List<byte[]> ParseMessages(byte[] msg)
        {
            var result = new List<byte[]>();
            
            var j = 0;
            while (j < msg.Length)
            {
                var length = msg[j] + 1;
                var nextMsg = new byte[length];
                Array.Copy(msg, j, nextMsg, 0, length);
                
                result.Add(nextMsg);
                j += length;
            }

            return result;
        }

        public void Send(byte[] msg)
        {
            if (client == null)
            {
                onSocketException?.Invoke(new System.Exception("Connection error. UDP Socket is not connected!"));
                onSocketException = null;
                return;
            }
            
            try
            {
                client.Send(msg, msg.Length);
                UnityEngine.Debug.Log($"<-- {string.Join(",", msg)}");
            }
            catch (System.Exception exception)
            {
                onSocketException?.Invoke(exception);
                onSocketException = null;
            }
        }

        public void Close()
        {
            if (client != null)
                try
                {
                    client.Close();
                }
                finally
                {
                    client.Dispose();
                    client = null;
                    onSocketException = null;
                }
        }
    }
}
