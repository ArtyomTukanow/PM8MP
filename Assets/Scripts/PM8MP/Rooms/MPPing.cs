using System;
using System.Globalization;
using PM8MP.Command;
using UniRx;
using UnityEngine;

namespace PM8MP.Rooms
{
    public class MPPing : IDisposable
    {
        private float lastPingSendTime;
        private readonly MPRoom room;
        private IDisposable timer;
        private IDisposable timeOut;
        
        public float Ping { get; private set; }

        public MPPing(MPRoom room)
        {
            this.room = room;
            AddListeners();
            SendPing();
        }

        private void AddListeners()
        {
            RemoveListeners();
            timer = Observable.Interval(TimeSpan.FromSeconds(room.Settings.TimeOutInterval)).Subscribe(SendPing);
        }

        private void RemoveListeners()
        {
            timer?.Dispose();
            timeOut?.Dispose();
            timer = null;
        }

        private void OnTimeOut(long t)
        {
            Debug.LogError("[PING] Timeout! Waiting server answer more than " + room.Settings.TimeOutInterval + " seconds");
            timeOut?.Dispose();
            
            // room.Reconnect();
        }

        private void SendPing() => SendPing(0);
        private void SendPing(long t)
        {
            lastPingSendTime = Time.realtimeSinceStartup;
            if (room.GetPlayerCommandSystem() is {} player)
            {
                player.TryAddCommand(new SpecialCmdPing());
                timeOut?.Dispose();
                timeOut = Observable.Timer(TimeSpan.FromSeconds(room.Settings.TimeOutInterval)).Subscribe(OnTimeOut);
            }
        }

        internal void OnPongReceived()
        {
            timeOut?.Dispose();
            Ping = Time.realtimeSinceStartup - lastPingSendTime;
            Ping = (float)Math.Round(Ping, 3);
            Debug.Log("[PING] " + Ping.ToString(CultureInfo.InvariantCulture));
        }

        public void Dispose()
        {
            RemoveListeners();
        }
        
        ~MPPing()
        {
            RemoveListeners();
        }
    }
}