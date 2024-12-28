using System.Diagnostics;
using System.Text;

namespace Day15;

public static class Program
{
    public static void Main()
    {
        Console.WriteLine(Part1("sample0.txt")); // 2028
        Console.WriteLine(Part1("sample1.txt")); // 10092
        Console.WriteLine(Part1("1.txt"));
    }

    private static uint Part1(string filename)
    {
        List<List<Cell>> grid = [];
        Cell? robotCell = null;
        using FileStream fileStream = File.OpenRead(filename);
        using StreamReader streamReader = new(fileStream, Encoding.UTF8, true);
        bool readsGrid = true;
        while (streamReader.ReadLine() is { } line)
        {
            if (readsGrid && line.Length > 0)
            {
                Cell? maybeRobotCell = ReadGridLine(line, grid);
                if (robotCell is null && maybeRobotCell is not null)
                    robotCell = maybeRobotCell;
            }
            else if (readsGrid && line.Length == 0)
            {
                PrintGrid(grid);
                readsGrid = false;
            }
            else
                robotCell = ReadMovesLine(line, grid, robotCell!);
        }

        PrintGrid(grid);
        uint result = (uint)grid.SelectMany(it => it).Select(cell => cell.GpsCoordinate()).Sum(it => it);
        return result;
    }

    private static Cell? ReadGridLine(string line, List<List<Cell>> grid)
    {
        List<Cell> row = [];
        grid.Add(row);
        
        Cell? robotCell = null;

        for (int x = 0; x < line.Length; x++)
        {
            Cell cell = new(line[x], (uint)x, (uint)grid.Count - 1);
            row.Add(cell);
            
            if (cell.ContainsRobot)
                robotCell = cell;
            
            if (x > 0) 
                cell.ConnectLeft(row[x - 1]);

            if (grid.Count > 1) 
                cell.ConnectUp(grid[^2][x]);
        }
        
        return robotCell;
    }

    private static Cell ReadMovesLine(string line, List<List<Cell>> grid, Cell robotCell)
    {
        foreach (char c in line)
        {
            robotCell = c switch
            {
                '<' => robotCell.MoveLeft(),
                '^' => robotCell.MoveUp(),
                '>' => robotCell.MoveRight(),
                'v' => robotCell.MoveDown(),
                _ => throw new InvalidOperationException($"Unexpected character: {c}"),
            };
        }
        return robotCell;
    }

    private static void PrintGrid(List<List<Cell>> grid)
    {
        foreach (List<Cell> row in grid)
        {
            foreach (Cell cell in row) 
                Console.Write(cell.Value);
            Console.WriteLine();
        }
    }
}

[DebuggerDisplay("{Value}")]
public sealed class Cell
{
    public readonly uint X;
    public readonly uint Y;
    public char Value { get; set; }
    public bool IsEmpty => Value == '.';
    public bool IsWall => Value == '#';
    public bool ContainsBox => Value == 'O';
    public bool ContainsRobot => Value == '@';

    public Cell? Right { get; private set; }
    public Cell? Left { get; private set; }
    public Cell? Down { get; private set; }
    public Cell? Up { get; private set; }
    
    public Cell(char value, uint x, uint y)
    {
        Value = value;
        X = x;
        Y = y;
    }

    public void ConnectUp(Cell up)
    {
        Up = up;
        up.Down = this;
    }

    public void ConnectLeft(Cell left)
    {
        Left = left;
        left.Right = this;
    }

    public uint GpsCoordinate()
    {
        if (!ContainsBox)
            return 0;

        return 100 * Y + X;
    }

    public Cell MoveLeft() => Move(Left, Direction.Left);

    public Cell MoveRight() => Move(Right, Direction.Right);

    public Cell MoveUp() => Move(Up, Direction.Up);
    
    public Cell MoveDown() => Move(Down, Direction.Down);

    private Cell Move(Cell? desiredCell, Direction direction)
    {
        if (desiredCell is null)
            return this;

        if (desiredCell.IsWall)
            return this;
        
        if (!ContainsRobot)
            return this;

        if (desiredCell.ContainsBox && desiredCell.GetNeighbor(direction)?.IsEmpty is true)
        {
            desiredCell.GetNeighbor(direction)!.TakeBox();
            desiredCell.Clear();
        }
        else if (desiredCell.ContainsBox)
        {
            List<Cell> neighbors = [];
            Cell? nextNeighbor = desiredCell.GetNeighbor(direction);
            while (nextNeighbor is not null)
            {
                if (nextNeighbor.ContainsBox)
                {
                    neighbors.Add(nextNeighbor);
                    nextNeighbor = nextNeighbor.GetNeighbor(direction);
                    continue;
                }
                
                if (nextNeighbor.IsWall)
                    return this;

                if (nextNeighbor.IsEmpty)
                {
                    neighbors.Add(nextNeighbor);
                    break;
                }
            }
            
            for (int i = neighbors.Count - 1; i >= 0; i--)
            {
                Cell neighbor = neighbors[i];
                neighbor.TakeBox();
                if (i > 0)
                    neighbors[i - 1].Clear();
            }
            desiredCell.Clear();
        }
        
        desiredCell.TakeRobot();
        Clear();
        return desiredCell;
    }

    private Cell? GetNeighbor(Direction direction)
    {
        return direction switch
        {
            Direction.Down => Down,
            Direction.Up => Up,
            Direction.Left => Left,
            Direction.Right => Right,
            _ => throw new InvalidOperationException($"Unexpected direction: {direction}"),
        };
    }

    private void TakeRobot()
    {
        if (Value != '.')
            throw new InvalidOperationException($"Can't take robot when cell value is {Value}");
        Value = '@';
    }

    private void TakeBox()
    {
        if (Value != '.')
            throw new InvalidOperationException($"Can't take box when cell value is {Value}");
        Value = 'O';
    }

    private void Clear()
    {
        Value = '.';
    }

    private enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
}