using PM8MP.Command;
using PM8MPSample.Core;
using System;
using UnityEngine;

namespace PM8MPSample.Command
{
    public class CmdMove : MPCommand
    {
        public const byte TYPE = 1;
        public override byte Type => TYPE;
        public override MPCmdRights Rights => MPCmdRights.Player;

        public Vector3 Position { get; private set; }

        public CmdMove()
        {
        }

        public CmdMove(Vector3 position)
        {
            Position = position;
        }

        protected override void Execute()
        {
            if (CmdSystem.CommandReceiver is Player player)
                player.PlayerMove.Move(Position);
        }

        protected override byte[] Serialize()
        {
            byte[] result = new byte[12];
            BitConverter.GetBytes(Position.x).CopyTo(result, 0);
            BitConverter.GetBytes(Position.y).CopyTo(result, 4);
            BitConverter.GetBytes(Position.z).CopyTo(result, 8);

            return result;
        }

        protected override void Deserialize(byte[] code)
        {
            float x = BitConverter.ToSingle(code, 0);
            float y = BitConverter.ToSingle(code, 4);
            float z = BitConverter.ToSingle(code, 8);

            Position = new Vector3(x, y, z);
        }
    }
}