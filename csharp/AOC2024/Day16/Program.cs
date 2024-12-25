using System.Diagnostics;
using System.Text;

internal class Program
{
    public static void Main()
    {
        Console.WriteLine("Hello, World!");

        (List<List<Cell?>> grid, Cell start) = ReadInput("1.txt");

        Console.WriteLine(Part1(start, grid));
    }

    private static uint Part2(Cell start, List<List<Cell?>> grid)
    {
        return 0;
    }

    private static uint Part1(Cell start, List<List<Cell?>> grid)
    {
        HashSet<Cell> closedSet = [];
        HashSet<Cell> openSet = [start];
        Cell? goal = null;
        start.BestStepToGetHere = new Step(start);
        PrintGrid(grid, openSet, closedSet, goal);
        bool doPrint = false;

        while (openSet.Count > 0 && !(openSet.Count == 1 && openSet.First().IsGoal))
        {
            List<Cell> currentSet = [..openSet];
            openSet.Clear();
            
            foreach (Cell cell in currentSet)
            {
                if (!cell.IsGoal)
                    closedSet.Add(cell);

                foreach (Cell neighbor in cell.Neighbors())
                {
                    Step step = new(neighbor, cell.BestStepToGetHere!);
                    if (neighbor.IsGoal && goal is null)
                    {
                        Console.WriteLine($"The goal has been reached for the first time: {step.CostToGetHere}");
                        goal = neighbor;
                        doPrint = true;
                    }
                    if (neighbor.BestStepToGetHere is null)
                        neighbor.BestStepToGetHere = step;
                    else if (neighbor.BestStepToGetHere.CostToGetHere > step.CostToGetHere)
                    {
                        if (neighbor.IsGoal)
                        {
                            Console.WriteLine($"Found a cheaper way to get the goal: {step.CostToGetHere}");
                            doPrint = true;
                        }
                        neighbor.BestStepToGetHere = step;
                    }
                    else
                        continue;
                    openSet.Add(neighbor);
                }
            }

            if (doPrint)
            {
                PrintGrid(grid, openSet, closedSet, goal);
                doPrint = false;
            }
        }

        PrintGrid(grid, openSet, closedSet, goal);
        uint costToGetHere = goal?.BestStepToGetHere?.CostToGetHere ?? 0;
        return costToGetHere;
    }

    private static void PrintGrid(List<List<Cell?>> grid, HashSet<Cell> openSet, HashSet<Cell> closedSet, Cell? goal)
    {
        List<Cell> pathFromGoal = [];
        while (goal is not null)
        {
            pathFromGoal.Add(goal);
            goal = goal.BestStepToGetHere?.Previous?.Cell;
        }
        foreach (List<Cell?> row in grid)
        {
            foreach (Cell? cell in row)
            {
                if (cell is null)
                    Console.Write('#');
                else if (openSet.Contains(cell))
                {
                    Console.ForegroundColor = pathFromGoal.Contains(cell) ? ConsoleColor.Cyan : ConsoleColor.Yellow;
                    Console.Write('o');
                    Console.ResetColor();
                }
                else if (closedSet.Contains(cell))
                {
                    Console.ForegroundColor = pathFromGoal.Contains(cell) ? ConsoleColor.Cyan : ConsoleColor.Red;
                    Console.Write('o');
                    Console.ResetColor();
                }
                else if (cell.IsGoal)
                    Console.Write('E');
                else
                    Console.Write('.');
            }
            Console.WriteLine();
        }
    }

    private static (List<List<Cell?>> grid, Cell start) ReadInput(string filename)
    {
        Cell start = null!;
        List<List<Cell?>> grid = [];
        using FileStream fileStream = File.OpenRead(filename);
        using StreamReader streamReader = new(fileStream, Encoding.UTF8, true);
        uint x;
        uint y = 0;
        while (streamReader.ReadLine() is { } line)
        {
            x = 0;
            List<Cell?> row = [];
            grid.Add(row);
            foreach (char c in line)
            {
                Cell? cell = c == '#' 
                    ? null 
                    : new Cell(x, y) { IsGoal = c == 'E' };

                if (c == 'S')
                    start = cell!;

                if (x > 0 && cell is not null && row[(int)x - 1] is not null) 
                    cell.ConnectWest(row[(int)x - 1]!);
                
                if (y > 0 && cell is not null && grid[(int)y - 1][(int)x] is not null)
                    cell.ConnectNorth(grid[(int)y - 1][(int)x]!);

                row.Add(cell);
                x++;
            }

            y++;
        }

        return (grid, start);
    }
}

public enum Orientation
{
    North,
    South,
    East,
    West,
}

public class Step
{
    public readonly Cell Cell;
    public readonly Orientation Orientation;
    public Step? Previous;
    public readonly uint Cost;
    public uint CostToGetHere;

    public Step(Cell cell)
    {
        Cell = cell;
        Orientation = Orientation.East;
        Cost = 0;
        CostToGetHere = 0;
    }

    public Step(Cell cell, Step previous)
    {
        Cell = cell;
        Previous = previous;
        if (Previous.Cell.CellToTheEast == cell)
            Orientation = Orientation.East;
        else if (Previous.Cell.CellToTheWest == cell)
            Orientation = Orientation.West;
        else if (Previous.Cell.CellToTheNorth == cell)
            Orientation = Orientation.North;
        else if (Previous.Cell.CellToTheSouth == cell)
            Orientation = Orientation.South;
        else
            throw new InvalidOperationException("Cell is not a neighbor of previous cell!");

        Cost = 1 + (Orientation == Previous.Orientation ? 0 : 1000u);

        CostToGetHere = GetCostToGetHere(Cost, previous);
    }

    public static uint GetCostToGetHere(uint cost, Step previous)
    {
        uint result = cost;

        Step? prev = previous;
        while (prev is not null)
        {
            result += prev.Cost;
            prev = prev.Previous;
        }
        
        return result;
    }
}

[DebuggerDisplay("X: {X}, Y: {Y}, Goal: {IsGoal}")]
public class Cell
{
    public readonly uint X;
    public readonly uint Y;
    public bool IsGoal { get; init; }
    
    public Cell? CellToTheNorth { get; private set; }
    public Cell? CellToTheSouth { get; private set; }
    public Cell? CellToTheEast { get; private set; }
    public Cell? CellToTheWest { get; private set; }

    public Step? BestStepToGetHere;

    public Cell(uint x, uint y)
    {
        X = x;
        Y = y;
    }

    public IEnumerable<Cell> Neighbors()
    {
        if (CellToTheNorth is not null)
            yield return CellToTheNorth;
        if (CellToTheSouth is not null)
            yield return CellToTheSouth;
        if (CellToTheEast is not null)
            yield return CellToTheEast;
        if (CellToTheWest is not null)
            yield return CellToTheWest;
    }

    public void ConnectNorth(Cell cellToTheNorth)
    {
        CellToTheNorth = cellToTheNorth;
        cellToTheNorth.CellToTheSouth = this;
    }

    public void ConnectWest(Cell cellToTheWest)
    {
        CellToTheWest = cellToTheWest;
        cellToTheWest.CellToTheEast = this;
    }
}