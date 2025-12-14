/*Отмена нескольких команд реализована с помощью стека CommandHistory.
Все успешно выполненные команды добавляются в стек.
Чтобы отменить несколько действий, команды последовательно извлекаются из стека и для каждой вызывается метод Undo().
Система откатывает состояние шаг за шагом в обратном порядке. 

Ограничения:
1. Не все команды обратимы (если действие не изменило состояние лифта, то оно не попадёт в историю и не может быть отменено).
2. История команд может расти без ограничений.*/


using System;
using System.Collections.Generic;

namespace Command_pattern
{
    class Program
    {
        static void Main(string[] args)
        {
            Building building = Building.GetBuilding();
            Elevator elevator = building.Elevator;

            CommandHistory commandHistory = new CommandHistory();
            LiftControl liftControl = new LiftControl(commandHistory);

            ShowHelp();

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine(
                    $"Состояние лифта: этаж {elevator.CurrentFloor}, двери {(elevator.IsDoorOpen ? "открыты" : "закрыты")}");
                Console.Write("Введите команду: ");

                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }

                if (input.Equals("0", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                switch (input)
                {
                    case "1":
                        liftControl.ExecuteCommand(new MoveUpCommand(elevator, building));
                        break;
                    case "2":
                        liftControl.ExecuteCommand(new MoveDownCommand(elevator, building));
                        break;
                    case "3":
                        liftControl.ExecuteCommand(new OpenDoorCommand(elevator));
                        break;
                    case "4":
                        liftControl.ExecuteCommand(new CloseDoorCommand(elevator));
                        break;
                    case "5":
                        liftControl.UndoLastCommand();
                        break;
                    case "6":
                        Console.Write("На какой этаж поехать? ");
                        if (int.TryParse(Console.ReadLine(), out int targetFloor))
                        {
                            if (targetFloor < 1 || targetFloor > 10)
                            {
                                Console.WriteLine("Этаж вне диапазона 1–10.");
                                break;
                            }
                            liftControl.ExecuteCommand(new GoToFloorCommand(elevator, building, targetFloor));
                        }
                        else
                        {
                            Console.WriteLine("Некорректный номер этажа.");
                        }
                        break;

                    default:
                        Console.WriteLine("Неизвестная команда.");
                        break;
                }
            }

            Console.WriteLine("Программа завершена.");
        }

        private static void ShowHelp()
        {
            Console.WriteLine("=== Система управления лифтом Волковой Ангелины ===");
            Console.WriteLine("1 - подняться на этаж выше");
            Console.WriteLine("2 - опуститься на этаж ниже");
            Console.WriteLine("3 - открыть двери");
            Console.WriteLine("4 - закрыть двери");
            Console.WriteLine("5 - отменить последнюю команду");
            Console.WriteLine("6 - доехать до указанного этажа");
            Console.WriteLine("0 - выход");
        }
    }

    public class Building
    {
        private static Building _building;

        private readonly List<Floor> _floors;

        public Elevator Elevator { get; private set; }

        public static Building GetBuilding()
        {
            if (_building == null)
            {
                _building = new Building();
            }

            return _building;
        }

        private Building()
        {

            _floors = new List<Floor>();
            CreateFloors();

            Elevator = Elevator.GetElevator(1, _floors.Count);
        }

        private void CreateFloors()
        {
            for (int i = 1; i <= 10; i++)
            {
                _floors.Add(new Floor(i));
            }
        }

        public Floor GetFloor(int floorNumber)
        {
            if (floorNumber < 1 || floorNumber > _floors.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(floorNumber), "Этаж отсутствует в здании.");
            }

            return _floors[floorNumber - 1];
        }
    }

    public struct Floor
    {
        public int Number { get; }

        public string RoomA { get; }

        public string RoomB { get; }

        public Floor(int number)
        {
            Number = number;
            RoomA = "Помещение " + '1';
            RoomB = "Помещение " + '2';
        }

        public void ShowRooms()
        {
            Console.WriteLine("На этом этаже {0}: {1}, {2}", Number, RoomA, RoomB);
        }
    }

    public class Elevator
    {
        private static Elevator _elevator;

        private readonly int _bottomFloor;
        private readonly int _topFloor;

        public int CurrentFloor { get; private set; }

        public bool IsDoorOpen { get; private set; }

        private Elevator(int bottomFloor, int topFloor)
        {
            if (bottomFloor > topFloor)
            {
                throw new ArgumentException("Нижний этаж не может быть выше верхнего.");
            }

            _bottomFloor = bottomFloor;
            _topFloor = topFloor;

            CurrentFloor = _bottomFloor;
            IsDoorOpen = false;

        }

        public static Elevator GetElevator(int bottomFloor, int topFloor)
        {
            if (_elevator == null)
            {
                _elevator = new Elevator(bottomFloor, topFloor);
            }

            return _elevator;
        }

        public void MoveUp()
        {
            if (IsDoorOpen)
            {
                Console.WriteLine("Нельзя ехать вверх с открытыми дверями.");
                return;
            }

            if (CurrentFloor >= _topFloor)
            {
                Console.WriteLine("Лифт уже на верхнем этаже.");
                return;
            }

            CurrentFloor++;
            Console.WriteLine("Лифт поднялся на {0} этаж.", CurrentFloor);
        }

        public void MoveDown()
        {
            if (IsDoorOpen)
            {
                Console.WriteLine("Нельзя ехать вниз с открытыми дверями.");
                return;
            }

            if (CurrentFloor <= _bottomFloor)
            {
                Console.WriteLine("Лифт уже на нижнем этаже.");
                return;
            }

            CurrentFloor--;
            Console.WriteLine("Лифт опустился на {0} этаж.", CurrentFloor);
        }

        public void OpenDoor()
        {
            if (IsDoorOpen)
            {
                Console.WriteLine("Двери уже открыты.");
                return;
            }

            IsDoorOpen = true;
            Console.WriteLine("Двери лифта открыты.");
        }

        public void CloseDoor()
        {
            if (!IsDoorOpen)
            {
                Console.WriteLine("Двери уже закрыты.");
                return;
            }

            IsDoorOpen = false;
            Console.WriteLine("Двери лифта закрыты.");
        }
    }


    public abstract class Command
    {
        public abstract string Name { get; }

        //чтобы не добавлять в историю неуспешные команды
        public bool IsExecutedSuccessfully { get; protected set; }

        public abstract void Execute();

        public abstract void Undo();
    }

    public class MoveUpCommand : Command
    {
        private readonly Elevator _elevator;
        private readonly Building _building;

        public MoveUpCommand(Elevator elevator, Building building)
        {
            _elevator = elevator;
            _building = building;
        }

        public override string Name => "Движение вверх";

        public override void Execute()
        {
            int floorBefore = _elevator.CurrentFloor;
            _elevator.MoveUp();
            IsExecutedSuccessfully = _elevator.CurrentFloor != floorBefore;

            if (IsExecutedSuccessfully)
            {
                Floor floor = _building.GetFloor(_elevator.CurrentFloor);
                floor.ShowRooms();
            }
        }

        public override void Undo()
        {
            if (!IsExecutedSuccessfully)
            {
                return;
            }

            _elevator.MoveDown();
            Console.WriteLine("Отмена: лифт возвращён на предыдущий этаж.");

            Floor floor = _building.GetFloor(_elevator.CurrentFloor);
            floor.ShowRooms();
        }
    }

    public class MoveDownCommand : Command
    {
        private readonly Elevator _elevator;
        private readonly Building _building;

        public MoveDownCommand(Elevator elevator, Building building)
        {
            _elevator = elevator;
            _building = building;
        }

        public override string Name => "Движение вниз";


        public override void Execute()
        {
            int floorBefore = _elevator.CurrentFloor;
            _elevator.MoveDown();
            IsExecutedSuccessfully = _elevator.CurrentFloor != floorBefore;

            if (IsExecutedSuccessfully)
            {
                Floor floor = _building.GetFloor(_elevator.CurrentFloor);
                floor.ShowRooms();
            }
        }

        public override void Undo()
        {
            if (!IsExecutedSuccessfully)
            {
                return;
            }

            _elevator.MoveUp();
            Console.WriteLine("Отмена: лифт возвращён на предыдущий этаж.");

            Floor floor = _building.GetFloor(_elevator.CurrentFloor);
            floor.ShowRooms();
        }
    }

    public class OpenDoorCommand : Command
    {
        private readonly Elevator _elevator;

        public OpenDoorCommand(Elevator elevator)
        {
            _elevator = elevator ?? throw new ArgumentNullException(nameof(elevator));
        }

        public override string Name
        {
            get { return "Открыть двери"; }
        }

        public override void Execute()
        {
            bool doorWasOpen = _elevator.IsDoorOpen;
            _elevator.OpenDoor();
            IsExecutedSuccessfully = !doorWasOpen && _elevator.IsDoorOpen;
        }

        public override void Undo()
        {
            if (!IsExecutedSuccessfully)
            {
                return;
            }

            _elevator.CloseDoor();
            Console.WriteLine("Отмена: двери снова закрыты.");
        }
    }

    public class CloseDoorCommand : Command
    {
        private readonly Elevator _elevator;

        public CloseDoorCommand(Elevator elevator)
        {
            _elevator = elevator ?? throw new ArgumentNullException(nameof(elevator));
        }

        public override string Name
        {
            get { return "Закрыть двери"; }
        }

        public override void Execute()
        {
            bool doorWasOpen = _elevator.IsDoorOpen;
            _elevator.CloseDoor();
            IsExecutedSuccessfully = doorWasOpen && !_elevator.IsDoorOpen;
        }

        public override void Undo()
        {
            if (!IsExecutedSuccessfully)
            {
                return;
            }

            _elevator.OpenDoor();
            Console.WriteLine("Отмена: двери снова открыты.");
        }
    }

    public class GoToFloorCommand : Command //команда движения на определённый этаж
    {
        private readonly Elevator _elevator;
        private readonly Building _building;
        private readonly int _targetFloor;
        private readonly List<Command> _executedCommands = new List<Command>();
        private int _initialFloor;
        private bool _initialDoorState;

        public GoToFloorCommand(Elevator elevator, Building building, int targetFloor)
        {
            _elevator = elevator ?? throw new ArgumentNullException(nameof(elevator));
            _building = building ?? throw new ArgumentNullException(nameof(building));
            _targetFloor = targetFloor;
        }

        public override string Name => $"Поездка на этаж {_targetFloor}";

        public override void Execute()
        {
            //сохраняем начальное состояние
            _initialFloor = _elevator.CurrentFloor;
            _initialDoorState = _elevator.IsDoorOpen;

            //закрываем двери, если они открыты
            if (_elevator.IsDoorOpen)
            {
                var closeDoorCommand = new CloseDoorCommand(_elevator);
                closeDoorCommand.Execute();
                if (closeDoorCommand.IsExecutedSuccessfully)
                {
                    _executedCommands.Add(closeDoorCommand);
                }
            }

            //двигаемся к определённому этажу
            while (_elevator.CurrentFloor < _targetFloor)
            {
                var moveUpCommand = new MoveUpCommand(_elevator, _building);
                moveUpCommand.Execute();
                if (moveUpCommand.IsExecutedSuccessfully)
                {
                    _executedCommands.Add(moveUpCommand);
                }
                else
                {
                    break;
                }
            }

            while (_elevator.CurrentFloor > _targetFloor)
            {
                var moveDownCommand = new MoveDownCommand(_elevator, _building);
                moveDownCommand.Execute();
                if (moveDownCommand.IsExecutedSuccessfully)
                {
                    _executedCommands.Add(moveDownCommand);
                }
                else
                {
                    break;
                }
            }

            //открываем двери, если мы на нужном этаже
            if (_elevator.CurrentFloor == _targetFloor)
            {
                var openDoorCommand = new OpenDoorCommand(_elevator);
                openDoorCommand.Execute();
                if (openDoorCommand.IsExecutedSuccessfully)
                {
                    _executedCommands.Add(openDoorCommand);
                }
            }

            IsExecutedSuccessfully = _executedCommands.Count > 0;
        }

        public override void Undo()
        {
            if (!IsExecutedSuccessfully)
            {
                return;
            }

            Console.WriteLine("Отмена поездки на этаж {0}", _targetFloor);

            //отменяем команды
            for (int i = _executedCommands.Count - 1; i >= 0; i--)
            {
                _executedCommands[i].Undo();
            }

            _executedCommands.Clear();
        }
    }

    public class CommandHistory
    {
        private readonly Stack<Command> _commands = new Stack<Command>();

        public bool HasCommands
        {
            get { return _commands.Count > 0; }
        }

        public void Push(Command command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            _commands.Push(command);
        }

        public Command Pop()
        {
            if (_commands.Count == 0)
            {
                return null;
            }

            return _commands.Pop();
        }
    }


    public class LiftControl
    {
        private readonly CommandHistory _commandHistory;

        public LiftControl(CommandHistory commandHistory)
        {
            _commandHistory = commandHistory ?? throw new ArgumentNullException(nameof(commandHistory));
        }

        public void ExecuteCommand(Command command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            command.Execute();

            if (command.IsExecutedSuccessfully)
            {
                _commandHistory.Push(command);
            }
            else
            {
                Console.WriteLine(
                    "Команда \"{0}\" не была выполнена, поэтому не сохранена в истории.",
                    command.Name);
            }
        }

        public void UndoLastCommand()
        {
            Command lastCommand = _commandHistory.Pop();

            if (lastCommand == null)
            {
                Console.WriteLine("Нет команд для отмены.");
                return;
            }

            Console.WriteLine("Отмена команды: {0}", lastCommand.Name);
            lastCommand.Undo();
        }
    }
}
