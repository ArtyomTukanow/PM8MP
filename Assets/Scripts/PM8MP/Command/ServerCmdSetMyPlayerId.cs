namespace PM8MP.Command
{
    public class ServerCmdSetMyPlayerId : MPCommand
    {
        public const byte TYPE = 252;
        public override byte Type => TYPE;
        public override MPCmdRights Rights => MPCmdRights.Server;
        
        protected override void Execute()
        {
            CmdSystem.Room.SetGamePlayer(CmdSystem.PlayerId);
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