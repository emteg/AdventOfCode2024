using System.Diagnostics;
using System.Text;

namespace Day20;

class Program
{
    public static void Main()
    {
        Console.WriteLine("Hello, World!");

        //Part1("sample1.txt", 30);
        Part1("1.txt", 100);
    }

    private static void Part1(string filename, uint desiredSavings)
    {
        Day20Cell start = ReadInput(filename);
        int cheatsSavingDesiredTimeOrMore = RacePart1(start, desiredSavings);
        Console.WriteLine($"There were {cheatsSavingDesiredTimeOrMore} cheats that save at least {desiredSavings} steps.");
    }

    private static Day20Cell ReadInput(string filename)
    {
        Day20Grid grid = new();
        List<Day20Cell[]> cells = [];
        using FileStream fileStream = File.OpenRead(filename);
        using StreamReader streamReader = new(fileStream, Encoding.UTF8, true);
        uint y = 0;
        uint x = 0;
        Day20Cell start = null!;
        while (streamReader.ReadLine() is { } line)
        {
            if (y == 0)
            {
                y++;
                continue;
            }

            List<Day20Cell> row = [];
            foreach (char c in line)
            {
                if (x == 0)
                {
                    x++;
                    continue;
                }

                if (x == line.Length - 1)
                    break;

                Day20Cell cell = new(x - 1, y - 1, c);
                if (cell.IsStart)
                    start = cell;
                row.Add(cell);
                x++;
            }

            cells.Add(row.ToArray());
            x = 0;
            y++;
        }
        cells.RemoveAt(cells.Count - 1);
        grid.SetCells(cells.ToArray());
        grid.Print();
        grid.BuildGraph(start);
        return start;
    }

    private static int RacePart1(Day20Cell start, uint desiredSavings)
    {
        Racer firstRacer = new(start);
        List<Racer> racers = [firstRacer];
        List<Racer> finishedRacers = [];
        uint racerIndex = 0;
        while (racers.Count > 0)
        {
            List<Racer> newRacers = [];
            foreach (Racer racer in racers)
            {
                List<Racer> cheatingRacers = racer.MoveNext(ref racerIndex);
                if (racer.Cell.IsEnd)
                    finishedRacers.Add(racer);
                else
                    newRacers.Add(racer);
                newRacers.AddRange(cheatingRacers);
                
            }
            racers.Clear();
            racers.AddRange(newRacers);
        }

        uint bestStepsTaken = uint.MaxValue;
        uint worstStepsTaken = 0;
        Racer? bestRacer = null;
        foreach (Racer finishedRacer in finishedRacers)
        {
            if (finishedRacer.StepsTaken < bestStepsTaken)
            {
                bestStepsTaken = finishedRacer.StepsTaken;
                bestRacer = finishedRacer;
            }
            if (finishedRacer.StepsTaken > worstStepsTaken)
                worstStepsTaken = finishedRacer.StepsTaken;
        }

        Console.WriteLine($"Racer {bestRacer?.Index} finished after {bestRacer?.StepsTaken}/{worstStepsTaken} steps and was able to save {worstStepsTaken - bestStepsTaken} steps with cheating.");
        
        int cheatsSavingDesiredTimeOrMore = finishedRacers.Count(it => worstStepsTaken - it.StepsTaken >= desiredSavings);
        return cheatsSavingDesiredTimeOrMore;
    }
}

public class Grid<T> where T : Cell
{
    public T[][] Cells { get; private set; } = [];
    public uint Width { get; private set; }
    public uint Height { get; private set; }

    public void SetCells(T[][] cells)
    {
        Cells = cells;
        Height = (uint)cells.Length;
        Width = (uint)cells[0].Length;
    }

    public void Print()
    {
        PrintHorizontalHeader();
        PrintLines();
    }

    private void PrintLines()
    {
        for (uint y = 0; y < Height; y++)
        {
            Console.Write($"{y.ToString(),3} ");
            for (uint x = 0; x < Width; x++) 
                Console.Write(Cells[y][x].Char);
            Console.WriteLine();
        }
    }

    private void PrintHorizontalHeader()
    {
        for (int i = 0; i < 3; i++)
        {
            Console.Write("    ");
            for (uint x = 0; x < Width; x++)
            {
                string s = x.ToString().PadLeft(3);
                if (s.Length <= i)
                    Console.Write(' ');
                else
                    Console.Write(s[i]);
            }
            Console.WriteLine();
        }
    }
}

[DebuggerDisplay("{X}|{Y}: {Char}")]
public class Cell
{
    public readonly uint X;
    public readonly uint Y;
    public char Char { get; protected set; }

    public Cell(uint x, uint y, char c)
    {
        X = x;
        Y = y;
        Char = c;
    }
}

public class Day20Grid : Grid<Day20Cell>
{
    public void BuildGraph(Day20Cell start)
    {
        uint index = 0;
        Day20Cell? cell = start;
        while (cell is not null)
        {
            Day20Cell? north  = cell.Y > 0          ? Cells[cell.Y - 1][cell.X] : null;
            Day20Cell? east   = cell.X < Width - 1  ? Cells[cell.Y][cell.X + 1] : null;
            Day20Cell? south  = cell.Y < Height - 1 ? Cells[cell.Y + 1][cell.X] : null;
            Day20Cell? west   = cell.X > 0          ? Cells[cell.Y][cell.X - 1] : null;

            cell.RegularPathIndex = index++;
            if (north is not null && north.NextRegularCell is null && (north.Char == '.' || north.IsEnd)) 
                cell.NextRegularCell = north;
            else if (east is not null && east.NextRegularCell is null && (east.Char == '.' || east.IsEnd))
                cell.NextRegularCell = east;
            else if (south is not null && south.NextRegularCell is null && (south.Char == '.' || south.IsEnd))
                cell.NextRegularCell = south;
            else if (west is not null && west.NextRegularCell is null && (west.Char == '.' || west.IsEnd))
                cell.NextRegularCell = west;
            else if (cell.IsEnd)
            {
                // nothing to do
            }
            else
                throw new Exception("Can find next cell!");
            
            cell = cell.NextRegularCell;
        }
        
        cell = start;
        while (cell is not null && !cell.IsEnd)
        {
            Day20Cell? north  = cell.Y > 0          ? Cells[cell.Y - 1][cell.X] : null;
            Day20Cell? north2 = cell.Y > 1          ? Cells[cell.Y - 2][cell.X] : null;
            Day20Cell? east   = cell.X < Width - 1  ? Cells[cell.Y][cell.X + 1] : null;
            Day20Cell? east2  = cell.X < Width - 2  ? Cells[cell.Y][cell.X + 2] : null;
            Day20Cell? south  = cell.Y < Height - 1 ? Cells[cell.Y + 1][cell.X] : null;
            Day20Cell? south2 = cell.Y < Height - 2 ? Cells[cell.Y + 2][cell.X] : null;
            Day20Cell? west   = cell.X > 0          ? Cells[cell.Y][cell.X - 1] : null;
            Day20Cell? west2  = cell.X > 1          ? Cells[cell.Y][cell.X - 2] : null;

            if (north is not null && 
                north.IsWall && 
                north2 is not null && 
                !north2.IsWall &&
                north2.RegularPathIndex > cell.RegularPathIndex)
            {
                cell.NextCheatingCells.Add(north2);
            }
            
            if (east is not null && 
                east.IsWall && 
                east2 is not null && 
                !east2.IsWall &&
                east2.RegularPathIndex > cell.RegularPathIndex)
            {
                cell.NextCheatingCells.Add(east2);
            }
            
            if (south is not null && 
                south.IsWall && 
                south2 is not null && 
                !south2.IsWall &&
                south2.RegularPathIndex > cell.RegularPathIndex)
            {
                cell.NextCheatingCells.Add(south2);
            }
            
            if (west is not null && 
                west.IsWall && 
                west2 is not null && 
                !west2.IsWall &&
                west2.RegularPathIndex > cell.RegularPathIndex)
            {
                cell.NextCheatingCells.Add(west2);
            }
            
            cell = cell.NextRegularCell;
        }
    }
}

public class Day20Cell : Cell
{
    public bool IsStart => Char == 'S';
    public bool IsEnd => Char == 'E';
    public bool IsWall => Char == '#';
    public Day20Cell? NextRegularCell;
    public readonly List<Day20Cell> NextCheatingCells = [];
    public bool CanCheat => NextCheatingCells.Count > 0;
    public uint RegularPathIndex;
    
    public Day20Cell(uint x, uint y, char c) : base(x, y, c)
    {
        
    }
}

[DebuggerDisplay("Racer {Index} (HasCheated: {HasCheated}, StepsTaken: {StepsTaken} PathIndex: {Cell.RegularPathIndex})")]
public class Racer
{
    public Day20Cell Cell { get; private set; }
    public uint StepsTaken { get; private set; }
    public bool HasCheated { get; }
    public uint Index { get; }

    public Racer(Day20Cell cell)
    {
        Cell = cell;
    }

    private Racer(Day20Cell cell, uint stepsTaken, uint index)
    {
        Cell = cell;
        StepsTaken = stepsTaken;
        HasCheated = true;
        Index = index;
    }

    public List<Racer> MoveNext(ref uint index)
    {
        if (Cell.IsEnd)
            return [];
        
        StepsTaken++;
        
        List<Racer> result = [];
        if (Cell.CanCheat && !HasCheated)
        {
            foreach (Day20Cell nextCheatingCell in Cell.NextCheatingCells) 
                result.Add(new Racer(nextCheatingCell, StepsTaken + 1, ++index));
        }
        Cell = Cell.NextRegularCell!;
        return result;
    }
}