using JetBrains.Annotations;
using PM8MP.Rooms;
using PM8MPSample.Command;

namespace PM8MPSample.Core
{
    public class Player : CommandReceiver
    {
        [CanBeNull]
        public Master Master => commandSystem.MasterReceiver as Master;

        private void CreateMove()
        {
            PlayerMove?.Dispose();
            
            if(IsCurrent)
                PlayerMove = new CurrentPlayerMove(this);
            else if (IsBot)
                PlayerMove = new BotMove(this);
            else
                PlayerMove = new NoneCurrentPlayerMove(this);
        }

        public override void OnInited()
        {
            CreateMove();
            PlayerMove.OnInited();
            
            if(IsCurrent)
                Respawn();
            
            if (IsCurrent)
            {
                controls = new SampleControls();

                if (gameObject.activeInHierarchy)
                    controls.Enable();

                Game.Instance.CameraControls.Player = this;
            }

            if (IsCurrent)
            {
                var cmd = new CmdCharacterSnapshot();
                commandSystem.TryAddCommand(cmd);
            }
        }

        private void Update()
        {
            PlayerMove.Update();
            
            if (IsCurrent && !IsMaster)
                UpdateAsNotAMaster();
        }

        private void FixedUpdate()
        {
            PlayerMove.FixedUpdate();
        }

        public void Respawn()
        {
            if (IsCurrent || IsMasterBot)
            {
                transform.localPosition = (commandSystem.Room.RoomView as RoomView).Map.PlayerSpawns.GetSpawnPoint();

                PlayerMove.Move(transform.localPosition);
                PlayerMove.OnRespawn();
            }
        }

        private void UpdateAsNotAMaster()
        {
            ExtrapolateEatFood();
        }

        private void ExtrapolateEatFood()
        {
            var pos = Game.Instance.RoomView.FoodController.CheckEatFood(commandSystem);
            if (pos != null)
            {
                var cmd = new CmdEatFood(pos.Value, false);
                commandSystem.ExtrapolateCommand(cmd);
            }
            
            pos = Game.Instance.RoomView.FoodController.CheckEatRadFood(commandSystem);
            if (pos != null)
            {
                var cmd = new CmdEatFood(pos.Value, true);
                commandSystem.ExtrapolateCommand(cmd);
            }
        }
    }
}