using System;
using System.Collections.Generic;
using System.Linq;
using PM8MP.Command;
using PM8MP.Rooms;
using PM8MPSample.Command;
using UniRx;
using UnityEngine;

namespace PM8MPSample.Core
{
    public class Master : MasterReceiver
    {
        private float SEND_FOOD_TIME = 10f;
        
        private FoodController foodController;
        private IDisposable sendFoodSub;

        public override void OnInited()
        {
            //foodController ??= Game.Instance.RoomView.FoodController;

            //sendFoodSub?.Dispose();
            //sendFoodSub = Observable.Interval(TimeSpan.FromSeconds(SEND_FOOD_TIME))
            //    .Subscribe(_ => UpdateFood());
            //UpdateFood();

            Game.Instance.RoomView.Map.DeathZone.InitAsMaster();
            Game.Instance.RoomView.Map.SpawnOnStart(commandSystem);
        }

        public void Update()
        {
            //CheckEatFood();
            //CheckEatRadFood();
        }

        private void CheckEatFood()
        {
            List<(MPCommandSystem system, Vector2Int pos)> needRemoveFood = null;
            foreach (var system in commandSystem.Room.GetAllSystems())
            {
                var pos = foodController.CheckEatFood(system);
                if (pos != null)
                {
                    needRemoveFood ??= new List<(MPCommandSystem, Vector2Int)>();
                    needRemoveFood.Add((system, pos.Value));
                }
            }

            if (needRemoveFood != null)
                foreach (var preCommand in needRemoveFood)
                    preCommand.system.TryAddCommand(new CmdEatFood(preCommand.pos, false));
        }

        private void CheckEatRadFood()
        {
            List<(MPCommandSystem system, Vector2Int pos)> needRemoveFood = null;
            foreach (var system in commandSystem.Room.GetAllSystems())
            {
                var pos = foodController.CheckEatRadFood(system);
                if (pos != null)
                {
                    needRemoveFood ??= new List<(MPCommandSystem, Vector2Int)>();
                    needRemoveFood.Add((system, pos.Value));
                }
            }

            if (needRemoveFood != null)
                foreach (var preCommand in needRemoveFood)
                    preCommand.system.TryAddCommand(new CmdEatFood(preCommand.pos, true));
        }

        internal void UpdateFood(bool needGenerate = true)
        {
            UpdateAllFoods(needGenerate);
            UpdateAllRadFoods(needGenerate);
        }

        private void UpdateAllFoods(bool needGenerate = true)
        {
            var cmd = new CmdSendFoods(needGenerate ? foodController.GetOrGenerateFoodPoses() : foodController.FoodPool.Keys.ToArray());
            commandSystem.TryAddCommand(cmd);
        }

        private void UpdateAllRadFoods(bool needGenerate = true)
        {
            var cmd = new CmdSendRadFoods(needGenerate ? foodController.GetOrGenerateRadFoodPoses() : foodController.RadFoodPool.Keys.ToArray());
            commandSystem.TryAddCommand(cmd);
        }

        public void OnDestroy()
        {
            if (commandSystem.Room.RoomView is RoomView view)
                view.Map.DeathZone.OnMasterDemoted();

            sendFoodSub?.Dispose();
            sendFoodSub = null;
        }

        public void SendCharacterData(Player player)
        {
            var cmd = new CmdCharacterSnapshot();
            player.commandSystem.TryAddCommand(cmd);
        }

        public override void SendDataToNewPlayer(MPCommandSystem system)
        {
            Game.Instance.RoomView.ObjectsPool.SendSpawnedInstancesData(commandSystem);
            Game.Instance.RoomView.Map.DeathZone.SendZoneStatus();

            foreach (var mpCommandSystem in commandSystem.Room.GetAllSystems())
                if (mpCommandSystem != system && mpCommandSystem.CommandReceiver is Player player)
                    SendCharacterData(player);
        }
    }
}