using PM8MP;
using PM8MP.Rooms;
using PM8MPSample.Map;
using System;
using Sample.Scripts.BattleRoyale.Character;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace PM8MPSample.Core
{
    public class RoomView : IRoomView
    {
        public IMPRoom Room;
        public MapContainer Map { get; private set; }

        public FoodController FoodController;

        public ObjectPool ObjectsPool;
        public bool WasFirstMaster { get; set; }

        public RoomView()
        {
            FoodController = new FoodController(this);
            ObjectsPool = new ObjectPool();
        }
        
        public void Connect(string room, string uid, ConnectionType connectionType = ConnectionType.UDP)
        {
            var settingsBuilder = new MPSettings.SettingsBuilder()
                .SetCommandTypes(SampleCommands.Commands)
                .SetConnectionType(connectionType)
                .SetRoomName(room)
                .SetUid(uid)
                .SetTimeOutInterval(5);

            switch (connectionType)
            {
                case ConnectionType.TCP:
                    settingsBuilder.SetHost("sample.tcp.server.net", 5002);
                    break;
                case ConnectionType.UDP:
                    settingsBuilder.SetHost("sample.udp.server.net", 5002);
                    break;
            }

            WasFirstMaster = false;

            Connect(settingsBuilder.Build());
        }
        
        public void Connect(MPSettings settings)
        {
            if (Room != null && (Room as MonoBehaviour))
            {
                Room.Disconnect();
                Object.Destroy(Room as MonoBehaviour);
            }

            Room = CreateRoom(settings);
        }

        public IMPRoom CreateRoom(MPSettings settings, Action<IMPRoom> onConnected = null)
        {
            GameObject go = new GameObject($"Room_{settings.RoomName}_{settings.Uid}");
            go.transform.SetParent(Game.Instance.transform);
            IMPRoom mpRoom = go.AddComponent<MPRoom>();

            ObjectsPool.Clear();

            Map = GameObject.FindObjectOfType<MapContainer>();
            Map.InitByRoomView(this);

            mpRoom.Init(settings, this);
            mpRoom.Connect(() =>
            {
                Debug.Log($"[Game] Connected! to Room_{settings.RoomName}_{settings.Uid}" );
                onConnected?.Invoke(mpRoom);
            });

            return mpRoom;
        }

        public CommandReceiver CreateCommandReceiver(byte playerId)
        {
            GameObject go = Object.Instantiate(BasePrefabs.Instance.playerPrefab);
            go.name = $"player_{playerId}";
            CommandReceiver receiver = go.GetComponent<Player>();

            return receiver;
        }

        public MasterReceiver CreateMasterReceiver(CommandReceiver commandReceiver)
        {
            return commandReceiver.gameObject.AddComponent<Master>();
        }

        public void OnDisconnect()
        {
            if (Map)
                Map.OnDisconnect();

            ObjectsPool.Clear();
            Game.Instance.HudContent.OnDisconnect();
        }
    }
}