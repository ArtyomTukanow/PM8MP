namespace PM8MP.Command
{
    public class SpecialCmdPong : MPCommand
    {
        public const byte TYPE = 247;
        public override byte Type => TYPE;
        public override MPCmdRights Rights => MPCmdRights.Player;
        
        protected override void Execute()
        {
            
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