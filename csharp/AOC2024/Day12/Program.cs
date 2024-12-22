using System.Text;

internal static class Program
{
    public static void Main()
    {
        Console.WriteLine("Hello, World!");
        
        Part1("1.txt");
        Part2("sample1.txt");
    }

    private static void Part1(string filename)
    {
        List<Region> regions = ReadInput(filename);
        Console.WriteLine(regions.Sum(it => it.Area() * it.Perimeter()));
    }
    
    private static void Part2(string filename)
    {
        List<Region> regions = ReadInput(filename);
        Console.WriteLine(regions.Sum(it => it.Sides()));
    }

    private static List<Region> ReadInput(string filename)
    {
        List<Region> result = [];
        
        using FileStream fileStream = File.OpenRead(filename);
        using StreamReader streamReader = new(fileStream, Encoding.UTF8, true);

        List<List<Cell>> grid = [];
        int x;
        int y = 0;
        while (streamReader.ReadLine() is { } line)
        {
            x = 0;
            List<Cell> row = [];
            grid.Add(row);
            foreach (char c in line)
            {
                Cell cell = new((uint)x, (uint)y, c);
                row.Add(cell);
                if (x > 0) 
                    cell.ConnectWest(row[x -1]);
                if (y > 0)
                    cell.ConnectNorth(grid[y - 1][x]);
                x++;
            }

            y++;
        }
        
        for (y = 0; y < grid.Count; y++)
        {
            for (x = 0; x < grid[y].Count; x++)
            {
                Cell cell = grid[y][x];
                if (cell.Region is not null)
                    continue;

                Region region = new(cell);
                region.Expand();
                result.Add(region);
            }
        }
        
        return result;
    }
}

public class Region
{
    public readonly HashSet<Cell> Cells = [];

    public Region(Cell cell)
    {
        Cells.Add(cell);
        cell.Region = this;
    }

    public void Expand()
    {
        Cells.First().JoinRegion(this);
    }
    
    public uint Area() => (uint)Cells.Count;

    public uint Perimeter()
    {
        return (uint)Cells.Sum(it => it.Perimeter());
    }

    public uint Sides()
    {
        uint minX = uint.MaxValue;
        uint maxX = uint.MinValue;
        uint minY = uint.MaxValue;
        uint maxY = uint.MinValue;
        foreach (Cell cell in Cells)
        {
            if (cell.X < minX)
                minX = cell.X;
            if (cell.X > maxX)
                maxX = cell.X;
            if (cell.Y < minY)
                minY = cell.Y;
            if (cell.Y > maxY)
                maxY = cell.Y;
        }
        return 0;
    }
}

public class Cell
{
    public readonly uint X;
    public readonly uint Y;
    public readonly char PlantType;
    
    public Cell? CellToTheNorth { get; private set; }
    public bool HasCellToTheNorth => CellToTheNorth is not null;
    public Cell? CellToTheSouth { get; private set; }
    public bool HasCellToTheSouth => CellToTheSouth is not null;
    public Cell? CellToTheEast { get; private set; }
    public bool HasCellToTheEast => CellToTheEast is not null;
    public Cell? CellToTheWest { get; private set; }
    public bool HasCellToTheWest => CellToTheWest is not null;
    public Region? Region { get; set; }

    public Cell(uint x, uint y, char plantType)
    {
        X = x;
        Y = y;
        PlantType = plantType;
    }

    public bool IsNeighborOf(Cell otherCell)
    {
        return CellToTheNorth == otherCell || 
               CellToTheSouth == otherCell || 
               CellToTheEast == otherCell || 
               CellToTheWest == otherCell;
    }

    public void JoinRegion(Region region)
    {
        Region = region;
        region.Cells.Add(this);
        foreach (Cell neighbor in Neighbors())
        {
            if (neighbor.Region is not null)
                continue;
            
            neighbor.JoinRegion(region);
        }
    }

    public void ConnectNorth(Cell cellToTheNorth)
    {
        if (cellToTheNorth.PlantType != PlantType)
            return;
        
        CellToTheNorth = cellToTheNorth;
        cellToTheNorth.CellToTheSouth = this;
    }

    public void ConnectWest(Cell cellToTheWest)
    {
        if (cellToTheWest.PlantType != PlantType)
            return;
        
        CellToTheWest = cellToTheWest;
        cellToTheWest.CellToTheEast = this;
    }

    public uint Perimeter()
    {
        return 4 - (uint)Neighbors().Count();
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
}