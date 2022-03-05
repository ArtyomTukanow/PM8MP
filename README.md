# PM8MP
PM8MP - UDP клиент-серверный движок цель которого использование своих технологий транспорта со своим сервером.

Preview video: https://youtu.be/6ZwQszaxRWg 

Цели:
* Разработать систему клиент-серверного соединения для мультиплеерной игры.
* Поддержка соединения до 30 пользователей и 40 ботов.
* Продумать валидацию команд (защита от взлома).
* Поддержка стабильного соединения без потери пакетов для определенных запросов.
* Плавность действий на всех устройствах.
* Получить в конечном итоге библиотеку клиент-серверного соединения, на которой будет построен тестовый прототип.

Задачи:
* Использовать UDP для повышения скорости передачи данных.
* Разработать отправку пакетов, отправку пакетов с ответом (позволит передать пакет без потери).
* Добиться плавности действий с помощью интерполяции и экстраполяции.
* Выделить Мастера для валидации команд.
* Назначить нового Мастера, если старый мастер отвалился.
* Задать командам права для выполнения (Клиент может выполнять команды клиента, мастер - команды клиента и мастера).

Ограничения:
* Клиент может управлять только своими данными (данными своего игрока). Мастер может управлять данными любого игрока. Клиент в приоритете применяет команды мастера на себя. 
* Пример 1: Игрок1 стреляет в Игрока2. Игрок1 посылает команду выстрела. Мастер валидирует и засчитывает убийство. Мастер посылает всем игрокам изменения Игрока2 (убит). Игрок1 максимум может показать фейковое убийство Игрока2, чтобы избежать задержек.
* Пример 2: Игрок1 взял дроп. Игрок2 взял тот же самый дроп. Дроп - не клиентские данные и такую дилемму может решить только Мастер. Игрок1 посылает команду “Хочу взять дроп”. Мастер посылает команду “Взял дроп”.
* Отправлять команды не чаще, чем раз в N мс.
* 

| Роль | Задача |
| ---- | ------ |
| Клиент | Подключение/отключение к хосту |
| Клиент | Отправление пакета команд с правами клиента в ретранслятор  |
| Клиент | Получение любых команд с ретранслятора  |
| Клиент | Повторная отправка команд с определенными сиквенсами при запросе от ретранслятора (потеря пакетов)  |
| Клиент | Запускать экстраполяцию при ожидании ответов мастера (делаем вид, что выполняем, когда отправили запрос мастеру. Ждем от мастера, что действительно выполнили команду)  |
| Клиент | Получает снапшот, корректирует данные по снапшоту. Корректирует поведение других клиентов посредствам интерполяции.  |
| Клиент | После снапшота при рассинхроне локальных данных клиента и данных клиента по снапшоту формирует команды, чтобы убрать рассинхрон.  |
| Клиент | Посылает пакет в ретранслятор в периодичность раз в N секунд (можно пустые)  |


| Роль | Задача |
| ---- | ------ |
| Мастер | Отправляет команды с правами мастера в ретранслятор |
| Мастер | Отправляет общее состояние игры (снапшот). |
| Мастер | Отправляет снапшот сразу после назначения в мастеры (если это клиент) |
| Мастер | Валидирует команды. При невалидной команде отправляет команду с правами мастера (бан, тп в точку) в ретранслятор. |

| Роль | Задача |
| ---- | ------ |
| Ретранслятор | Назначает мастера |
| Ретранслятор | Выбирает получателей для отправки команд |
| Ретранслятор | Повторно отправляет команды с определенными сиквенсами при запросе от клиента (потеря пакетов) |
| Ретранслятор | Отсылает мастеру команду на отключение пользователей с нестабильным интернет-соединением |

| Роль | Задача |
| ---- | ------ |
| Сервер | Создает комнату |
| Сервер | Закрывает комнату, если клиентов не осталось |


Временная диаграмма. Пример работы выстрела в игрока.
![alt text](https://github.com/ArtyomTukanow/PM8MP/blob/master/ShooterScreenshot-59-05-03-22.png)


```C#
    /// <summary>
    /// Конкретная реализация комнаты. Заведует коннектом и созданием игроков на сцене.
    /// </summary>
    public class RoomView : IRoomView
    {
        public IMPRoom Room;

        public void Connect(string room, string uid)
        {
            var settingsBuilder = new MPSettings.SettingsBuilder()
                .SetCommandTypes(SampleCommands.Commands)
                .SetConnectionType(ConnectionType.UDP)
                .SetRoomName(room)
                .SetUid(uid)
                .SetTimeOutInterval(5)
                .SetHost("develop.sample.net", 5002);

            Room = CreateRoom(settingsBuilder.Build());
        }

        public IMPRoom CreateRoom(MPSettings settings, Action<IMPRoom> onConnected = null)
        {
            GameObject go = new GameObject($"Room_{settings.RoomName}_{settings.Uid}");
            go.transform.SetParent(Game.Instance.transform);
            IMPRoom mpRoom = go.AddComponent<MPRoom>();

            mpRoom.Init(settings, this);
            mpRoom.Connect(() =>
            {
                Debug.Log($"[Game] Connected! to Room_{settings.RoomName}_{settings.Uid}" );
                onConnected?.Invoke(mpRoom);
            });

            return mpRoom;
        }

        public CommandReceiver CreateCommandReceiver(byte playerId)
        {
            GameObject go = Object.Instantiate(BasePrefabs.Instance.playerPrefab);
            go.name = $"player_{playerId}";
            CommandReceiver receiver = go.GetComponent<Player>();

            return receiver;
        }

        public MasterReceiver CreateMasterReceiver(CommandReceiver commandReceiver)
        {
            return commandReceiver.gameObject.AddComponent<Master>();
        }

        public void OnDisconnect()
        {
            if (Map)
                Map.OnDisconnect();

            ObjectsPool.Clear();
            Game.Instance.HudContent.OnDisconnect();
        }
    }
```


```C#
    /// <summary>
    /// Команда на движение персонажа
    /// </summary>
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

```


```C#

    /// <summary>
    /// Поеданием "Еды" и восстановление HP (С экстраполяцией).
    /// </summary>
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
```
