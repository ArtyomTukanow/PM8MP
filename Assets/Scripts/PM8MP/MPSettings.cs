using System;
using System.Collections.Generic;

namespace PM8MP
{
    public enum ConnectionType
    {
        Local, TCP, UDP, UDPServer
    }
    
    public class MPSettings
    {
        internal const int RESERVED_BYTES_LENGTH = 3;
        
        public string Hostname { get; private set; }
        public int Port { get; private set; }
        public Dictionary<byte, Type> CommandTypes { get; private set; }
        public string RoomName { get; private set; }
        public string Uid { get; private set; }
        public double FakePingDelay { get; private set; }
        public float TimeOutInterval { get; private set; } = 5f;

        public bool IsTCP => ConnectionType == ConnectionType.TCP;
        public bool IsLocal => ConnectionType == ConnectionType.Local;
        
        public ConnectionType ConnectionType { get; private set; } = ConnectionType.UDP;


        public class SettingsBuilder
        {
            private readonly MPSettings settings = new MPSettings();

            public MPSettings Build()
            {
                if (!settings.IsLocal && (string.IsNullOrWhiteSpace(settings.Hostname) || settings.Port == default))
                    throw new System.Exception("Host or port doesn't set correctly. Set host or make settings local to connect");
                if (settings.CommandTypes == null || settings.CommandTypes.Count == 0)
                    throw new System.Exception("CommandTypes is null! Add builder.SetCommandTypes(types) to avoid exception");
                if (settings.RoomName == null)
                    throw new System.Exception("RoomName is null! Add builder.SetRoomName(roomName) to avoid exception");
                if (settings.Uid == null)
                    throw new System.Exception("Uid is null! Add builder.SetUid(uid) to avoid exception");

                return settings;
            }

            public SettingsBuilder SetHost(string host, int port)
            {
                settings.Hostname = host;
                settings.Port = port;
                return this;
            }

            public SettingsBuilder SetConnectionType(ConnectionType type)
            {
                settings.ConnectionType = type;
                return this;
            }

            public SettingsBuilder SetCommandTypes(Dictionary<byte, Type> types)
            {
                settings.CommandTypes = types;
                return this;
            }

            public SettingsBuilder SetRoomName(string val)
            {
                settings.RoomName = val;
                return this;
            }

            public SettingsBuilder SetUid(string val)
            {
                settings.Uid = val;
                return this;
            }

            public SettingsBuilder SetFakePingDelay(double val)
            {
                settings.FakePingDelay = val;
                return this;
            }

            public SettingsBuilder SetTimeOutInterval(float val)
            {
                settings.TimeOutInterval = val;
                return this;
            }
        }
    }
}
