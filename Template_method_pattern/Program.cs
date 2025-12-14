/* Чтобы добавить новый тип заказа (например, заказ с предоплатой), нужно:
1. Добавить OrderType.Prepaid.
2. В Order добавить bool IsPaid + MarkPaid().
3. В Order.Process() расширить выбор обработчика: Prepaid => new PrepaidOrderProcessing().
4. Создать PrepaidOrderProcessing : OrderProcessing, где будет:
        Pay(order): принять предоплату → order.MarkPaid().
        Доставка: переопределить Deliver(order) и не отгружать без оплаты
ProcessOrder() не меняем, так как предоплата реализуется переопределением шагов в самом подклассе.*/


namespace Template_method_pattern
{
    public enum OrderType
    {
        Standard = 1,
        Express = 2
    }

    public class Order
    {
        public string ProductName { get; }
        public int Quantity { get; }
        public decimal UnitPrice { get; }
        public string DeliveryAddress { get; }
        public OrderType Type { get; }

        public decimal TotalAmount => UnitPrice * Quantity;

        public Order(string productName, int quantity, decimal unitPrice, string deliveryAddress, OrderType type)
        {
            ProductName = productName;
            Quantity = quantity;
            UnitPrice = unitPrice;
            DeliveryAddress = deliveryAddress;
            Type = type;
        }

        public void Process()
        {
            OrderProcessing processor;
            switch (Type)
            {
                case OrderType.Standard:
                    processor = new StandardOrderProcessing();
                    break;
                case OrderType.Express:
                    processor = new ExpressOrderProcessing();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Type), "Неизвестный тип заказа.");
            }

            processor.ProcessOrder(this);
        }
    }

    public abstract class OrderProcessing
    {
        public void ProcessOrder(Order order)
        {
            SelectProduct(order);
            Checkout(order);
            Pay(order);
            Deliver(order);
        }

        protected virtual void SelectProduct(Order order)
        {
            Console.WriteLine($"Выбран товар: {order.ProductName}");
            Console.WriteLine($"Количество: {order.Quantity}");
            Console.WriteLine($"Итого: {order.TotalAmount}");
        }

        protected virtual void Checkout(Order order)
        {
            Console.WriteLine("Оформление заказа...");
            Console.WriteLine($"Адрес доставки: {order.DeliveryAddress}");
        }

        protected virtual void Pay(Order order)
        {
            Console.WriteLine("Оплата заказа...");
            Console.WriteLine("Оплата принята.");
        }

        protected virtual void Deliver(Order order)
        {
            var method = GetDeliveryMethod(order);
            Console.WriteLine($"Доставка: {method}");
            Console.WriteLine("Заказ передан в службу доставки.");
        }

        protected abstract string GetDeliveryMethod(Order order);
    }

    public class StandardOrderProcessing : OrderProcessing
    {
        protected override string GetDeliveryMethod(Order order)
        {
            return "Стандартная доставка (3–5 дней)";
        }
    }

    public class ExpressOrderProcessing : OrderProcessing
    {
        protected override string GetDeliveryMethod(Order order)
        {
            return "Экспресс-доставка (1–2 дня)";
        }
    }

    internal class Program
    {
        private static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("Выберите тип заказа:");
            Console.WriteLine("1 — Стандартный");
            Console.WriteLine("2 — Экспресс");
            Console.Write("Ввод: ");
            var typeText = Console.ReadLine();

            if (!int.TryParse(typeText, out var typeNumber) || (typeNumber != 1 && typeNumber != 2))
            {
                Console.WriteLine("Некорректный выбор типа заказа.");
                return;
            }

            var orderType = (OrderType)typeNumber;

            Console.Write("Название товара: ");
            var productName = Console.ReadLine() ?? "Товар";

            Console.Write("Количество: ");
            if (!int.TryParse(Console.ReadLine(), out var quantity) || quantity <= 0)
            {
                Console.WriteLine("Некорректное количество.");
                return;
            }

            Console.Write("Цена за единицу: ");
            if (!decimal.TryParse(Console.ReadLine(), out var unitPrice) || unitPrice < 0)
            {
                Console.WriteLine("Некорректная цена.");
                return;
            }

            Console.Write("Адрес доставки: ");
            var address = Console.ReadLine() ?? "Адрес не указан";

            var order = new Order(productName, quantity, unitPrice, address, orderType);

            Console.WriteLine();
            Console.WriteLine("=== Обработка заказа ===");
            order.Process();
            Console.WriteLine("=== Готово ===");
        }
    }
}
