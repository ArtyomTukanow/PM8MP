using System;

namespace PM8MP.Command
{
    public class ServerCmdPlayerLeft : MPCommand
    {
        public const byte TYPE = 253;
        public override byte Type => TYPE;
        public override MPCmdRights Rights => MPCmdRights.Server;
        
        protected override void Execute()
        {
            CmdSystem.Room.RemoveCommandSystem(CmdSystem.PlayerId);
        }

        protected override byte[] Serialize()
        {
            return new byte[0];
        }

        protected override void Deserialize(byte[] code)
        {
            
        }
    }
}