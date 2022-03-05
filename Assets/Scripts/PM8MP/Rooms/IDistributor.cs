using System;
using System.Collections.Generic;
using PM8MP.Sockets;
using UnityEngine;

namespace PM8MP.Rooms
{
    public interface IDistributor
    {
        IMPRoom Room { get; }

        void Connect(Action onConnected);

        internal void SendData(byte[] msg);

        void NotifySystems();

        internal void Dispose();
    }
}
