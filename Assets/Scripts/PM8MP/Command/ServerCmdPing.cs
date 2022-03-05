namespace PM8MP.Command
{
    public class ServerCmdPing : MPCommand
    {
        public const byte TYPE = 248;
        public override byte Type => TYPE;
        public override MPCmdRights Rights => MPCmdRights.Server;
        
        protected override void Execute()
        {
            var pong = new SpecialCmdPong();
            CmdSystem.TryAddCommand(pong);
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