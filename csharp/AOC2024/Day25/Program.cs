using System.Diagnostics;
using System.Text;

namespace Day25;

static class Program
{
    public static void Main()
    {
        Console.WriteLine(Part1("sample1.txt"));
        Console.WriteLine(Part1("1.txt"));
    }

    private static uint Part1(string filename)
    {
        (List<Lock> locks, List<Key> keys) = ReadInput(filename);

        uint result = 0;
        foreach (Lock lck in locks)
        {
            foreach (Key key in keys)
            {
                if (key.FitsInto(lck))
                    result++;
            }
        }

        return result;
    }

    private static (List<Lock> locks, List<Key> keys) ReadInput(string filename)
    {
        using FileStream fileStream = File.OpenRead(filename);
        using StreamReader streamReader = new(fileStream, Encoding.UTF8, true);
        List<string> buffer = [];
        List<LockOrKey> locksOrKeys = [];
        LockOrKey lok;
        while (streamReader.ReadLine() is { } line)
        {
            if (line.Length == 0)
            {
                lok = CreateLockOrKey(buffer);
                locksOrKeys.Add(lok);
                buffer.Clear();
            }
            else
                buffer.Add(line);
        }
        lok = CreateLockOrKey(buffer);
        locksOrKeys.Add(lok);
        return (
            locksOrKeys.Where(it => it is Lock).Cast<Lock>().ToList(), 
            locksOrKeys.Where(it => it is Key).Cast<Key>().ToList()
        );
    }

    private static LockOrKey CreateLockOrKey(List<string> buffer)
    {
        byte[] heights = new byte[5];
        for (int i = 1; i < buffer.Count - 1; i++)
        {
            for (int x = 0; x < buffer[i].Length; x++)
            {
                if (buffer[i][x] == '#')
                    heights[x]++;
            }
        }
        if (buffer[0] == "#####")
        {
            Lock lck = new(heights);
            return lck;
        }

        Key key = new(heights);
        return key;
    }
}

public abstract class LockOrKey
{
    public byte[] Heights { get; }

    protected LockOrKey(byte[] heights)
    {
        Heights = heights;
    }

    public override string ToString() => string.Join(',', Heights);
}

public sealed class Lock : LockOrKey
{
    public Lock(byte[] heights) : base(heights)
    {
    }
}

public sealed class Key : LockOrKey
{
    public Key(byte[] heights) : base(heights)
    {
    }

    public bool FitsInto(Lock lck)
    {
        for (int i = 0; i < Heights.Length; i++)
        {
            int sum = Heights[i] + lck.Heights[i];
            if (sum > 5)
                return false;
        }

        return true;
    }
}