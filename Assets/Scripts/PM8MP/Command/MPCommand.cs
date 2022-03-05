using System;
using UnityEngine;

namespace PM8MP.Command
{
    public abstract class MPCommand
    {
        public MPCommandSystem CmdSystem { get; private set; }

        public abstract byte Type { get; }
        public abstract MPCmdRights Rights { get; }
        
        protected abstract void Execute();
        protected abstract byte[] Serialize();
        protected abstract void Deserialize(byte[] code);

        internal virtual int ReservedByteLength => MPSettings.RESERVED_BYTES_LENGTH;

        internal MPCommand WithSystem(MPCommandSystem cmdSystem)
        {
            CmdSystem = cmdSystem;
            return this;
        }

        public bool CanExecute()
        {
            if (CmdSystem.Room.GetPlayerCommandSystem()?.Rights < Rights)
            {
                Debug.LogError($"Has no rights to send command {GetType().Name}, your rights: {CmdSystem.Rights}, need rights: {Rights}");
                return false;
            }

            return true;
        }

        internal virtual void Run()
        {
            Debug.Log($"Start execute: {GetType().Name} from: Player_{CmdSystem.PlayerId} ({Rights})");
            CmdSystem.TryCreateReceiver();
            Execute();
        }

        internal virtual byte[] SerializeCommand()
        {
            var cmd = Serialize();
            var result = new byte[cmd.Length + ReservedByteLength];
            Array.Copy(cmd, 0, result, ReservedByteLength, cmd.Length);
            result[0] = (byte)(result.Length - 1);
            result[1] = Type;
            result[2] = CmdSystem?.PlayerId ?? 0;
            result.ValidateByteCommand();
            return result;
        }

        internal virtual MPCommand DeserializeCommand(byte[] code)
        {
            if (code[1] != Type)
                throw new System.Exception($"[BaseTransportCommand] Type: {code[1]} doesn't equal with {GetType().Name}.Type = {Type}");
            
            byte[] shortcutCode;

            if (code.Length <= ReservedByteLength)
                shortcutCode = new byte[0];
            else
            {
                shortcutCode = new byte[code.Length - ReservedByteLength];
                Array.Copy(code, ReservedByteLength, shortcutCode, 0, shortcutCode.Length);
            }
            
            Deserialize(shortcutCode);
            
            return this;
        }
    }
}