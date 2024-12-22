using System.Text;

internal static class Program
{
    public static void Main()
    {
        Console.WriteLine("Hello, World!");
        
        Part1("1.txt");
    }

    private static void Part1(string filename)
    {
        List<List<Cell>> grid = [];
        List<Region> regions = [];
        using FileStream fileStream = File.OpenRead(filename);
        using StreamReader streamReader = new(fileStream, Encoding.UTF8, true);

        int x = 0;
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
                regions.Add(region);
            }
        }
        
        Console.WriteLine(regions.Sum(it => it.Price()));
    }
}

public class Region
{
    public readonly HashSet<Cell> Cells = [];
    public readonly char PlantType;

    public Region(Cell cell)
    {
        Cells.Add(cell);
        PlantType = cell.PlantType;
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
    
    public uint Price() => Area() * Perimeter();
}

public class Cell
{
    public readonly uint X;
    public readonly uint Y;
    public readonly char PlantType;
    
    public Cell? CellToTheNorth { get; private set; }
    public Cell? CellToTheSouth { get; private set; }
    public Cell? CellToTheEast { get; private set; }
    public Cell? CellToTheWest { get; private set; }
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