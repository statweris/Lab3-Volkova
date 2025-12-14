using System;
using System.Collections.Generic;

namespace Strategy_pattern
{
    public abstract class PaymentStrategy
    {
        public abstract void ProcessPayment(decimal amount);
    }

    public class CashlessPayment : PaymentStrategy
    {
        private string _cardNumber;
        private string _expiryDate;
        private string _cvvCode;

        public CashlessPayment(string cardNumber, string expiryDate, string cvvCode)
        {
            _cardNumber = cardNumber;
            _expiryDate = expiryDate;
            _cvvCode = cvvCode;
        }

        public override void ProcessPayment(decimal amount)
        {
            Console.WriteLine($"Обработка безналичного платежа на сумму {amount} рублей");
            Console.WriteLine($"Списано с карты {_cardNumber}");
            Console.WriteLine("Платеж успешно завершен\n");
        }
    }

    public class CashPayment : PaymentStrategy
    {
        public override void ProcessPayment(decimal amount)
        {
            Console.WriteLine($"Обработка наличного платежа на сумму {amount} рублей");
            Console.WriteLine("Ожидание оплаты наличными при получении заказа\n");
        }
    }

    public class TransferOnDeliveryPayment : PaymentStrategy
    {
        private string _recipientDetails;

        public TransferOnDeliveryPayment(string recipientDetails)
        {
            _recipientDetails = recipientDetails;
        }

        public override void ProcessPayment(decimal amount)
        {
            Console.WriteLine($"Обработка перевода при получении на сумму {amount} рублей");
            Console.WriteLine($"Получатель: {_recipientDetails}");
            Console.WriteLine("Ожидание перевода при получении заказа\n");
        }
    }

    public class Order
    {
        private PaymentStrategy _paymentStrategy;
        private List<ProductItem> _products = new List<ProductItem>();
        public PaymentStrategy PaymentStrategy //свойство для динамического изменения стратегии оплаты
        {
            set { _paymentStrategy = value; }
        }

        public void AddProduct(string name, decimal price, int quantity)
        {
            _products.Add(new ProductItem(name, price, quantity));
        }

        public void ProcessOrder()
        {
            if (_paymentStrategy == null)
            {
                Console.WriteLine("Ошибка: не выбран способ оплаты");
                return;
            }

            decimal totalAmount = CalculateTotal();
            Console.WriteLine($"Оформление заказа на сумму {totalAmount} рублей");

            _paymentStrategy.ProcessPayment(totalAmount);

            Console.WriteLine("Заказ успешно оформлен!");
        }

        private decimal CalculateTotal()
        {
            decimal total = 0;
            foreach (var product in _products)
            {
                total += product.Price * product.Quantity;
            }
            return total;
        }

        private class ProductItem
        {
            public string Name { get; }
            public decimal Price { get; }
            public int Quantity { get; }

            public ProductItem(string name, decimal price, int quantity)
            {
                Name = name;
                Price = price;
                Quantity = quantity;
            }
        }
    }

    class Program
    {
        // Предположим, что мы хотим добавить оплату криптовалютой
        // 1. Создаём новый класс, наследующий PaymentStrategy  (+ объявляем адрес кошелька и тип криптовалюты)
        // 2. Добавим новый case в switch в методе Main:
        // 3. Обновим меню выбора способов оплаты, добавив пункт "5. Криптовалюта"
        static void Main(string[] args)
        {
            Console.WriteLine("=== Система оплаты интернет-магазина ===\n");

            Order customerOrder = new Order(); //заказ
            customerOrder.AddProduct("Смартфон", 70000, 1);
            customerOrder.AddProduct("Чехол для телефона", 2000, 1);
            customerOrder.AddProduct("Защитное стекло", 900, 2);

            bool exitProgram = false;

            while (!exitProgram)
            {
                Console.WriteLine("\nВыберите способ оплаты:");
                Console.WriteLine("1. Безналичный расчет (банковская карта)");
                Console.WriteLine("2. Наличный расчет");
                Console.WriteLine("3. Перевод при получении");
                Console.WriteLine("4. Выход");
                Console.Write("Ваш выбор: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.Write("Введите номер карты: ");
                        string cardNumber = Console.ReadLine();
                        Console.Write("Введите срок действия (ММ/ГГ): ");
                        string expiryDate = Console.ReadLine();
                        Console.Write("Введите CVV код: ");
                        string cvvCode = Console.ReadLine();

                        customerOrder.PaymentStrategy = new CashlessPayment(cardNumber, expiryDate, cvvCode);
                        customerOrder.ProcessOrder();
                        break;

                    case "2":
                        customerOrder.PaymentStrategy = new CashPayment();
                        customerOrder.ProcessOrder();
                        break;

                    case "3":
                        Console.Write("Введите реквизиты получателя: ");
                        string recipientDetails = Console.ReadLine();

                        customerOrder.PaymentStrategy = new TransferOnDeliveryPayment(recipientDetails);
                        customerOrder.ProcessOrder();
                        break;

                    case "4":
                        exitProgram = true;
                        break;

                    default:
                        Console.WriteLine("Неверный выбор. Попробуйте снова.");
                        break;
                }

                if (!exitProgram && choice != "4")
                {
                    Console.Write("\nХотите выбрать другой способ оплаты для этого же заказа? (y/n): ");
                    string response = Console.ReadLine().ToLower();

                    if (response != "y")
                    {
                        exitProgram = true;
                    }
                }
            }

        }
    }
}