using System.Diagnostics;
using System.Text;

namespace Day14;

public static class Program
{
    public static void Main()
    {
        Console.WriteLine(Part1("1.txt", 101, 103));
        Console.WriteLine(Part2("1.txt", 101, 103));
    }

    private static uint Part1(string filename, uint width, uint height)
    {
        List<List<Cell>> grid = CreateGrid(width, height);
        List<Robot> robots = PlaceRobots(filename, grid);
        UpdateRobots(robots);
        PrintGrid(grid);
        return SafetyFactor(grid);
    }

    private static uint Part2(string filename, uint width, uint height)
    {
        List<List<Cell>> grid = CreateGrid(width, height);
        List<Robot> robots = PlaceRobots(filename, grid);
        uint iterations = 0;
        while (true)
        {
            foreach (Robot robot in robots) 
                robot.Update();
            
            iterations++;
            if (iterations % 1000 == 0)
                Console.WriteLine(iterations);
            
            string s = GridToString(grid);
            if (s.Contains("#####################"))
            {
                PrintGrid(grid);
                Console.WriteLine($"Iterations: {iterations}");
                break;
            }
        }
        
        return iterations;
    }
    
    private static List<List<Cell>> CreateGrid(uint width, uint height)
    {
        List<List<Cell>> grid = [];

        for (uint y = 0; y < height; y++)
        {
            List<Cell> row = [];
            grid.Add(row);
            for (uint x = 0; x < width; x++)
            {
                Cell cell = new(x, y);
                row.Add(cell);
                if (x > 0)
                {
                    Cell left = row[(int)x - 1];
                    cell.ConnectLeft(left);
                }

                if (x == width - 1)
                {
                    Cell rowStart = row[0];
                    cell.ConnectRight(rowStart);
                }

                if (y > 0)
                {
                    Cell up = grid[(int)y - 1][(int)x];
                    cell.ConnectUp(up);
                }

                if (y == height - 1)
                {
                    Cell colStart = grid[0][(int)x];
                    cell.ConnectDown(colStart);
                }
            }
        }
        
        return grid;
    }

    private static List<Robot> PlaceRobots(string filename, List<List<Cell>> grid)
    {
        List<Robot> robots = [];
        using FileStream fileStream = File.OpenRead(filename);
        using StreamReader streamReader = new(fileStream, Encoding.UTF8, true);
        while (streamReader.ReadLine() is { } line)
        {
            string[] properties = line.Split(' ');
            short[] position = properties[0].Substring(2).Split(',').Select(short.Parse).ToArray();
            short[] speed = properties[1].Substring(2).Split(',').Select(short.Parse).ToArray();
            
            Cell cell = grid[position[1]][position[0]];
            Robot robot = new(cell, speed[0], speed[1]);
            robots.Add(robot);
        }

        return robots;
    }

    private static void UpdateRobots(List<Robot> robots)
    {
        for (int i = 0; i < 100; i++)
        {
            foreach (Robot robot in robots) 
                robot.Update();
        }
    }

    private static void PrintGrid(List<List<Cell>> grid)
    {
        foreach (List<Cell> row in grid)
        {
            foreach (Cell cell in row) 
                Console.Write(cell);
            Console.WriteLine();
        }
    }

    private static string GridToString(List<List<Cell>> grid)
    {
        StringBuilder builder = new();
        foreach (List<Cell> row in grid)
        {
            foreach (Cell cell in row) 
                builder.Append(cell.Robots.Count > 0 ? '#' : '.');
            builder.AppendLine();
        }
        return builder.ToString();
    }

    private static uint SafetyFactor(List<List<Cell>> grid)
    {
        uint result = 1;
        
        result *= CountRobots(grid, 0, grid.Count / 2, 0, grid[0].Count / 2);
        result *= CountRobots(grid, grid.Count / 2 + 1, grid.Count, 0, grid[0].Count / 2);
        result *= CountRobots(grid, 0, grid.Count / 2, grid[0].Count / 2 + 1, grid[0].Count);
        result *= CountRobots(grid, grid.Count / 2 + 1, grid.Count, grid[0].Count / 2 + 1, grid[0].Count);
        
        return result;
    }

    private static uint CountRobots(List<List<Cell>> grid, int yMin, int yMax, int xMin, int xMax)
    {
        uint result = 0;
        
        for (int y = yMin; y < yMax; y++)
            for (int x = xMin; x < xMax; x++) 
                result += (uint)grid[y][x].Robots.Count;

        return result;
    }
}

[DebuggerDisplay("p={Cell.X},{Cell.Y} v={DeltaX},{DeltaY}")]
public sealed class Robot
{
    public readonly short DeltaX;
    public readonly short DeltaY;
    public Cell Cell { get; private set; }

    public Robot(Cell cell, short deltaX, short deltaY)
    {
        Cell = cell;
        DeltaX = deltaX;
        DeltaY = deltaY;
        cell.AddRobot(this);
    }

    public void Update()
    {
        short toDo = DeltaX;
        while (toDo > 0)
        {
            MoveTo(Cell.Right);
            toDo--;
        }

        while (toDo < 0)
        {
            MoveTo(Cell.Left);
            toDo++;
        }

        toDo = DeltaY;
        while (toDo > 0)
        {
            MoveTo(Cell.Down);
            toDo--;
        }

        while (toDo < 0)
        {
            MoveTo(Cell.Up);
            toDo++;
        }
    }

    private void MoveTo(Cell cell)
    {
        Cell.RemoveRobot(this);
        Cell = cell;
        Cell.AddRobot(this);
    }
}

[DebuggerDisplay("{X},{Y}: {Robots.Count}")]
public sealed class Cell
{
    public readonly uint X;
    public readonly uint Y;
    public Cell Right { get; private set; } = null!;
    public Cell Left { get; private set; } = null!;
    public Cell Up { get; private set; } = null!;
    public Cell Down { get; private set; } = null!;
    public readonly List<Robot> Robots = [];

    public Cell(uint x, uint y)
    {
        X = x;
        Y = y;
    }

    public void ConnectLeft(Cell left)
    {
        Left = left;
        left.Right = this;
    }

    public void ConnectUp(Cell up)
    {
        Up = up;
        up.Down = this;
    }

    public void ConnectRight(Cell right)
    {
        Right = right;
        right.Left = this;
    }

    public void ConnectDown(Cell down)
    {
        Down = down;
        down.Up = this;
    }

    public void AddRobot(Robot robot) => Robots.Add(robot);

    public void RemoveRobot(Robot robot) => Robots.Remove(robot);

    public override string ToString()
    {
        return Robots.Count == 0 
            ? "." 
            : Robots.Count.ToString();
    }
}