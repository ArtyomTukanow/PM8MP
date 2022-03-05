using PM8MP.Command;
using UnityEngine;

namespace PM8MP.Rooms
{
    public abstract class CommandReceiver : MonoBehaviour
    {
        public MPCommandSystem commandSystem;
        
        public MasterReceiver masterReceiver => commandSystem.MasterReceiver;

        public bool IsMaster => commandSystem.IsMaster;
        public bool IsCurrent => commandSystem.IsCurrent;
        public bool IsBot => commandSystem.IsBot;
        public bool IsMasterBot => commandSystem.Room.GetPlayerCommandSystem()?.IsMaster == true && IsBot;
        public byte PlayerId => commandSystem.PlayerId;

        public void Init(MPCommandSystem commandSystem)
        {
            this.commandSystem = commandSystem;
            OnInited();
        }

        public abstract void OnInited();

        public abstract void SendDataToNewPlayer(MPCommandSystem system);

        public void Disconnect()
        {
            Destroy(gameObject);
        }
    }
}
