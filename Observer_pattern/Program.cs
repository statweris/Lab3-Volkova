using System;
using System.Collections.Generic;

namespace Observer_pattern
{
    public enum OrderStatus
    {
        Placed,      
        Processing, 
        Shipped,    
        Delivered 
    }

    public static class OrderStatusExtensions
    {
        public static string ToRussian(this OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Placed => "'Оформлен'",
                OrderStatus.Processing => "'В обработке'",
                OrderStatus.Shipped => "'Отправлен'",
                OrderStatus.Delivered => "'Доставлен'",
                _ => status.ToString()
            };
        }
    }


    public interface IOrderObserver     //наблюдатель за заказом
    {
        void OnOrderStatusChanged(Order order, OrderStatus previousStatus);
    }

    public class Order
    {
        private readonly List<IOrderObserver> observers = new List<IOrderObserver>();

        public int Id { get; }
        public OrderStatus Status { get; private set; }

        public Order(int id, OrderStatus initialStatus)
        {
            Id = id;
            Status = initialStatus;
        }

        public void AddObserver(IOrderObserver observer)
        {
            if (observer == null)
                throw new ArgumentNullException(nameof(observer));

            if (!observers.Contains(observer))
            {
                observers.Add(observer);
            }
        }

        public void RemoveObserver(IOrderObserver observer)
        {
            if (observer == null)
                throw new ArgumentNullException(nameof(observer));

            observers.Remove(observer);
        }

        public void ChangeStatus(OrderStatus newStatus)
        {
            if (Status == newStatus)
            {
                return;
            }

            var previousStatus = Status;
            Status = newStatus;

            NotifyObservers(previousStatus);
        }

        public void NotifyObservers(OrderStatus previousStatus)
        {
            foreach (var observer in observers)
            {
                observer.OnOrderStatusChanged(this, previousStatus);
            }
        }
    }

    public class ClientNotification : IOrderObserver
    {
        private readonly string clientName;
        private readonly string contactInfo;

        public ClientNotification(string clientName, string contactInfo)
        {
            this.clientName = clientName;
            this.contactInfo = contactInfo;
        }

        public void OnOrderStatusChanged(Order order, OrderStatus previousStatus)
        {
            Console.WriteLine(
                $"[Клиент {clientName}] Заказ #{order.Id}: статус изменился с {previousStatus.ToRussian()} на {order.Status.ToRussian()}. " +
                $"Уведомление отправлено по адресу {contactInfo}.");
        }
    }


    public class ManagerNotification : IOrderObserver
    {
        private readonly string managerName;

        public ManagerNotification(string managerName)
        {
            this.managerName = managerName;
        }

        public void OnOrderStatusChanged(Order order, OrderStatus previousStatus)
        {
            Console.WriteLine(
                $"[Менеджер {managerName}] Заказ #{order.Id}: статус изменился с {previousStatus.ToRussian()} на {order.Status.ToRussian()}. " +
                "Проверьте корректность обработки заказа.\n");
        }
    }

    public class AnalyticsSystem : IOrderObserver
    {
        public void OnOrderStatusChanged(Order order, OrderStatus previousStatus)
        {
            Console.WriteLine(
                $"[Аналитическая система] Зафиксировано изменение статуса заказа #{order.Id}: {previousStatus.ToRussian()} -> {order.Status.ToRussian()}.");
        }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            //заказ и наблюдатели
            var order = new Order(id: 1, initialStatus: OrderStatus.Placed);

            var clientNotification = new ClientNotification("Ангелина Волкова", "volkova@gmail.com");
            var managerNotification = new ManagerNotification("Влада Иванова");
            var analyticsSystem = new AnalyticsSystem();

            //подписываем наблюдателей на обновления по заказу
            order.AddObserver(clientNotification);
            order.AddObserver(managerNotification);
            order.AddObserver(analyticsSystem);

            order.ChangeStatus(OrderStatus.Processing);
            order.ChangeStatus(OrderStatus.Shipped);

            Console.WriteLine("\nМенеджер больше не подписан на уведомления.\n");
            order.RemoveObserver(managerNotification);

            order.ChangeStatus(OrderStatus.Delivered);
        }
    }
}
