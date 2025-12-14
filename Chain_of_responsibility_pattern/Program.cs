/*
Изменения для обработки ситуации, когда запрос не обрабатывается ни одним из обработчиков:

1. Метод HandleRequest() возвращает bool:

        true — запрос обработан текущим обработчиком или кем-то дальше по цепочке;

        false — дошли до конца цепочки, и никто запрос не обработал.

2. В базовом классе Handler:

Реализована логика:

      проверить CanHandle(request);

      если да — вызвать ProcessRequest(request) и вернуть true;

      если нет, передать запрос дальше (_nextHandler.HandleRequest(request)), если следующий есть;

      если следующего нет — вывести сообщение, что запрос не обработан, и вернуть false.

3. В клиентском коде (в Program.ProcessRequest) результат bool handled проверяется:

если handled == false, можно:

      уведомить пользователя,

      записать в лог,

      отправить запрос на ручное рассмотрение и т.д.
*/

namespace Chain_of_responsibility_pattern
{
    // класс заявки на возврат товара
    class ReturnRequest
    {
        public int Id { get; }
        public decimal Amount { get; }
        public string Reason { get; }

        public ReturnRequest(int id, decimal amount, string reason)
        {
            Id = id;
            Amount = amount;
            Reason = reason;
        }
    }

    // абстрактный обработчик
    abstract class Handler
    {
        private Handler _nextHandler;

        public void SetNext(Handler nextHandler)
        {
            _nextHandler = nextHandler;
        }

        public bool HandleRequest(ReturnRequest request)
        {
            if (CanHandle(request))
            {
                ProcessRequest(request);
                return true;
            }

            if (_nextHandler != null)
            {
                Console.WriteLine($"Заявка #{request.Id} передана следующему обработчику.");
                return _nextHandler.HandleRequest(request);
            }

            // дошли до конца цепочки и так и не обработали
            Console.WriteLine($"Заявка #{request.Id} не обработана ни одним из обработчиков.");
            return false;
        }

        // может ли именно этот обработчик взять запрос
        protected abstract bool CanHandle(ReturnRequest request);

        // непосредственно обработка
        protected abstract void ProcessRequest(ReturnRequest request);
    }

    // обработчик: менеджер
    class ManagerHandler : Handler
    {
        // менеджер обрабатывает небольшие суммы, например до 1000
        protected override bool CanHandle(ReturnRequest request)
        {
            return request.Amount <= 1000m;
        }

        protected override void ProcessRequest(ReturnRequest request)
        {
            Console.WriteLine(
                $"Менеджер одобрил возврат по заявке #{request.Id} на сумму {request.Amount} руб. " +
                $"(Причина: {request.Reason})");
        }
    }

    // обработчик: руководитель
    class SupervisorHandler : Handler
    {
        // руководитель обрабатывает суммы от 1001 до 10000
        protected override bool CanHandle(ReturnRequest request)
        {
            return request.Amount > 1000m && request.Amount <= 10000m;
        }

        protected override void ProcessRequest(ReturnRequest request)
        {
            Console.WriteLine(
                $"Руководитель одобрил возврат по заявке #{request.Id} на сумму {request.Amount} руб. " +
                $"(Причина: {request.Reason})");
        }
    }

    // обработчик: служба поддержки
    class SupportHandler : Handler
    {
        // служба поддержки рассматривает сложные случаи,
        // допустим, что до 50000, остальное вне регламента
        protected override bool CanHandle(ReturnRequest request)
        {
            return request.Amount > 10000m && request.Amount <= 50000m;
        }

        protected override void ProcessRequest(ReturnRequest request)
        {
            Console.WriteLine(
                $"Служба поддержки зарегистрировала и одобрила возврат по заявке #{request.Id} " +
                $"на сумму {request.Amount} руб. (Причина: {request.Reason})");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Handler manager = new ManagerHandler();
            Handler supervisor = new SupervisorHandler();
            Handler support = new SupportHandler();

            manager.SetNext(supervisor);
            supervisor.SetNext(support);

            var smallRequest = new ReturnRequest(
                id: 1,
                amount: 500m,
                reason: "Не подошёл размер"
            );

            var mediumRequest = new ReturnRequest(
                id: 2,
                amount: 5000m,
                reason: "Дефект товара"
            );

            var bigRequest = new ReturnRequest(
                id: 3,
                amount: 20000m,
                reason: "Брак в крупной партии"
            );

            // этот запрос никто не возьмёт, т.к. он выше лимитов всех обработчиков
            var tooBigRequest = new ReturnRequest(
                id: 4,
                amount: 100000m,
                reason: "Очень крупный возврат, требуется отдельное согласование"
            );

            ProcessRequest(manager, smallRequest);
            ProcessRequest(manager, mediumRequest);
            ProcessRequest(manager, bigRequest);
            ProcessRequest(manager, tooBigRequest);

        }

        private static void ProcessRequest(Handler firstHandler, ReturnRequest request)
        {
            Console.WriteLine();
            Console.WriteLine(new string('-', 70));
            Console.WriteLine(
                $"Начало обработки заявки #{request.Id} на сумму {request.Amount} руб. " +
                $"(Причина: {request.Reason})");

            bool handled = firstHandler.HandleRequest(request);

            if (!handled)
            {
                Console.WriteLine(
                    $"Заявка #{request.Id} осталась необработанной. " +
                    "Необходимо отдельное рассмотрение или изменение регламента.");
            }
        }
    }
}
