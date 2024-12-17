using System.Text;

namespace Day06;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        Part1("sample1.txt");
        Part1("1.txt");
    }

    private static void Part1(string filename)
    {
        Map map = ParseInput(filename);
        while (map.MoveGuard()) { }
        Console.WriteLine($"Guard has left the map after visiting {map.CellsVisitedByGuard()} cells");
    }

    private static Map ParseInput(string filename)
    {
        using FileStream fileStream = File.OpenRead(filename);
        using StreamReader streamReader = new(fileStream, Encoding.UTF8, true);
        Map map = new();
        while (streamReader.ReadLine() is { } line)
        {
            map.AddLine(line);
        }
        return map;
    }
}

class Map
{
    public void AddLine(string line)
    {
        List<Cell> cells = [];
        map.Add(cells);
        uint y = (uint)(map.Count - 1);
        uint x = 0;
        foreach (char c in line)
        {
            if (c == '.')
                cells.Add(Cell.Empty(x, y));
            else if (c == '#')
                cells.Add(Cell.Obstructed(x, y));
            else if (c is '<' or '>' or '^' or 'v')
            {
                cells.Add(Cell.OccupiedByGuard(x, y));
                guard = new Guard(x, y, c);
            }
            x++;
        }
    }

    public void Print()
    {
        for (int y = 0; y < map.Count; y++)
        {
            List<Cell> line = map[y];
            for (int x = 0; x < line.Count; x++)
            {
                Cell cell = line[x];
                if (guard.X == x && guard.Y == y)
                    Console.Write(guard.DirectionToChar());
                else
                    Console.Write(cell.IsObstructed ? '#' : cell.VisitedByGuard ? 'X' : '.');
            }
            Console.WriteLine();
        }
    }

    public bool MoveGuard()
    {
        (long newX, long newY) = guard.NextLocation();
        if (newX < 0 || newY < 0 || newX >= map[0].Count || newY >= map.Count)
            return false;

        Cell newCell = map[(int)newY][(int)newX];
        if (newCell.IsObstructed)
        {
            guard.TurnRight();
        }
        else
        {
            guard.Move();
            newCell.VisitedByGuard = true;
        }
        
        return true;
    }

    public int CellsVisitedByGuard()
    {
        return map.Sum(line => line.Count(cell => cell.VisitedByGuard));
    }
    
    private class Cell
    {
        public readonly bool IsObstructed;
        public readonly uint X;
        public readonly uint Y;
        public bool VisitedByGuard;

        public static Cell Empty(uint x, uint y) => new(x, y, false, false);
        public static Cell Obstructed(uint x, uint y) => new(x, y, true, false);
        public static Cell OccupiedByGuard(uint x, uint y) => new(x, y, false, true);

        private Cell(uint x, uint y, bool isObstructed, bool visitedByGuard)
        {
            IsObstructed = isObstructed;
            VisitedByGuard = visitedByGuard;
            X = x;
            Y = y;
        }

    }

    private enum Direction { Up, Down, Left, Right };
    private class Guard
    {
        public Direction Direction;
        public long X;
        public long Y;

        public Guard(long x, long y, char direction)
        {
            X = x;
            Y = y;
            Direction = direction switch
            {
                '<' => Direction.Left,
                '>' => Direction.Right,
                '^' => Direction.Up,
                'v' => Direction.Down,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Illegal direction!")
            };
        }

        public char DirectionToChar()
        {
            return Direction switch
            {
                Direction.Up => '^',
                Direction.Down => 'v',
                Direction.Left => '<',
                Direction.Right => '>',
                _ => throw new ArgumentOutOfRangeException(nameof(Direction), Direction, "Illegal direction!")
            };
        }

        public (long x, long y) NextLocation()
        {
            return Direction switch
            {
                Direction.Up => (X, Y - 1),
                Direction.Down => (X, Y + 1),
                Direction.Left => (X - 1, Y),
                Direction.Right => (X + 1, Y),
                _ => throw new ArgumentOutOfRangeException(nameof(Direction), Direction, "Illegal direction!")
            };
        }

        public void Move()
        {
            (X, Y) = NextLocation();
        }

        public void TurnRight()
        {
            Direction = Direction switch
            {
                Direction.Up => Direction.Right,
                Direction.Down => Direction.Left,
                Direction.Left => Direction.Up,
                Direction.Right => Direction.Down,
                _ => throw new ArgumentOutOfRangeException(nameof(Direction), Direction, "Illegal direction!")
            };
        }
    }
    
    private List<List<Cell>> map = [];
    private Guard guard;
}