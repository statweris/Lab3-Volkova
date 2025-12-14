/*Обработка ситуации, когда в каталоге нет товаров, соответствующих критерию

1. Для всех итераторов:
   а) В конструкторе проверяем, передан ли null или пустой список продуктов
   б) Если да, инициализируем пустой внутренний список
   в) Все методы (HasNext, Next, Next(count)) корректно работают с пустым списком

2. Для CategoryIterator с конкретной категорией:
   а) Если в переданном списке нет товаров с указанной категорией, создаем пустой отфильтрованный список
   б) Итератор будет вести себя как пустой

3. В классе Catalog:
   а) При отображении проверяем, установлен ли итератор
   б) Проверяем результат работы итератора и выводим соответствующее сообщение
   в) Добавлен метод GetAllProducts() для получения товаров из каталога
*/


using System;
using System.Collections.Generic;
using System.Linq;
public interface ICatalogIterator // интерфейс итератора
{
    bool HasNext();
    Product Next();
    List<Product> Next(int count);
    void Reset();
}

public class Product // класс товара
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public decimal Price { get; set; }
    public int PopularityScore { get; set; } // оценка популярности (чем выше, тем популярнее)

    public Product(string id, string name, string category, decimal price, int popularityScore)
    {
        Id = id;
        Name = name;
        Category = category;
        Price = price;
        PopularityScore = popularityScore;
    }

    public override string ToString()
    {
        return $"{Name} (Категория: {Category}, Цена: {Price:C}, Популярность: {PopularityScore})";
    }
}


public class CategoryIterator : ICatalogIterator // итератор по категориям
{
    private readonly List<Product> _filteredProducts;
    private int _currentPosition;

    public CategoryIterator(List<Product> products, string targetCategory = null)
    {
        if (products == null || products.Count == 0)
        {
            _filteredProducts = new List<Product>();
        }
        else if (!string.IsNullOrEmpty(targetCategory))
        {
            _filteredProducts = products
                .Where(p => p.Category.Equals(targetCategory, StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p.Name)
                .ToList();
        }
        else
        {
            _filteredProducts = products
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToList();
        }

        Reset();
    }

    public bool HasNext()
    {
        return _currentPosition < _filteredProducts.Count;
    }

    public Product Next()
    {
        if (!HasNext())
        {
            return null;
        }

        var product = _filteredProducts[_currentPosition];
        _currentPosition++;
        return product;
    }

    public List<Product> Next(int count)
    {
        var result = new List<Product>();

        for (int i = 0; i < count && HasNext(); i++)
        {
            result.Add(Next());
        }

        return result;
    }

    public void Reset()
    {
        _currentPosition = 0;
    }
}

public class PriceIterator : ICatalogIterator // итератор по цене (от меньшей к большей)
{
    private readonly List<Product> _sortedProducts;
    private int _currentPosition;

    public PriceIterator(List<Product> products)
    {
        if (products == null || products.Count == 0)
        {
            _sortedProducts = new List<Product>();
        }
        else
        {
            _sortedProducts = products
                .OrderBy(p => p.Price)
                .ThenBy(p => p.Name)
                .ToList();
        }

        Reset();
    }

    public bool HasNext()
    {
        return _currentPosition < _sortedProducts.Count;
    }

    public Product Next()
    {
        if (!HasNext())
        {
            return null;
        }

        var product = _sortedProducts[_currentPosition];
        _currentPosition++;
        return product;
    }

    public List<Product> Next(int count)
    {
        var result = new List<Product>();

        for (int i = 0; i < count && HasNext(); i++)
        {
            result.Add(Next());
        }

        return result;
    }

    public void Reset()
    {
        _currentPosition = 0;
    }
}

public class PopularityIterator : ICatalogIterator // итератор по популярности (от более популярных к менее популярным)
{
    private readonly List<Product> _sortedProducts;
    private int _currentPosition;

    public PopularityIterator(List<Product> products)
    {
        if (products == null || products.Count == 0)
        {
            _sortedProducts = new List<Product>();
        }
        else
        {
            _sortedProducts = products
                .OrderByDescending(p => p.PopularityScore)
                .ThenBy(p => p.Name)
                .ToList();
        }

        Reset();
    }

    public bool HasNext()
    {
        return _currentPosition < _sortedProducts.Count;
    }

    public Product Next()
    {
        if (!HasNext())
        {
            return null;
        }

        var product = _sortedProducts[_currentPosition];
        _currentPosition++;
        return product;
    }

    public List<Product> Next(int count)
    {
        var result = new List<Product>();

        for (int i = 0; i < count && HasNext(); i++)
        {
            result.Add(Next());
        }

        return result;
    }

    public void Reset()
    {
        _currentPosition = 0;
    }
}

public class Catalog // класс каталога
{
    private readonly List<Product> _products;
    private ICatalogIterator _currentIterator;

    public Catalog()
    {
        _products = new List<Product>();
        _currentIterator = null;
    }

    public void AddProduct(Product product)
    {
        _products.Add(product);
    }

    public void SetIterator(ICatalogIterator iterator)
    {
        _currentIterator = iterator;
        _currentIterator?.Reset();
    }

    public void DisplayProducts()
    {
        if (_currentIterator == null)
        {
            Console.WriteLine("Итератор не установлен.");
            return;
        }

        _currentIterator.Reset();
        int itemNumber = 1;

        Console.WriteLine("Товары в каталоге:");
        Console.WriteLine(new string('-', 60));

        while (_currentIterator.HasNext())
        {
            var product = _currentIterator.Next();
            if (product != null)
            {
                Console.WriteLine($"{itemNumber}. {product}");
                itemNumber++;
            }
        }

        if (itemNumber == 1)
        {
            Console.WriteLine("Нет товаров для отображения.");
        }
    }

    public void DisplayNextProducts(int count)
    {
        if (_currentIterator == null)
        {
            Console.WriteLine("Итератор не установлен.");
            return;
        }

        var products = _currentIterator.Next(count);

        if (products.Count == 0)
        {
            Console.WriteLine("Больше нет товаров для отображения.");
            return;
        }

        Console.WriteLine($"Следующие {products.Count} товаров:");
        Console.WriteLine(new string('-', 60));

        for (int i = 0; i < products.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {products[i]}");
        }
    }

    public int GetProductCount()
    {
        return _products.Count;
    }

    public List<string> GetAvailableCategories()
    {
        return _products
            .Select(p => p.Category)
            .Distinct()
            .ToList();
    }

    public List<Product> GetAllProducts()
    {
        return new List<Product>(_products);
    }
}


class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // создание каталога и добавление товаров
        Catalog productCatalog = new Catalog();

        productCatalog.AddProduct(new Product("P001", "Ноутбук", "Электроника", 1000.99m, 95));
        productCatalog.AddProduct(new Product("P002", "Смартфон", "Электроника", 700.50m, 98));
        productCatalog.AddProduct(new Product("P003", "Футболка", "Одежда", 25.99m, 85));
        productCatalog.AddProduct(new Product("P004", "Джинсы", "Одежда", 80.99m, 88));
        productCatalog.AddProduct(new Product("P005", "Настольная лампа", "Дом", 34.99m, 75));
        productCatalog.AddProduct(new Product("P006", "Кофеварка", "Кухня", 129.99m, 92));
        productCatalog.AddProduct(new Product("P007", "Книга", "Книги", 19.99m, 78));
        productCatalog.AddProduct(new Product("P008", "Наушники", "Электроника", 199.99m, 96));
        productCatalog.AddProduct(new Product("P009", "Кроссовки", "Обувь", 89.99m, 90));
        productCatalog.AddProduct(new Product("P010", "Рюкзак", "Аксессуары", 49.99m, 82));

        Console.WriteLine($"В каталоге {productCatalog.GetProductCount()} товаров");
        Console.WriteLine();

        // получаем список товаров из каталога
        List<Product> allProducts = productCatalog.GetAllProducts();

        // итератор по категориям
        Console.WriteLine("=== Обход по категориям (Электроника) ===");
        productCatalog.SetIterator(new CategoryIterator(allProducts, "Электроника"));
        productCatalog.DisplayProducts();
        Console.WriteLine();

        // итератор по цене
        Console.WriteLine("=== Обход по цене (от дешевых к дорогим) ===");
        productCatalog.SetIterator(new PriceIterator(allProducts));
        productCatalog.DisplayProducts();
        Console.WriteLine();

        // итератор по популярности
        Console.WriteLine("=== Обход по популярности ===");
        productCatalog.SetIterator(new PopularityIterator(allProducts));
        productCatalog.DisplayProducts();
        Console.WriteLine();

        // работа с пустой категорией
        Console.WriteLine("=== Попытка обхода несуществующей категории ===");
        productCatalog.SetIterator(new CategoryIterator(allProducts, "Мебель"));
        productCatalog.DisplayProducts();
        Console.WriteLine();

        // Next(int count)
        Console.WriteLine("=== Постраничный вывод (по 3 товара) по популярности ===");
        productCatalog.SetIterator(new PopularityIterator(allProducts));

        productCatalog.DisplayNextProducts(3);
        Console.WriteLine();
        productCatalog.DisplayNextProducts(3);
        Console.WriteLine();
        productCatalog.DisplayNextProducts(3);
        Console.WriteLine();
        productCatalog.DisplayNextProducts(3);

        // обход всех товаров по категориям
        Console.WriteLine("\n=== Обход всех товаров по категориям ===");
        productCatalog.SetIterator(new CategoryIterator(allProducts));
        productCatalog.DisplayProducts();
        Console.WriteLine();

        // информация о доступных категориях
        Console.WriteLine("Доступные категории:");
        foreach (var category in productCatalog.GetAvailableCategories())
        {
            Console.WriteLine($"- {category}");
        }
    }
}

