using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM8MP.Sockets
{
    public interface ISocketController
    {
        Task Connect(string hostname, int port);

        /// <summary>
        /// Функция открытия получения данных
        /// </summary>
        /// <param name="onReceive">Потокобезопасная функция обработки данных</param>
        void StartReceive(Action<byte[]> onReceive);

        void Send(byte[] msg);

        void Close();
    }
}
