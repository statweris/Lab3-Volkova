/*
 Корректность переходов:
1. Логика переходов инкапсулирована в классах состояний
2. Метод SetState() имеет internal доступ для контроля изменений
3. Проверки в методах ( CancelOrder() ) блокируют недопустимые действия

Добавленные методы:
1. SetState() - внутренний метод смены состояния
2. CancelOrder() - отмена с проверками
3. GetCurrentStatus() - получение текущего статуса

*/
namespace State_pattern
{
    public abstract class OrderState
    {
        public abstract void ProcessOrder(Order order);
        public abstract string GetStatus();
    }

    //состояния

    public class NewState : OrderState
    {
        public override void ProcessOrder(Order order)
        {
            Console.WriteLine("Заказ взят в обработку.");
            order.SetState(new ProcessingState());
        }

        public override string GetStatus()
        {
            return "Новый";
        }
    }

    public class ProcessingState : OrderState
    {
        public override void ProcessOrder(Order order)
        {
            Console.WriteLine("Заказ отправлен покупателю.");
            order.SetState(new ShippedState());
        }

        public override string GetStatus()
        {
            return "В обработке";
        }
    }

    public class ShippedState : OrderState
    {
        public override void ProcessOrder(Order order)
        {
            Console.WriteLine("Заказ доставлен покупателю.");
            order.SetState(new DeliveredState());
        }

        public override string GetStatus()
        {
            return "Отправлен";
        }
    }

    public class DeliveredState : OrderState
    {
        public override void ProcessOrder(Order order)
        {
            Console.WriteLine("Заказ уже доставлен. Дальнейшие действия невозможны.");
        }

        public override string GetStatus()
        {
            return "Доставлен";
        }
    }

    public class CancelledState : OrderState
    {
        public override void ProcessOrder(Order order)
        {
            Console.WriteLine("Заказ отменен. Дальнейшие действия невозможны.");
        }

        public override string GetStatus()
        {
            return "Отменен";
        }
    }

    // класс заказа
    public class Order
    {
        private OrderState _currentState;
        private readonly string _orderId;

        public Order(string orderId)
        {
            _orderId = orderId;
            _currentState = new NewState();
        }

        // метод для установки состояния
        internal void SetState(OrderState newState)
        {
            _currentState = newState;
        }

        // основной метод обработки заказа
        public void ProcessOrder()
        {
            Console.WriteLine($"\nОбработка заказа {_orderId}");
            Console.WriteLine($"Текущий статус: {_currentState.GetStatus()}");
            _currentState.ProcessOrder(this);
        }

        // метод для отмены заказа с проверкой допустимости
        public void CancelOrder()
        {
            if (_currentState is DeliveredState)
            {
                Console.WriteLine("Невозможно отменить доставленный заказ.");
                return;
            }

            if (_currentState is CancelledState)
            {
                Console.WriteLine("Заказ уже отменен.");
                return;
            }

            Console.WriteLine("Заказ отменен.");
            SetState(new CancelledState());
        }

        // метод для получения текущего статуса
        public string GetCurrentStatus()
        {
            return _currentState.GetStatus();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // создаем новый заказ
            Order customerOrder = new Order("ORD-001");

            // последовательная обработка заказа
            customerOrder.ProcessOrder(); // Новый -> В обработке
            customerOrder.ProcessOrder(); // В обработке -> Отправлен
            customerOrder.ProcessOrder(); // Отправлен -> Доставлен
            customerOrder.ProcessOrder(); // Попытка обработки доставленного заказа

            Console.WriteLine("\n--- Создание нового заказа ---");
            Order anotherOrder = new Order("ORD-002");
            anotherOrder.ProcessOrder(); // Новый -> В обработке
            anotherOrder.CancelOrder();  // Отмена заказа
            anotherOrder.ProcessOrder(); // Попытка обработки отмененного заказа

            Console.WriteLine("\n--- Проверка статусов ---");
            Console.WriteLine($"Статус первого заказа: {customerOrder.GetCurrentStatus()}");
            Console.WriteLine($"Статус второго заказа: {anotherOrder.GetCurrentStatus()}");

            // Попытка отменить доставленный заказ
            Console.WriteLine("\n--- Попытка отменить доставленный заказ ---");
            customerOrder.CancelOrder();
        }
    }
}
