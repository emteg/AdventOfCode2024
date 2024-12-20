using System.Diagnostics;
using System.Text;

namespace Day10;

class Program
{
    private static List<List<Cell>> grid;
    
    public static void Main()
    {
        Console.WriteLine("Hello, World!");

        Part2("1.txt");
    }

    private static void Part1(string filename)
    {
        (grid, List<Cell> possibleTrailheads) = ReadGrid(filename);
        PrintGrid();
        Console.WriteLine(ScoreTrailheads(possibleTrailheads));
    }

    private static uint ScoreTrailheads(List<Cell> possibleTrailheads)
    {
        uint scores = 0;
        foreach (Cell possibleTrailhead in possibleTrailheads)
        {
            scores += ScoreTrailhead(possibleTrailhead);
        }
        
        return scores;
    }

    private static uint ScoreTrailhead(Cell possibleTrailhead)
    {
        HashSet<Cell> openSet = [..possibleTrailhead.Neighbors()];
        HashSet<Cell> summits = [];
        HashSet<Cell> closedSet = [];

        while (openSet.Count > 0)
        {
            Cell cell = openSet.First();
            openSet.Remove(cell);
            closedSet.Add(cell);

            if (cell.IsSummit)
            {
                summits.Add(cell);
                continue;
            }

            foreach (Cell neighbor in cell.Neighbors().Where(it =>!closedSet.Contains(it)))
            {
                openSet.Add(neighbor);
            }
        }
        
        return (uint)summits.Count;
    }
    
    private static void Part2(string filename)
    {
        (grid, List<Cell> possibleTrailheads) = ReadGrid(filename);
        PrintGrid();
        Console.WriteLine(RateTrailheads(possibleTrailheads));
    }
    
    private static uint RateTrailheads(List<Cell> possibleTrailheads)
    {
        uint ratings = 0;
        foreach (Cell possibleTrailhead in possibleTrailheads)
        {
            ratings += RateTrailhead(possibleTrailhead);
        }
        
        return ratings;
    }

    private static uint RateTrailhead(Cell possibleTrailhead)
    {
        uint result = 0;

        if (possibleTrailhead.IsSummit)
            return 1;
        
        foreach (Cell neighbor in possibleTrailhead.Neighbors())
        {
            result += RateTrailhead(neighbor);
        }

        return result;
    }

    private static (List<List<Cell>>, List<Cell>) ReadGrid(string filename)
    {
        List<List<Cell>> grid = [];
        List<Cell> possibleTrailheads = [];
        using FileStream fileStream = File.OpenRead(filename);
        using StreamReader streamReader = new(fileStream, Encoding.UTF8, true);
        int y = 0;
        int x = 0;
        int cellId = 0;
        while (streamReader.ReadLine() is { } line)
        {
            List<Cell> gridLine = [];
            grid.Add(gridLine);
            foreach (char c in line)
            {
                Cell cell = new(cellId++, x, y, c == '.' ? (byte)255 : (byte)(c - 48));
                
                if (cell.IsInvalid)
                {
                    gridLine.Add(cell);
                    x++;
                    continue;
                }

                if (cell.Height == 0) 
                    possibleTrailheads.Add(cell);
                
                gridLine.Add(cell);

                Cell otherCell;
                if (x > 0)
                {
                    otherCell = gridLine[x - 1];
                    cell.ConnectWest(otherCell);
                }

                if (y > 0)
                {
                    otherCell = grid[y - 1][x];
                    cell.ConnectNorth(otherCell);
                }
                
                x++;
            }
            y++;
            x = 0;
        }

        return (grid, possibleTrailheads.Where(it => !it.HasNoConnections).ToList());
    }

    private static void PrintGrid(bool withConnections = false, 
        HashSet<Cell>? openSet = null, HashSet<Cell>? closedSet = null)
    {
        foreach (List<Cell> row in grid)
        {
            foreach (Cell cell in row)
            {
                Console.ResetColor();

                if (withConnections)
                    Console.Write(cell.CellToTheWest is null ? '|' : '-');
                
                if (cell.Height == 0)
                    Console.ForegroundColor = ConsoleColor.Yellow;
                else if (cell.Height == 9)
                    Console.ForegroundColor = ConsoleColor.Red;
                else if (closedSet is not null && closedSet.Contains(cell))
                    Console.ForegroundColor = ConsoleColor.Yellow;
                else if (openSet is not null && openSet.Contains(cell))
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                
                if ((cell.HasNoConnections && cell.Height < 9) || cell.IsInvalid)
                    Console.Write('.');
                else
                    Console.Write(cell.Height);
            }
            Console.ResetColor();

            if (withConnections)
            {
                Console.WriteLine("|");
                foreach (Cell cell in row)
                {
                    Console.Write(" ");
                    Console.Write(cell.CellToTheSouth is null ? '-' : '|');
                }
            }
            
            Console.WriteLine();
        }
    }
}

[DebuggerDisplay("Id = {Id}, X = {X}, Y = {Y}, Height = {Height}")]
class Cell
{
    public readonly int Id;
    public readonly uint X;
    public readonly uint Y;
    public readonly byte Height;
    public bool IsSummit => Height == 9;
    public bool CanBeTrailStart => Height == 0;
    public readonly List<int> SummitsReachable = [];
    public bool IsInvalid => Height == 255;
    public Cell? CellToTheNorth { get; private set; }
    public Cell? CellToTheEast { get; private set; }
    public Cell? CellToTheSouth { get; private set; }
    public Cell? CellToTheWest { get; private set; }
    public bool HasNoConnections => CellToTheNorth is null && CellToTheEast is null && CellToTheSouth is null && CellToTheWest is null;

    public IEnumerable<Cell> Neighbors()
    {
        if (CellToTheNorth is not null)
            yield return CellToTheNorth;
        if (CellToTheEast is not null)
            yield return CellToTheEast;
        if (CellToTheSouth is not null)
            yield return CellToTheSouth;
        if (CellToTheWest is not null)
            yield return CellToTheWest;
    }
    
    public Cell(int id, int x, int y, byte height) 
    {
        Id = id;
        X = (uint)x;
        Y = (uint)y;
        Height = height;
    }

    /// <summary>
    /// Connects to the given cell which is to the north of this cell. Connection is only established, if the height
    /// difference between the two cells is 1 and only in the ascending direction.
    /// </summary>
    public void ConnectNorth(Cell cellToTheNorth)
    {
        if (IsInvalid || cellToTheNorth.IsInvalid)
            return;
        
        int diff = Height - cellToTheNorth.Height;
        if (Math.Abs(diff) != 1)
            return;
        
        if (diff < 0)
            CellToTheNorth = cellToTheNorth;
        
        if (diff > 0)
            cellToTheNorth.CellToTheSouth = this;
    }

    /// <summary>
    /// Connects to the given cell which is to the west of this cell. Connection is only established, if the height
    /// difference between the two cells is 1 and only in the ascending direction.
    /// </summary>
    public void ConnectWest(Cell cellToTheWest)
    {
        if (IsInvalid || cellToTheWest.IsInvalid)
            return;
        
        int diff = Height - cellToTheWest.Height;
        if (Math.Abs(diff) != 1)
            return;
        
        if (diff < 0)
            CellToTheWest = cellToTheWest;
        if (diff > 0)
            cellToTheWest.CellToTheEast = this;
    }
}