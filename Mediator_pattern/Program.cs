/*
Чтобы обеспечить безопасность при обработке сообщений между компонентами, в посреднике можно добавить:

1. Проверку отправителя

В OrderMediator.Notify проверяется, кто отправил сообщение:

только Client может слать "NewOrder",

только Manager — "OrderApproved",

только Warehouse — "OrderPrepared".

2. Проверку типа и целостности данных

Перед обработкой проверяем, что data действительно OrderRequest и содержит корректные значения

3. Проверку бизнес-правил

Менеджер проверяет, что количество неотрицательное, не слишком большое и т.п., иначе заказ не подтверждается.

4. Обработку неизвестных событий

Для неизвестных событий посредник ничего не делает и выводит сообщение, чтобы не выполнять непредусмотренные действия.

Также можно добавить аудит, логирование и авторизацию
 */

namespace Mediator_pattern
{
    // простая модель заказа, которую будут передавать компоненты через посредника
    class OrderRequest
    {
        public string ProductName { get; }
        public int Quantity { get; }

        public OrderRequest(string productName, int quantity)
        {
            ProductName = productName;
            Quantity = quantity;
        }
    }

    // абстрактный посредник
    abstract class Mediator
    {
        // общий метод для взаимодействия между компонентами
        public abstract void Notify(object sender, string eventCode, object data);
    }

    // базовый класс для всех участников, работающих через посредника
    abstract class Participant
    {
        protected Mediator mediator;

        protected Participant(Mediator mediator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public void ChangeMediator(Mediator newMediator)
        {
            mediator = newMediator ?? throw new ArgumentNullException(nameof(newMediator));
        }
    }

    // клиент
    class Client : Participant
    {
        public string Name { get; }

        public Client(Mediator mediator, string name) : base(mediator)
        {
            Name = name;
        }

        // отправка сообщения через посредника (оформление заказа)
        public void PlaceOrder(string productName, int quantity)
        {
            if (string.IsNullOrWhiteSpace(productName))
            {
                Console.WriteLine("Клиент: название товара не может быть пустым.");
                return;
            }

            if (quantity <= 0)
            {
                Console.WriteLine("Клиент: количество должно быть больше нуля.");
                return;
            }

            var order = new OrderRequest(productName.Trim(), quantity);
            Console.WriteLine($"Клиент {Name}: оформляет заказ на товар '{order.ProductName}', {order.Quantity} шт.");
            mediator.Notify(this, "NewOrder", order);
        }

        // обработка сообщения от других компонентов (через посредника)
        public void NotifyOrderReady(OrderRequest order)
        {
            Console.WriteLine(
                $"Клиент {Name}: получил уведомление, что заказ '{order.ProductName}' ({order.Quantity} шт.) готов к выдаче.");
        }
    }

    // менеджер
    class Manager : Participant
    {
        public string Name { get; }

        public Manager(Mediator mediator, string name) : base(mediator)
        {
            Name = name;
        }

        // обработка сообщения от клиента (через посредника)
        public void ProcessNewOrder(OrderRequest order)
        {
            Console.WriteLine(
                $"Менеджер {Name}: получил новый заказ на товар '{order.ProductName}' ({order.Quantity} шт.).");

            if (order.Quantity <= 0)
            {
                Console.WriteLine("Менеджер: некорректное количество, заказ отклонён.");
                return;
            }

            if (order.Quantity > 1000)
            {
                Console.WriteLine("Менеджер: слишком большой объём заказа, требуется дополнительное согласование.");
                return;
            }

            Console.WriteLine("Менеджер: подтверждает заказ и отправляет его на склад.");
            mediator.Notify(this, "OrderApproved", order);
        }
    }

    // склад
    class Warehouse : Participant
    {
        public string Name { get; }

        public Warehouse(Mediator mediator, string name) : base(mediator)
        {
            Name = name;
        }

        // обработка сообщения от менеджера (через посредника)
        public void ReserveOrder(OrderRequest order)
        {
            Console.WriteLine(
                $"Склад {Name}: резервирует товар '{order.ProductName}' ({order.Quantity} шт.) на складе.");

            // чисто заглушка
            Console.WriteLine("Склад: заказ подготовлен, сообщаем посреднику.");
            mediator.Notify(this, "OrderPrepared", order);
        }
    }

    // конкретный посредник
    class OrderMediator : Mediator
    {
        public Client Client { get; set; }
        public Manager Manager { get; set; }
        public Warehouse Warehouse { get; set; }

        // управление взаимодействием между Client, Manager и Warehouse
        public override void Notify(object sender, string eventCode, object data)
        {
            if (sender == null) throw new ArgumentNullException(nameof(sender));
            if (eventCode == null) throw new ArgumentNullException(nameof(eventCode));

            // проверяем тип данных
            if (data is not OrderRequest order)
            {
                Console.WriteLine("Посредник: получены некорректные данные заказа, действие отменено.");
                return;
            }

            switch (eventCode)
            {
                case "NewOrder":
                    // проверка, кто имеет право создавать заказ
                    if (!ReferenceEquals(sender, Client))
                    {
                        Console.WriteLine("Посредник: только клиент может создавать заказ. Попытка отклонена.");
                        return;
                    }

                    Console.WriteLine("Посредник: передаём новый заказ менеджеру.");
                    Manager?.ProcessNewOrder(order);
                    break;

                case "OrderApproved":
                    // проверка, кто имеет право утверждать заказ
                    if (!ReferenceEquals(sender, Manager))
                    {
                        Console.WriteLine("Посредник: только менеджер может утверждать заказ. Попытка отклонена.");
                        return;
                    }

                    Console.WriteLine("Посредник: заказ утверждён менеджером, передаём на склад.");
                    Warehouse?.ReserveOrder(order);
                    break;

                case "OrderPrepared":
                    // проверка, кто может подтверждать подготовку заказа
                    if (!ReferenceEquals(sender, Warehouse))
                    {
                        Console.WriteLine("Посредник: только склад может подтверждать подготовку заказа. Попытка отклонена.");
                        return;
                    }

                    Console.WriteLine("Посредник: заказ подготовлен на складе, уведомляем клиента.");
                    Client?.NotifyOrderReady(order);
                    break;

                default:
                    Console.WriteLine($"Посредник: неизвестный тип события '{eventCode}', действие проигнорировано.");
                    break;
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // создаём посредника и компоненты
            var mediator = new OrderMediator();

            var client = new Client(mediator, "Иван");
            var manager = new Manager(mediator, "Ольга");
            var warehouse = new Warehouse(mediator, "Склад");

            // связываем компоненты с посредником
            mediator.Client = client;
            mediator.Manager = manager;
            mediator.Warehouse = warehouse;

            Console.WriteLine("=== Система управления заказами ===\n");

            Console.Write("Введите название товара: ");
            string productName = Console.ReadLine();

            Console.Write("Введите количество: ");
            string quantityText = Console.ReadLine();

            if (!int.TryParse(quantityText, out int quantity))
            {
                Console.WriteLine("Ошибка: количество должно быть целым числом.");
                return;
            }


            client.PlaceOrder(productName, quantity);

        }
    }
}
