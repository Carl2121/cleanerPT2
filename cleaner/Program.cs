using System;
namespace RobotCleaner
{
    public class Map
    {
        private enum CellType { Empty, Dirt, Obstacle, Cleaned };
        private CellType[,] _grid;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Map(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            _grid = new CellType[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _grid[x, y] = CellType.Empty;
                }
            }
        }

        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < this.Width && y >= 0 && y < this.Height;
        }

        public bool IsDirt(int x, int y)
        {
            return IsInBounds(x, y) && _grid[x, y] == CellType.Dirt;
        }

        public bool IsObstacle(int x, int y)
        {
            return IsInBounds(x, y) && _grid[x, y] == CellType.Obstacle;
        }

        public void AddObstacle(int x, int y)
        {
            _grid[x, y] = CellType.Obstacle;
        }
        public void AddDirt(int x, int y)
        {
            _grid[x, y] = CellType.Dirt;
        }

        public void Clean(int x, int y)
        {
            if (IsInBounds(x, y))
            {
                _grid[x, y] = CellType.Cleaned;
            }
        }
        public void Display(int robotX, int robotY)
        {
            // display the 2d grid, it accepts the location of the robot in x and y
            Console.Clear();
            Console.WriteLine("Vacuum cleaner robot simulation");
            Console.WriteLine("--------------------------------");
            Console.WriteLine("Legends: #=Obstacles, D=Dirt, .=Empty, R=Robot, C=Cleaned");

            //display the grid using loop
            for (int y = 0; y < this.Height; y++)
            {
                for (int x = 0; x < this.Width; x++)
                {
                    if (x == robotX && y == robotY)
                    {
                        Console.Write("R ");
                    }
                    else
                    {
                        switch (_grid[x, y])
                        {
                            case CellType.Empty: Console.Write(". "); break;
                            case CellType.Dirt: Console.Write("D "); break;
                            case CellType.Obstacle: Console.Write("# "); break;
                            case CellType.Cleaned: Console.Write("C "); break;
                        }
                    }
                }
                Console.WriteLine();
            } //outer for loop
              // add delay
            Thread.Sleep(200);
        } // display method
    }//class map
    public interface IStrategy
    {
        void Clean(Robot robot);
    }

    public class Robot
    {
        private readonly Map _map;
        private readonly IStrategy _strategy;

        public int X { get; set; }
        public int Y { get; set; }

        public Map Map { get { return _map; } }

        public Robot(Map map, IStrategy strategy)
        {
            _map = map;
            _strategy = strategy;
            X = 0;
            Y = 0;
        }

        public bool Move(int newX, int newY)
        {
            if (_map.IsInBounds(newX, newY) && !_map.IsObstacle(newX, newY))
            {
                // set the new location
                X = newX;
                Y = newY;
                // display the map with the robot in its location in the grid
                _map.Display(X, Y);
                return true;
            }
            // it cannot move
            return false;
        }// Move method

        public void CleanCurrentSpot()
        {
            if (_map.IsDirt(X, Y))
            {
                _map.Clean(X, Y);
                _map.Display(X, Y);
            }
        }

        public void StartCleaning()
        {
            _strategy.Clean(this);
        }
    }

    public class SomeStrategy : IStrategy
    {
        public void Clean(Robot robot)
        {
            int direction = 1; // 1 = right, -1 = left
            for (int y = 0; y < robot.Map.Height; y++)
            {
                int startX = (direction == 1) ? 0 : robot.Map.Width - 1;
                int endX = (direction == 1) ? robot.Map.Width : -1;

                for (int x = startX; x != endX; x += direction)
                {
                    robot.Move(x, y);
                    robot.CleanCurrentSpot();
                }
                direction *= -1; // Reverse direction for the next row
            }
        }
    }

    public class PerimeterHuggerStrategy : IStrategy
    {
        public void Clean(Robot robot)
        {
            while (robot.Move(robot.X + 1, robot.Y))
            {
                robot.CleanCurrentSpot();
            }
            while (robot.Move(robot.X, robot.Y + 1))
            {
                robot.CleanCurrentSpot();
            }
            while (robot.Move(robot.X - 1, robot.Y))
            {
                robot.CleanCurrentSpot();
            }
            while (robot.Move(robot.X, robot.Y - 1))
            {
                robot.CleanCurrentSpot();
            }
        }
    }

    public class SpiralStrategy : IStrategy
    {
        public void Clean(Robot robot)
        {
            int startX = robot.Map.Width / 2;
            int startY = robot.Map.Height / 2;
            robot.Move(startX, startY);


            int[,] directions = new int[,]
            {
            { 1, 0 },  // Right
            { 0, 1 },  // Down
            { -1, 0 }, // Left
            { 0, -1 }  // Up
            };

            int dirIndex = 0;         // Start going Right
            int segmentLength = 1;    // Start with 1 step per direction
            int stepsTaken = 0;       // How many steps taken in the current segment
            int turns = 0;            // Count turns to know when to increase segment length

            bool canMove = true;

            // Clean the starting position first
            robot.CleanCurrentSpot();

            while (canMove)
            {
                // Move in the current direction
                int newX = robot.X + directions[dirIndex, 0];
                int newY = robot.Y + directions[dirIndex, 1];

                if (robot.Move(newX, newY))
                {
                    robot.CleanCurrentSpot();
                    stepsTaken++;
                }
                else
                {
                    // If can't move in this direction, check if all 4 directions are blocked
                    bool stuck = true;
                    for (int i = 0; i < 4; i++)
                    {
                        int testX = robot.X + directions[i, 0];
                        int testY = robot.Y + directions[i, 1];
                        if (robot.Map.IsInBounds(testX, testY) && !robot.Map.IsObstacle(testX, testY))
                        {
                            stuck = false;
                            break;
                        }
                    }

                    if (stuck)
                        break; // stop completely if surrounded
                    else
                    {
                        // If blocked only in this direction, turn to next
                        dirIndex = (dirIndex + 1) % 4;
                        stepsTaken = 0;
                        turns++;
                        if (turns % 2 == 0)
                            segmentLength++;
                        continue;
                    }
                }

                // Once we’ve taken enough steps in this direction, turn
                if (stepsTaken >= segmentLength)
                {
                    dirIndex = (dirIndex + 1) % 4; // Turn 90° clockwise
                    stepsTaken = 0;
                    turns++;

                    // After every two turns, increase the segment length
                    if (turns % 2 == 0)
                    {
                        segmentLength++;
                    }
                }
            }
        }
    }

    public class Program
    {

        public static void Main(string[] args)
        {
            Console.WriteLine("Initialize robot");


            //IStrategy some_strategy = new SomeStrategy();
            //IStrategy perim_hugger = new PerimeterHuggerStrategy();
            IStrategy outer_spiral = new SpiralStrategy();

            Map map = new Map(20, 10);
            // map.Display( 10,10);

            map.AddDirt(5, 3);
            map.AddDirt(10, 8);
            map.AddObstacle(2, 5);
            map.AddObstacle(12, 1);
            map.Display(11, 8);

            //Robot robot = new Robot(map, some_strategy);
            // Robot robot = new Robot(map, perim_hugger);
            Robot robot2 = new Robot(map, outer_spiral);

            //robot.StartCleaning();
            robot2.StartCleaning();

            Console.WriteLine("Done.");
        }
    }
}