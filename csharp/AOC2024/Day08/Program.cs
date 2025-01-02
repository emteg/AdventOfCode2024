using AocUtils;

namespace Day08;

public static class Program
{
    public static void Main()
    {
        Console.WriteLine(Part1("sample1.txt")); // 14
        Console.WriteLine(Part1("1.txt"));
    }

    private static uint Part1(string filename)
    {
        (Grid2D<bool> grid, Dictionary<char, List<GridCell2D<bool>>> antennas) = ReadGrid(filename);

        FindAntinodes(antennas, grid);

        return grid.Count(it => it.Payload);
    }

    private static void FindAntinodes(Dictionary<char, List<GridCell2D<bool>>> antennas, Grid2D<bool> grid)
    {
        foreach ((_, List<GridCell2D<bool>>? antennaLocations) in antennas)
        {
            foreach (GridCell2D<bool> antennaA in antennaLocations)
            {
                foreach (GridCell2D<bool> antennaB in antennaLocations.Where(it => it != antennaA))
                {
                    // points from A to B
                    long deltaX = antennaB.X - (long)antennaA.X;
                    long deltaY = antennaB.Y - (long)antennaA.Y;
                    
                    long antinodeAx = antennaB.X + deltaX;
                    long antinodeAy = antennaB.Y + deltaY;

                    if (grid.HasCellAt(antinodeAx, antinodeAy)) 
                        grid.Get((uint)antinodeAx, (uint)antinodeAy).Payload = true;

                    // points from B to A
                    deltaX *= -1;
                    deltaY *= -1;
                    
                    long antinodeBx = antennaA.X + deltaX;
                    long antinodeBy = antennaA.Y + deltaY;

                    if (grid.HasCellAt(antinodeBx, antinodeBy)) 
                        grid.Get((uint)antinodeBx, (uint)antinodeBy).Payload = true;
                }
            }
        }
    }

    private static (Grid2D<bool> grid, Dictionary<char, List<GridCell2D<bool>>> antennas) ReadGrid(string filename)
    {
        Grid2D<bool> grid = new();
        List<GridCell2D<bool>> row = null!;
        Dictionary<char, List<GridCell2D<bool>>> antennas = [];
        foreach ((char c, uint x, uint y, bool newLine) in FileReader.ReadChars(filename))
        {
            if (newLine) 
                row = grid.AddRow();

            GridCell2D<bool> cell = new(x, y, c);
            row.Add(cell);

            if (c != '.')
            {
                if (antennas.TryGetValue(c, out List<GridCell2D<bool>>? cells))
                    cells.Add(cell);
                else
                    antennas.Add(c, [cell]);
            }
            
            if (x > 0)
                cell.ConnectLeftNeighbor(grid.Get(x - 1, y));
            if (y > 0)
                cell.ConnectUpNeighbor(grid.Get(x, y - 1));
        }

        return (grid, antennas);
    }
}