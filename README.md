# PM8MP
PM8MP - UDP клиент-серверный движок цель которого использование своих технологий транспорта со своим сервером.

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