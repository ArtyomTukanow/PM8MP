using System.Text;

namespace PM8MP.Command
{
    public class CmdSetUidFromServer : MPCommand
    {
        public const byte TYPE = 249;
        public override byte Type => TYPE;
        public override MPCmdRights Rights => MPCmdRights.Server;

        private string UID;

        public CmdSetUidFromServer()
        {
            
        }

        internal CmdSetUidFromServer(string UID)
        {
            this.UID = UID;
        }
        
        protected override void Execute()
        {
            CmdSystem.UID = UID;
            CmdSystem.Room.GetPlayerCommandSystem()?.CommandReceiver?.SendDataToNewPlayer(CmdSystem);
            CmdSystem.Room.GetPlayerCommandSystem()?.MasterReceiver?.SendDataToNewPlayer(CmdSystem);
        }

        protected override byte[] Serialize()
        {
            return Encoding.UTF8.GetBytes(UID);
        }

        protected override void Deserialize(byte[] code)
        {
            UID = Encoding.UTF8.GetString(code);
        }
    }
}