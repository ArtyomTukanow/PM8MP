using PM8MP.Command;
using PM8MPSample.Core;
using Sample.Scripts.BattleRoyale.Character;
using UnityEngine;

namespace PM8MPSample.Command
{
    public class CmdEatFood : MPExtrapolationCommand
    {
        public const byte TYPE = 4;

        public override byte Type => TYPE;
        public override MPCmdRights Rights => MPCmdRights.Master;

        private Vector2Int foodPos;
        private bool isRad;

        private GameObject food;
        
        /// <summary>
        /// Конструктор, который запускается, если команда пришла с ретранслятора. 
        /// </summary>
        public CmdEatFood()
        {
            
        }

        /// <summary>
        /// Конструктор, который запускается на локальной машине (Когда команда отправляется в ретранслятор) 
        /// </summary>
        public CmdEatFood(Vector2Int foodPos, bool isRad)
        {
            this.foodPos = foodPos;
            this.isRad = isRad;
        }
        
        /// <summary>
        /// Непосредственное выполнение команды. Запускается на всех клиентах.
        /// </summary>
        protected override void Execute()
        {
            Game.Instance.RoomView.FoodController.RemoveFood(foodPos);
            
            if (CmdSystem.CommandReceiver is Player player)
            {
                if (isRad)
                    player.Character.AddAbility(CharacterAbility.AddShootRange);
                else
                    player.Character.ChangeHP(10);
            }
        }

        /// <summary>
        /// Экстраполируем команду.
        /// Со стороны клиента, еда должна съесться моментально. Результат можно также моментально применить, но с возможностью его отката.
        /// Ждем от мастера точно такую же команду (должна побайтово совпадать), чтобы применить экстраполяцию. Иначе, через N секунд команда откатится <see cref="RevertExtrapolate()"/> 
        /// </summary>
        protected override void ExecuteExtrapolate()
        {
            var pool = Game.Instance.RoomView.FoodController.FoodPool;
            if (pool.ContainsKey(foodPos))
                food = pool[foodPos];

            pool = Game.Instance.RoomView.FoodController.RadFoodPool;
            if (pool.ContainsKey(foodPos))
                food = pool[foodPos];
            
            if(food)
                food.SetActive(false);
        }

        /// <summary>
        /// Отменяем экстраполяцию команды. В данном случае, при экстраполяции мы ничего не начисляли.
        /// Если бы в <see cref="ExecuteExtrapolate()"/> было начисление HP, то тут пришлось бы его отменить.
        /// </summary>
        protected override void RevertExtrapolate()
        {
            if(food)
                food.SetActive(true);
        }

        /// <summary>
        /// Сериализация в байтовое представление команды
        /// </summary>
        /// <returns></returns>
        protected override byte[] Serialize()
        {
            return new[] {(byte) foodPos.x, (byte) foodPos.y, isRad ? (byte)1 : (byte)0};
        }

        /// <summary>
        /// Десериализация байтов непосредственно в данную команду
        /// </summary>
        /// <param name="code"></param>
        protected override void Deserialize(byte[] code)
        {
            foodPos = new Vector2Int(code[0], code[1]);
            isRad = code[2] == 1;
        }
    }
}