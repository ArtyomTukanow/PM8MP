using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace PM8MP.Command
{
    public abstract class MPExtrapolationCommand : MPCommand
    {
        private IDisposable timer;
        internal override int ReservedByteLength => base.ReservedByteLength + 1;

        protected abstract void ExecuteExtrapolate();
        protected abstract void RevertExtrapolate();

        public bool IsValid { get; private set; } = true;

        public MPExtrapolationCommand()
        {
            
        }

        public MPExtrapolationCommand(bool isValid)
        {
            IsValid = isValid;
        }

        internal void Extrapolate(Action onTimeOut)
        {
            Debug.Log($"Start extrapolate: {GetType().Name} from: Player_{CmdSystem.PlayerId} ({Rights})");
            ExecuteExtrapolate();
            timer = Observable.Interval(TimeSpan.FromSeconds(4)).Subscribe(OnTimeOut);
            
            void OnTimeOut(long time)
            {
                EndTimer();
                onTimeOut?.Invoke();
                Debug.Log($"CANCEL EXTRAPOLATE (TIME OUT): {GetType().Name} from: Player_{CmdSystem.PlayerId} ({Rights})");
                RevertExtrapolate();
            }
        }

        internal void ApproveExtrapolate(bool isValid)
        {
            IsValid = isValid;
            EndTimer();
            Run();
        }
        
        internal override void Run()
        {
            if (IsValid)
                base.Run();
            else
            {
                Debug.Log($"CANCEL EXTRAPOLATE FROM MASTER: {GetType().Name} from: Player_{CmdSystem.PlayerId} ({Rights})");
                RevertExtrapolate();
            }
        }

        private void EndTimer()
        {
            timer?.Dispose();
            timer = null;
        }

        internal override byte[] SerializeCommand()
        {
            var result = base.SerializeCommand();
            result[ReservedByteLength - 1] = IsValidExtrapolation() ? (byte)1 : (byte)0;
            return result;
        }

        internal override MPCommand DeserializeCommand(byte[] code)
        {
            IsValid = code.Length >= ReservedByteLength && code[ReservedByteLength - 1] == 1;
            return base.DeserializeCommand(code);
        }

        private bool IsValidExtrapolation()
        {
            return CmdSystem.IsCurrent || IsValid;
        }

        public bool IsEqual(object obj)
        {
            return obj is MPExtrapolationCommand cmd
                   && cmd.Type == Type &&
                   cmd.Serialize().SequenceEqual(Serialize());
        }
    }
}