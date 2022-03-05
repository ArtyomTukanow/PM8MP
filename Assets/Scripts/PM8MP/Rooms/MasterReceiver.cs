using PM8MP.Command;
using UnityEngine;

namespace PM8MP.Rooms
{
    public abstract class MasterReceiver : MonoBehaviour
    {
        protected MPCommandSystem commandSystem;

        public void Init(MPCommandSystem commandSystem)
        {
            this.commandSystem = commandSystem;
            OnInited();
        }

        public abstract void OnInited();

        public abstract void SendDataToNewPlayer(MPCommandSystem system);
    }
}