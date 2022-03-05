using System;
using System.Collections.Generic;
using PM8MP.Command;
using UniRx;
using UnityEngine;

namespace PM8MP.Rooms
{
    public class MPRoom : MonoBehaviour, IMPRoom
    {
        public IDistributor Distributor { get; private set; }
        public MPSettings Settings { get; private set; }
        public MPPing Pinger { get; private set; }
        public bool IsConnected { get; private set; }
        public byte? PlayerId { get; private set; }
        
        public IRoomView RoomView { get; private set; }
        
        private Dictionary<byte, MPCommandSystem> cmdSystems;
        private Action onPlayerReceived;
        private bool isInited;
        private IDisposable connectTimeoutDisposable;
        
        public void Init(MPSettings settings, IRoomView roomView)
        {
            if (isInited)
                throw new System.Exception("MPRoom Already inited. Use MPRoom.Disconnect() first!");
            
            RoomView = roomView;
            Settings = settings;
            
            cmdSystems = new Dictionary<byte, MPCommandSystem>(byte.MaxValue);
            Distributor = new MPDistributor(this);

            isInited = true;
        }
        
        private void FixedUpdate()
        {
            Distributor?.NotifySystems();
        }

        public void Connect(Action onConnected = null)
        {
            onPlayerReceived = OnPlayerReceived;
            Distributor.Connect(OnConnected);

            void OnConnected()
            {
                if (Settings.IsTCP)
                {
                    Distributor.SendData(new SpecialCmdSendRoomName(this).Serialize());
                    Distributor.SendData(new SpecialCmdSendMyUid(this).Serialize());
                }
                else
                {
                    Distributor.SendData(new SpecialCmdSendMyUid(this).Serialize());
                    Distributor.SendData(new SpecialCmdSendRoomName(this).Serialize());
                }

                connectTimeoutDisposable?.Dispose();
                connectTimeoutDisposable = Observable.Timer(TimeSpan.FromSeconds(5)).Subscribe(OnConnectionFailed);
            }

            void OnPlayerReceived()
            {
                connectTimeoutDisposable?.Dispose();
                foreach (var cmdSystem in cmdSystems.Values)
                    cmdSystem.TryCreateReceiver();
                
                Pinger = new MPPing(this);
                
                onConnected?.Invoke();
            }

            void OnConnectionFailed(long time)
            {
                Debug.LogError("[MPRoom] Connection failed! Reconnect!");
                Reconnect();
            }
        }

        internal void SetGamePlayer(byte playerId)
        {
            PlayerId = playerId;
            IsConnected = true;
            CreateOrGetCommandSystem(playerId).UID = Settings.Uid;
            onPlayerReceived?.Invoke();
        }
        
        void IMPRoom.SetGamePlayer(byte playerId) => SetGamePlayer(playerId);

        private MPCommandSystem playerCommandSystem;
        public MPCommandSystem GetPlayerCommandSystem() => playerCommandSystem ??= PlayerId != null ? GetCommandSystem(PlayerId.Value) : null;
        
        public MPCommandSystem GetCommandSystem(byte playerId) => cmdSystems.ContainsKey(playerId) ? cmdSystems[playerId] : null;
        public IEnumerable<MPCommandSystem> GetAllSystems() => cmdSystems.Values;

        internal void RemoveCommandSystem(byte playerId)
        {
            if(GetCommandSystem(playerId)?.CommandReceiver)
                GetCommandSystem(playerId)?.CommandReceiver.Disconnect();
            cmdSystems.Remove(playerId);
        }
        
        void IMPRoom.RemoveCommandSystem(byte playerId) => RemoveCommandSystem(playerId);

        internal MPCommandSystem CreateOrGetCommandSystem(byte player)
        {
            if (!cmdSystems.ContainsKey(player))
                cmdSystems.Add(player, new MPCommandSystem(player, this));
            return cmdSystems[player];
        }
        
        MPCommandSystem IMPRoom.CreateOrGetCommandSystem(byte player) => CreateOrGetCommandSystem(player);

        public void Reconnect()
        {
            Disconnect(false);
            isInited = false;
            Init(Settings, RoomView);
            Connect(() =>
            {
                Debug.Log($"[MPRoom] Reconnected! to Room_{Settings.RoomName}_{Settings.Uid}" );
            });
        }
        
        public void Disconnect(bool destroy = true)
        {
            PlayerId = null;
            playerCommandSystem = null;

            Pinger?.Dispose();
            
            connectTimeoutDisposable?.Dispose();
            IsConnected = false;

            Distributor.Dispose();
            
            foreach (var sys in cmdSystems.Values)
                if(sys.CommandReceiver)
                    sys.CommandReceiver.Disconnect();

            RoomView.OnDisconnect();

            cmdSystems.Clear();
            onPlayerReceived = null;
            
            if(destroy && this)
                Destroy(gameObject);
        }
    }
}
