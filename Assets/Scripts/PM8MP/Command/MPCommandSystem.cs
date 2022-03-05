using PM8MP.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using PM8MP.Exception;
using Object = UnityEngine.Object;

namespace PM8MP.Command
{
    public class MPCommandSystem
    {
        public bool IsCurrent => Room.PlayerId == PlayerId;

        public readonly IMPRoom Room;
        
        public CommandReceiver CommandReceiver { get; private set; }
        public MasterReceiver MasterReceiver { get; private set; }
        
        public string UID { get; internal set; }
        
        public bool IsMaster { get; private set; }
        public byte PlayerId { get; }

        public MPCmdRights Rights => IsMaster ? MPCmdRights.Master : MPCmdRights.Player;

        protected Dictionary<byte, Type> Types => Room.Settings.CommandTypes;
        public bool IsBot => PlayerId >= 100;

        internal MPCommandSystem(byte playerId, IMPRoom room)
        {
            Room = room;
            PlayerId = playerId;
        }

        public void TryCreateReceiver()
        {
            if (!CommandReceiver && Room.IsConnected)
            {
                CommandReceiver = Room.RoomView.CreateCommandReceiver(PlayerId);
                CommandReceiver.Init(this);
            }
        }

        internal void SetAsMaster()
        {
            if (IsMaster)
                return;

            foreach (var sys in Room.GetAllSystems())
                if (sys != this)
                {
                    sys.IsMaster = false;
                    
                    if(sys.MasterReceiver)
                        Object.Destroy(sys.MasterReceiver);
                }

            IsMaster = true;

            if (IsCurrent)
            {
                TryCreateReceiver();
                MasterReceiver = Room.RoomView.CreateMasterReceiver(CommandReceiver);
                MasterReceiver.Init(this);
            }

            Room.RoomView.WasFirstMaster = true;
        }

        public void ExtrapolateCommand(MPExtrapolationCommand cmd)
        {
            if(IsMaster)
                throw new System.Exception("Master can't extrapolate commands");
            
            if(extrapolationCommands.Contains(cmd))
                throw new System.Exception($"Command {cmd.GetType().Name} already extrapolated!");
            
            cmd.WithSystem(this);
            extrapolationCommands.Add(cmd);
            cmd.Extrapolate(() => extrapolationCommands.Remove(cmd));
        }

        public void TryAddCommand(MPCommand cmd, bool needExecute = true)
        {
            cmd.WithSystem(this);
            if (IsCurrent || Room.GetPlayerCommandSystem()?.IsMaster == true)
            {
                if (cmd.CanExecute())
                {
                    if(needExecute)
                        cmd.Run();
                    Room.Distributor.SendData(cmd.SerializeCommand());
                }
            }
        }

        internal void AddCommandFromServer(byte[] code)
        {
            AddCommandFromServer(CommandFactory(code));
        }

        internal void AddCommandFromServer([NotNull] MPCommand cmd)
        {
            if (IsCurrent && cmd is MPExtrapolationCommand extrCmd && TryCompleteExtrapolationCommand(extrCmd))
                return;
            
            cmd.Run();
        }

        private bool TryCompleteExtrapolationCommand(MPExtrapolationCommand cmd)
        {
            if (extrapolationCommands.FirstOrDefault(c => c.IsEqual(cmd)) is {} foundedCmd)
            {
                extrapolationCommands.Remove(foundedCmd);
                foundedCmd.ApproveExtrapolate(cmd.IsValid);
                return true;
            }

            return false;
        }
        
        internal MPCommand CommandFactory(byte[] code)
        {
            code.ValidateByteCommand();
            
            var type = code.GetCommandType();
            if (!Types.ContainsKey(type))
                throw new ByteCodeException(code, "[BaseCommandSystem] Unknown type: " + type);
            
            var cmd = Activator.CreateInstance(Types[type]) as MPCommand;
            return cmd.DeserializeCommand(code).WithSystem(this);
        }
        
        private List<MPExtrapolationCommand> extrapolationCommands = new List<MPExtrapolationCommand>();
    }
}