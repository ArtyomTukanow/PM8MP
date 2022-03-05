namespace PM8MP.Command
{
    public class ServerCmdPong : MPCommand
    {
        public const byte TYPE = 247;
        public override byte Type => TYPE;
        public override MPCmdRights Rights => MPCmdRights.Server;
        
        protected override void Execute()
        {
            CmdSystem.Room.Pinger?.OnPongReceived();
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