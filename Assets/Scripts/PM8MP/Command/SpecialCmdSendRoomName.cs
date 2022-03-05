using System;
using System.Text;
using PM8MP.Rooms;

namespace PM8MP.Command
{
    public class SpecialCmdSendRoomName
    {
        public const byte TYPE = 251;
        
        private readonly IMPRoom room;

        public SpecialCmdSendRoomName(IMPRoom room)
        {
            this.room = room;
        }

        public byte[] Serialize()
        {
            if (room.Settings.IsTCP)
            {
                string name = room.Settings.RoomName;
                var nameBytes = Encoding.UTF8.GetBytes(name);
                var result = new byte[nameBytes.Length + 1];
                Array.Copy(nameBytes, 0, result, 1, nameBytes.Length);
                result[0] = (byte)nameBytes.Length;
                return result;
            }
            else
            {
                string name = room.Settings.RoomName;
                var nameBytes = Encoding.UTF8.GetBytes(name);
                var result = new byte[nameBytes.Length + 2];
                Array.Copy(nameBytes, 0, result, 2, nameBytes.Length);
                result[0] = (byte) (result.Length - 1);
                result[1] = TYPE;
                return result;
            }
        }
    }
}