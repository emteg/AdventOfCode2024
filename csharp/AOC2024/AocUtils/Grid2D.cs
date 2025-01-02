namespace AocUtils;

public class Grid2D<T>
{
    public List<List<GridCell2D<T>>> Cells { get; } = [];
    public uint Height => (uint)Cells.Count;
    public uint Width => Cells.Count > 0 ? (uint)Cells[0].Count : 0;

    public bool HasCellAt(uint x, uint y)
    {
        try
        {
            _ = Cells[(int)y][(int)x];
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    public bool HasCellAt(int x, int y)
    {
        try
        {
            _ = Cells[y][x];
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    public bool HasCellAt(long x, long y)
    {
        if (x < int.MinValue || x > int.MaxValue || y < int.MinValue || y > int.MaxValue)
            throw new ArgumentOutOfRangeException();
        
        return HasCellAt((int)x, (int)y);
    }
    
    public GridCell2D<T> Get(uint x, uint y)
    {
        if (!HasCellAt(x, y))
            throw new InvalidOperationException($"No cell found at location '{x}'/'{y}'");
        
        return Cells[(int)y][(int)x];
    }
    
    public GridCell2D<T> Get(int x, int y)
    {
        if (!HasCellAt(x, y))
            throw new InvalidOperationException($"No cell found at location '{x}'/'{y}'");
        
        return Cells[y][x];
    }
    
    public GridCell2D<T> Get(long x, long y)
    {
        if (!HasCellAt(x, y))
            throw new InvalidOperationException($"No cell found at location '{x}'/'{y}'");
        
        return Cells[(int)y][(int)x];

    }

    public List<GridCell2D<T>> AddRow()
    {
        List<GridCell2D<T>> row = [];
        Cells.Add(row);
        return row;
    }

    public uint Count(Func<GridCell2D<T>, bool> predicate)
    {
        uint result = 0;
        foreach (List<GridCell2D<T>> row in Cells)
        {
            result += (uint)row.Count(predicate);
        }
        return result;
    }
}