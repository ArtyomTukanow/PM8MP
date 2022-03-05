namespace PM8MP.Command
{
    public class SpecialCmdDisconnectMe
    {
        public const byte TYPE = 255;

        public byte[] Serialize()
        {
            return new byte[] {1, 255};
        }
    }
}