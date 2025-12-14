/*
Для нового вида расчёта (например, скидка) нужно создать новый класс-посетитель, 
наследуемый от OrderVisitor, и реализовать в нём VisitProduct() и VisitBox().

Код классов Product, Box и Order менять не нужно, так как у них уже есть метод Accept(visitor).
*/

using System;
using System.Collections.Generic;

namespace Visitor_pattern
{
    // элемент заказа, который можно "посетить" посетителем
    public interface IOrderItem
    {
        decimal GetPrice();
        void Display(int depth = 0);
        void Accept(OrderVisitor visitor);
    }

    // абстрактный посетитель для элементов заказа
    public abstract class OrderVisitor
    {
        public abstract void VisitProduct(Product product);
        public abstract void VisitBox(Box box);
    }

    // конкретный элемент — товар
    public class Product : IOrderItem
    {
        public string Name { get; set; }
        public decimal Price { get; set; }

        public Product(string name, decimal price)
        {
            Name = name;
            Price = price;
        }

        public decimal GetPrice()
        {
            return Price;
        }

        public void Display(int depth = 0)
        {
            string indent = new string(' ', depth * 4);
            Console.WriteLine($"{indent}Продукт: {Name} - {Price} руб.");
        }

        public void Accept(OrderVisitor visitor)
        {
            visitor.VisitProduct(this);
        }
    }

    // коробка, содержащая другие элементы (товары и коробки)
    public class Box : IOrderItem
    {
        public string Name { get; set; }
        public decimal PackagingCost { get; set; }
        public List<IOrderItem> Contents { get; }

        public Box(string name, decimal packagingCost = 0)
        {
            Name = name;
            PackagingCost = packagingCost;
            Contents = new List<IOrderItem>();
        }

        public void AddItem(IOrderItem item)
        {
            Contents.Add(item);
        }

        // стоимость коробки = стоимость упаковки + стоимость содержимого
        public decimal GetPrice()
        {
            decimal totalPrice = PackagingCost;

            foreach (var item in Contents)
            {
                totalPrice += item.GetPrice();
            }

            return totalPrice;
        }

        public void Display(int depth = 0)
        {
            string indent = new string(' ', depth * 4);
            Console.WriteLine($"{indent}Коробка: {Name} (упаковка: {PackagingCost} руб.)");

            foreach (var item in Contents)
            {
                item.Display(depth + 1);
            }
        }

        public void Accept(OrderVisitor visitor)
        {
            // сначала посетитель обрабатывает саму коробку
            visitor.VisitBox(this);

            // затем последовательно обрабатываются все элементы внутри коробки
            foreach (var item in Contents)
            {
                item.Accept(visitor);
            }
        }
    }

    // заказ как набор элементов
    public class Order
    {
        public List<IOrderItem> Items { get; }

        public Order()
        {
            Items = new List<IOrderItem>();
        }

        public void AddItem(IOrderItem item)
        {
            Items.Add(item);
        }

        public decimal CalculateTotalPrice()
        {
            decimal total = 0;

            foreach (var item in Items)
            {
                total += item.GetPrice();
            }

            return total;
        }

        public void DisplayOrderContents()
        {
            Console.WriteLine("Состав заказа:");
            Console.WriteLine("=========================");

            foreach (var item in Items)
            {
                item.Display();
            }

            Console.WriteLine("=========================");
            Console.WriteLine($"Общая стоимость заказа: {CalculateTotalPrice()} руб.");
        }

        public void Accept(OrderVisitor visitor)
        {
            foreach (var item in Items)
            {
                item.Accept(visitor);
            }
        }
    }

    // посетитель для расчёта стоимости доставки
    public class DeliveryCostCalculator : OrderVisitor
    {
        // итоговая стоимость доставки по заказу
        public decimal TotalDeliveryCost { get; private set; }

        // условная ставка доставки для товаров (процент от цены товара)
        private const decimal ProductDeliveryRate = 0.05m; // 5%

        // условная базовая стоимость обработки каждой коробки
        private const decimal BoxHandlingCost = 100m;

        public override void VisitProduct(Product product)
        {
            // доставка товара зависит от его цены
            TotalDeliveryCost += product.Price * ProductDeliveryRate;
        }

        public override void VisitBox(Box box)
        {
            // для коробки учитываем стоимость упаковки и фиксированную обработку
            TotalDeliveryCost += box.PackagingCost + BoxHandlingCost;
        }
    }

    // посетитель для расчёта налогов
    public class TaxCalculator : OrderVisitor
    {
        // итоговая сумма налогов по заказу
        public decimal TotalTax { get; private set; }

        // условная ставка налога (НДС 20%)
        private const decimal TaxRate = 0.20m;

        public override void VisitProduct(Product product)
        {
            // налог с цены товара
            TotalTax += product.Price * TaxRate;
        }

        public override void VisitBox(Box box)
        {
            // налог со стоимости упаковки
            TotalTax += box.PackagingCost * TaxRate;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var laptop = new Product("Ноутбук", 100000m);
            var mouse = new Product("Компьютерная мышь", 2500m);
            var keyboard = new Product("Механическая клавиатура", 8000m);
            var headphones = new Product("Наушники", 15000m);
            var charger = new Product("Зарядное устройство", 3000m);
            var usbFlash = new Product("USB-флешка", 1500m);
            var extendedWarranty = new Product("Дополнительная гарантия", 7000m);

            var smallBox = new Box("Маленькая коробка", 200m);
            var mediumBox = new Box("Средняя коробка", 500m);
            var largeBox = new Box("Большая коробка", 1000m);

            smallBox.AddItem(mouse);
            smallBox.AddItem(usbFlash);

            mediumBox.AddItem(keyboard);
            mediumBox.AddItem(headphones);
            mediumBox.AddItem(smallBox);

            largeBox.AddItem(laptop);
            largeBox.AddItem(mediumBox);
            largeBox.AddItem(charger);

            var order = new Order();

            order.AddItem(largeBox);
            order.AddItem(extendedWarranty); // простой продукт без упаковки

            order.DisplayOrderContents();

            Console.WriteLine();


            var deliveryCalculator = new DeliveryCostCalculator();
            var taxCalculator = new TaxCalculator();

            order.Accept(deliveryCalculator);
            order.Accept(taxCalculator);

            Console.WriteLine($"Стоимость доставки по заказу: {deliveryCalculator.TotalDeliveryCost} руб.");
            Console.WriteLine($"Сумма налогов по заказу:       {taxCalculator.TotalTax} руб.");
        }
    }
}


