/*
Хранение нескольких точек сохранения:
Использование двух стеков - один для истории (undo), другой для отмененных действий (redo). 
При каждом изменении сохраняю снимок в undo. При отмене - переносим в redo.

Ограничения при использовании паттерна Memento:
1. Потребление памяти - каждый снимок хранит полную копию состояния
2. Необходимость ограничивать глубину истории
3. Потенциальные проблемы производительности при частых изменениях

 */

using System;
using System.Collections.Generic;
using System.Linq;

public class ShoppingCartMemento
{
    private readonly List<CartItem> _itemsSnapshot;
    private readonly DateTime _createdAt;

    public ShoppingCartMemento(List<CartItem> items)
    {
        // cоздаем глубокую копию списка товаров
        _itemsSnapshot = items.Select(item => new CartItem(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice)).ToList();
        _createdAt = DateTime.Now;
    }

    public List<CartItem> GetSavedState()
    {
        // возвращаем копию сохраненного состояния
        return _itemsSnapshot.Select(item => new CartItem(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice)).ToList();
    }

    public DateTime GetCreationTime()
    {
        return _createdAt;
    }
}

public class CartItem
{
    public int ProductId { get; }
    public string ProductName { get; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; }

    public decimal TotalPrice => Quantity * UnitPrice;

    public CartItem(int productId, string productName, int quantity, decimal unitPrice)
    {
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public override string ToString()
    {
        return $"{ProductName} (ID: {ProductId}) - {Quantity} × {UnitPrice:C} = {TotalPrice:C}";
    }
}
public class ShoppingCart
{
    private List<CartItem> _items;

    public ShoppingCart()
    {
        _items = new List<CartItem>();
    }

    public void AddProduct(int productId, string productName, int quantity, decimal unitPrice)
    {
        var existingItem = _items.FirstOrDefault(item => item.ProductId == productId);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
            Console.WriteLine($"Обновлено количество товара '{productName}': {existingItem.Quantity} шт.");
        }
        else
        {
            _items.Add(new CartItem(productId, productName, quantity, unitPrice));
            Console.WriteLine($"Добавлен товар '{productName}': {quantity} шт.");
        }
    }

    public void RemoveProduct(int productId, int quantityToRemove = 0)
    {
        var itemToRemove = _items.FirstOrDefault(item => item.ProductId == productId);

        if (itemToRemove == null)
        {
            Console.WriteLine($"Товар с ID {productId} не найден в корзине.");
            return;
        }

        if (quantityToRemove <= 0 || quantityToRemove >= itemToRemove.Quantity)
        {
            _items.Remove(itemToRemove);
            Console.WriteLine($"Товар '{itemToRemove.ProductName}' полностью удален из корзины.");
        }
        else
        {
            itemToRemove.Quantity -= quantityToRemove;
            Console.WriteLine($"Уменьшено количество товара '{itemToRemove.ProductName}': осталось {itemToRemove.Quantity} шт.");
        }
    }

    public void ClearCart()
    {
        _items.Clear();
        Console.WriteLine("Корзина полностью очищена.");
    }

    public ShoppingCartMemento SaveState()
    {
        Console.WriteLine($"Сохранено состояние корзины ({_items.Count} товаров)");
        return new ShoppingCartMemento(_items);
    }

    public void RestoreState(ShoppingCartMemento memento)
    {
        if (memento == null)
        {
            Console.WriteLine("Не удалось восстановить состояние: снимок отсутствует.");
            return;
        }

        _items = memento.GetSavedState();
        Console.WriteLine($"Восстановлено состояние корзины из снимка от {memento.GetCreationTime():HH:mm:ss}");
    }

    public void DisplayCart()
    {
        if (_items.Count == 0)
        {
            Console.WriteLine("Корзина пуста.");
            return;
        }

        Console.WriteLine("\n=== СОДЕРЖИМОЕ КОРЗИНЫ ===");
        foreach (var item in _items)
        {
            Console.WriteLine(item);
        }

        decimal total = _items.Sum(item => item.TotalPrice);
        Console.WriteLine($"ИТОГО: {total:C}");
        Console.WriteLine($"Количество товаров: {_items.Sum(item => item.Quantity)} шт.");
        Console.WriteLine("=========================\n");
    }

    public int GetItemsCount()
    {
        return _items.Count;
    }
}


public class ShoppingCartHistory
{
    private readonly Stack<ShoppingCartMemento> _undoStack;
    private readonly Stack<ShoppingCartMemento> _redoStack;
    private readonly int _maxHistorySize;

    public ShoppingCartHistory(int maxHistorySize = 10)
    {
        _undoStack = new Stack<ShoppingCartMemento>();
        _redoStack = new Stack<ShoppingCartMemento>();
        _maxHistorySize = maxHistorySize;
    }

    public void SaveState(ShoppingCart cart)
    {
        if (_undoStack.Count >= _maxHistorySize)
        {
            // удаляем самый старый снимок (дно стека)
            var tempStack = new Stack<ShoppingCartMemento>();
            while (_undoStack.Count > 1)
            {
                tempStack.Push(_undoStack.Pop());
            }
            _undoStack.Clear();
            while (tempStack.Count > 0)
            {
                _undoStack.Push(tempStack.Pop());
            }
        }

        _undoStack.Push(cart.SaveState());
        _redoStack.Clear(); // при новом действии очищаем историю redo
    }

    public void Undo(ShoppingCart cart)
    {
        if (_undoStack.Count <= 1) // Нельзя отменить, если есть только текущее состояние или ничего
        {
            Console.WriteLine("Невозможно отменить: история изменений пуста.");
            return;
        }

        // сохраняем текущее состояние в redo-стек
        _redoStack.Push(_undoStack.Peek());

        // удаляем текущее состояние
        _undoStack.Pop();

        // восстанавливаем предыдущее состояние
        var previousState = _undoStack.Peek();
        cart.RestoreState(previousState);
    }

    public void Redo(ShoppingCart cart)
    {
        if (_redoStack.Count == 0)
        {
            Console.WriteLine("Невозможно повторить: нет отмененных действий.");
            return;
        }

        var stateToRestore = _redoStack.Pop();
        _undoStack.Push(stateToRestore);
        cart.RestoreState(stateToRestore);
    }

    public int GetHistorySize()
    {
        return _undoStack.Count;
    }

    public int GetRedoSize()
    {
        return _redoStack.Count;
    }

    public void ClearHistory()
    {
        _undoStack.Clear();
        _redoStack.Clear();
        Console.WriteLine("История изменений очищена.");
    }
}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== СИСТЕМА УПРАВЛЕНИЯ КОРЗИНОЙ ПОКУПОК ВОЛКОВОЙ АНГЕЛИНЫ===\n");

        var shoppingCart = new ShoppingCart();
        var cartHistory = new ShoppingCartHistory(maxHistorySize: 5);

        // сохраняем начальное состояние (пустая корзина)
        cartHistory.SaveState(shoppingCart);

        bool isRunning = true;

        while (isRunning)
        {
            Console.WriteLine("\nКоманды:");
            Console.WriteLine("1 - Показать корзину");
            Console.WriteLine("2 - Добавить товар");
            Console.WriteLine("3 - Удалить товар");
            Console.WriteLine("4 - Отменить последнее действие (Undo)");
            Console.WriteLine("5 - Повторить действие (Redo)");
            Console.WriteLine("6 - Очистить корзину");
            Console.WriteLine("7 - Показать размер истории");
            Console.WriteLine("0 - Выход");
            Console.Write("\nВыберите команду: ");

            if (!int.TryParse(Console.ReadLine(), out int command))
            {
                Console.WriteLine("Неверная команда.");
                continue;
            }

            switch (command)
            {
                case 1:
                    shoppingCart.DisplayCart();
                    break;

                case 2:
                    Console.Write("Введите ID товара: ");
                    int id = int.Parse(Console.ReadLine());
                    Console.Write("Введите название товара: ");
                    string name = Console.ReadLine();
                    Console.Write("Введите количество: ");
                    int quantity = int.Parse(Console.ReadLine());
                    Console.Write("Введите цену за единицу: ");
                    decimal price = decimal.Parse(Console.ReadLine());

                    shoppingCart.AddProduct(id, name, quantity, price);
                    cartHistory.SaveState(shoppingCart);
                    break;

                case 3:
                    Console.Write("Введите ID товара для удаления: ");
                    int removeId = int.Parse(Console.ReadLine());
                    Console.Write("Введите количество для удаления (0 для полного удаления): ");
                    int removeQuantity = int.Parse(Console.ReadLine());

                    shoppingCart.RemoveProduct(removeId, removeQuantity);
                    cartHistory.SaveState(shoppingCart);
                    break;

                case 4:
                    Console.WriteLine($"\n--- Отмена действия ---");
                    cartHistory.Undo(shoppingCart);
                    break;

                case 5:
                    Console.WriteLine($"\n--- Повтор действия ---");
                    cartHistory.Redo(shoppingCart);
                    break;

                case 6:
                    shoppingCart.ClearCart();
                    cartHistory.SaveState(shoppingCart);
                    break;

                case 7:
                    Console.WriteLine($"Размер истории: {cartHistory.GetHistorySize()} снимков");
                    Console.WriteLine($"Доступно повторов: {cartHistory.GetRedoSize()} действий");
                    break;

                case 0:
                    isRunning = false;
                    Console.WriteLine("Выход из программы.");
                    break;

                default:
                    Console.WriteLine("Неизвестная команда.");
                    break;
            }
        }
    }
}
