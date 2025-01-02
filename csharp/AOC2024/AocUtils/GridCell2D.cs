namespace AocUtils;

/// <summary>
/// A cell in a 2D grid with references to its grid neighbors.
/// </summary>
public class GridCell2D<T>
{
    public char Char { get; set; }
    public T? Payload { get; set; }
    public uint X { get; }
    public uint Y { get; }
    public GridCell2D<T>? RightNeighbor { get; protected set; }
    public GridCell2D<T>? LeftNeighbor { get; protected set; }
    public GridCell2D<T>? UpNeighbor { get; protected set; }
    public GridCell2D<T>? DownNeighbor { get; protected set; }

    public GridCell2D(uint x, uint y, char c, T? payload = default)
    {
        Char = c;
        X = x;
        Y = y;
        Payload = payload;
    }

    public GridCell2D<T> ConnectLeftNeighbor(GridCell2D<T> leftNeighbor)
    {
        LeftNeighbor = leftNeighbor;
        leftNeighbor.RightNeighbor = this;
        return this;
    }

    public GridCell2D<T> ConnectUpNeighbor(GridCell2D<T> upNeighbor)
    {
        UpNeighbor = upNeighbor;
        upNeighbor.DownNeighbor = this;
        return this;
    }

    public override string ToString()
    {
        return Char.ToString();
    }
}