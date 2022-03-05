using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using PM8MP.Command;

namespace PM8MP.Rooms
{
    public interface IMPRoom
    {
        IDistributor Distributor { get; }
        MPSettings Settings { get; }
        MPPing Pinger { get; }
        bool IsConnected { get; }        
        byte? PlayerId { get; }
        IRoomView RoomView { get; }

        void Init(MPSettings settings, IRoomView roomView);

        void Connect(Action onConnected = null);

        [CanBeNull] MPCommandSystem GetPlayerCommandSystem();

        [CanBeNull] MPCommandSystem GetCommandSystem(byte playerId);

        IEnumerable<MPCommandSystem> GetAllSystems();

        void Disconnect(bool destroy = true);
        void Reconnect();



        internal void SetGamePlayer(byte playerId);

        [CanBeNull] public MPCommandSystem CreateOrGetCommandSystem(byte player);

        internal void RemoveCommandSystem(byte playerId);
    }
}
