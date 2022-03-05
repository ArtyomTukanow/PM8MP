using System;
using System.Net.Sockets;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PM8MP.Sockets
{
    public class TCPSocketController : ISocketController
    {
        private TcpClient client;

        private Action<System.Exception> onSocketException;

        private const byte CR = 13; // Carriage Return (CR)
        private const byte WILL = 0xFB; // 251 - WILL (option code)
        private const byte WONT = 0xFC; // 252 - WON'T (option code)
        private const byte DO = 0xFD; // 253 - DO (option code)
        private const byte DONT = 0xFE; // 254 - DON'T (option code)
        private const byte IAC = 0xFF; // 255 - Interpret as Command (IAC)

        private bool isConnected;
        private bool IsConnected
        {
            get
            {
                //https://stackoverflow.com/questions/6993295/how-to-determine-if-the-tcp-is-connected-or-not
                try
                {
                    //if (GameTime.Now <= lastCheckedConnectionStatusTime + checkConnectionInterval) return isConnected;
                    //lastCheckedConnectionStatusTime = GameTime.Now;

                    if (client != null && client.Client != null && client.Client.Connected)
                    {
                        /* pear to the documentation on Poll:
						 * When passing SelectMode.SelectRead as a parameter to the Poll method it will return 
						 * -either- true if Socket.Listen(Int32) has been called and a connection is pending;
						 * -or- true if data is available for reading; 
						 * -or- true if the connection has been closed, reset, or terminated; 
						 * otherwise, returns false
						 */

                        // Detect if client disconnected
                        if (client.Client.Poll(0, SelectMode.SelectRead))
                        {
                            var buff = new byte[1];
                            isConnected = client.Client.Receive(buff, SocketFlags.Peek) != 0;
                        }
                        else
                            isConnected = true;
                    }
                    else
                        isConnected = false;
                }
                catch
                {
                    isConnected = false;
                }

                return isConnected;
            }
        }

        private NetworkStream networkStream;
        private NetworkStream NetworkStream
        {
            get
            {
                if (networkStream == null && client != null && IsConnected)
                    networkStream = client.GetStream();

                return networkStream;
            }
        }

        public TCPSocketController(Action<System.Exception> OnSocketException = null)
        {
            onSocketException = OnSocketException;
        }

        public async Task Connect(string hostname, int port)
        {
            if (client != null)
                Close();

            client = new TcpClient();
            await client.ConnectAsync(hostname, port);
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
            while (client != null && NetworkStream != null
#if UNITY_EDITOR
                && EditorApplication.isPlaying
#endif
                )
            {
                try
                {
                    if (client.Available > 0)
                        ReadData(onReceive);
                }
                catch (System.Exception exception)
                {
                    onSocketException?.Invoke(exception);
                }

                await Task.Yield();
            }
        }

        private byte dataLength = 0;
        private byte currentLength = 0;
        private byte[] receivedByteArray = new byte[byte.MaxValue];

        private void ReadData(Action<byte[]> onReceive)
        {
            if (NetworkStream == null)
                return;
            
            lock (NetworkStream)
            {
                while (NetworkStream.DataAvailable)
                {
                    byte b = (byte) NetworkStream.ReadByte();
                    currentLength++;
                
                    if (dataLength == 0)
                    {
                        dataLength = b;
                        currentLength = 0;
                    }
                    receivedByteArray[currentLength] = b;
                
                    if (dataLength > 0 && currentLength >= dataLength)
                    {
                        var result = new byte[dataLength + 1];
                        Array.Copy(receivedByteArray, result, result.Length);
                        dataLength = 0;
                        onReceive(result);
                    }
                }
            }
        }

        public void Send(byte[] msg)
        {
            if (client == null || NetworkStream == null)
                throw new System.Exception("Connection error. TCP Socket is not connected!");
            
            try
            {
                if (NetworkStream.CanWrite)
                {
                    NetworkStream.Write(msg, 0, msg.Length);
                    UnityEngine.Debug.Log($"Sended: {string.Join(",", msg)}");
                }
                else
                    onSocketException?.Invoke(new SocketException(-1));
            }
            catch (System.Exception exception)
            {
                onSocketException?.Invoke(exception);
            }
        }

        public void Close()
        {
            networkStream?.Close();
            networkStream = null;

            if (client != null)
                try
                {
                    client.Close();
                }
                finally
                {
                    client.Dispose();
                    client = null;
                }
        }
    }
}
