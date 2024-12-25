using System.Diagnostics;
using System.Text;

namespace Day18;

static class Program
{
    public static void Main()
    {
        Console.WriteLine("Hello, World!");

        //Cell goal = Part1(7, "sample1.txt", 12);
        //Console.WriteLine(goal.PathLength);
        //
        //goal = Part1(71, "1.txt", 1024);
        //Console.WriteLine(goal.PathLength);

        (int linesRead, int[] lastByte) = Part2(71, "1.txt", 1024);
        Console.WriteLine($"Found no path after reading {linesRead} bytes (byte at {lastByte[0]}, {lastByte[1]})");
    }

    private static (int linesRead, int[] lastByte) Part2(uint size, string filename, int minNumberOfLinesToRead)
    {
        int numberOfLinesToRead = minNumberOfLinesToRead;
        int[] lastByte = [];
        uint lastPathLength = 0;
        while (true)
        {
            (List<List<Cell>> grid, lastByte) = CorruptGrid(size, filename, numberOfLinesToRead);
            Cell start = grid[0][0];
            start.PathLength = 0;
            Cell goal = grid[(int)size - 1][(int)size - 1];
            
            FindPath(start);

            if (goal.PathLength > lastPathLength)
            {
                PrintGrid(grid, goal);
                lastPathLength = goal.PathLength;
                Console.WriteLine($"Path to goal has increased to length {goal.PathLength}");
            }
            
            if (goal.Previous is null)
                break;

            numberOfLinesToRead++;
        }

        return (numberOfLinesToRead, lastByte);
    }

    private static Cell Part1(uint size, string filename, int numberOfLinesToRead)
    {
        (List<List<Cell>> grid, _) = CorruptGrid(size, filename, numberOfLinesToRead);
        Cell start = grid[0][0];
        start.PathLength = 0;
        Cell goal = grid[(int)size - 1][(int)size - 1];
        FindPath(start);
        PrintGrid(grid, goal);
        return goal;
    }

    private static void FindPath(Cell start)
    {
        HashSet<Cell> openSet = [start];
        HashSet<Cell> closedSet = [];
        while (openSet.Count > 0)
        {
            List<Cell> currentSet = openSet.ToList();
            openSet.Clear();
            foreach (Cell cell in currentSet)
            {
                closedSet.Add(cell);
                foreach (Cell neighbor in cell.UncorruptedNeighbors().Where(it => !closedSet.Contains(it)))
                {
                    openSet.Add(neighbor);
                    uint newPathLength = cell.PathLength + 1;
                    if (newPathLength < neighbor.PathLength)
                        neighbor.SetPrevious(cell);
                }
            }
        }
    }

    private static (List<List<Cell>>, int[] lastByte) CorruptGrid(uint size, string filename, int numberOfLinesToRead)
    {
        List<List<Cell>> grid = Cell.CreateGrid(size);
        using FileStream fileStream = File.OpenRead(filename);
        using StreamReader streamReader = new(fileStream, Encoding.UTF8, true);
        int linesRead = 0;
        int[] currentByte = [];
        while (streamReader.ReadLine() is { } line)
        {
            currentByte = line.Split(',').Select(int.Parse).ToArray();
            (int x, int y) = (currentByte[0], currentByte[1]);
            grid[y][x].IsCorrupt = true;
            linesRead++;
            if (linesRead >= numberOfLinesToRead)
                break;
        }

        return (grid, currentByte);
    }

    private static void PrintGrid(List<List<Cell>> grid, Cell goal)
    {
        if (goal.Previous is null)
            return;
        
        List<Cell> shortestPath = [];
        if (goal.Previous is not null)
        {
            Cell? c = goal;
            while (c is not null)
            {
                shortestPath.Add(c);
                c = c.Previous;
            }
        }
        foreach (List<Cell> row in grid)
        {
            for (int x = 0; x < grid.Count; x++)
            {
                if (shortestPath.Contains(row[x]))
                    Console.ForegroundColor = ConsoleColor.Red;
                else
                    Console.ResetColor();
                Console.Write(row[x].IsCorrupt ? '#' : shortestPath.Contains(row[x]) ? 'O' : '.');
            }
            Console.ResetColor();
            Console.WriteLine();
        }
    }
}

[DebuggerDisplay("{X}|{Y}: {PathLength}")]
public class Cell
{
    public readonly uint X;
    public readonly uint Y;
    public bool IsCorrupt = false;

    public Cell? Previous { get; private set; }
    public uint PathLength { get; set; } = uint.MaxValue;

    public Cell? RightNeighbor { get; private set; }
    public Cell? LeftNeighbor { get; }
    public Cell? DownNeighbor { get; private set; }
    public Cell? UpNeighbor { get; private set; }

    public Cell(uint x, uint y)
    {
        X = x;
        Y = y;
    }

    public Cell(uint x, uint y, Cell leftNeighbor) : this(x, y)
    {
        LeftNeighbor = leftNeighbor;
        leftNeighbor.RightNeighbor = this;
    }

    public Cell(uint x, uint y, Cell leftNeighbor, Cell upNeighbor) : this(x, y, leftNeighbor)
    {
        UpNeighbor = upNeighbor;
        upNeighbor.DownNeighbor = this;
    }

    public IEnumerable<Cell> Neighbors()
    {
        if (RightNeighbor is not null)
            yield return RightNeighbor;
        if (LeftNeighbor is not null)
            yield return LeftNeighbor;
        if (DownNeighbor is not null)
            yield return DownNeighbor;
        if (UpNeighbor is not null)
            yield return UpNeighbor;
    }
    
    public IEnumerable<Cell> UncorruptedNeighbors() => Neighbors().Where(cell => !cell.IsCorrupt);

    public void SetPrevious(Cell previous)
    {
        Previous = previous;
        Cell? c = previous;
        PathLength = 0;
        while (c is not null)
        {
            PathLength++;
            c = c.Previous;
        }
    }

    public static List<List<Cell>> CreateGrid(uint size)
    {
        List<List<Cell>> grid = [];

        for (uint y = 0; y < size; y++)
        {
            List<Cell> row = [];
            grid.Add(row);
            for (uint x = 0; x < size; x++)
            {
                if (x == 0 && y == 0)
                    row.Add(new Cell(x, y));
                else if (x > 0 && y == 0)
                    row.Add(new Cell(x, y, row[(int)x - 1]));
                else if (x == 0 && y > 0)
                {
                    Cell cell = new(x, y);
                    row.Add(cell);
                    cell.UpNeighbor = grid[(int)y - 1][(int)x];
                    cell.UpNeighbor.DownNeighbor = cell;
                }
                else // x > 0, y > 0
                    row.Add(new Cell(x, y, row[(int)x - 1], grid[(int)y - 1][(int)x]));
            }
        }

        return grid;
    }
}