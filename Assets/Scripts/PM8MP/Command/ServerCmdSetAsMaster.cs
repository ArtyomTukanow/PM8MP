using System;

namespace PM8MP.Command
{
    public class ServerCmdSetAsMaster : MPCommand
    {
        public const byte TYPE = 254;
        public override byte Type => TYPE;
        public override MPCmdRights Rights => MPCmdRights.Server;

        protected override void Execute()
        {
            CmdSystem.SetAsMaster();
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