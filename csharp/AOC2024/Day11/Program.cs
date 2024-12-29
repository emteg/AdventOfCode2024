namespace Day11;

public static class Program
{
    public static void Main()
    {
        Console.WriteLine(Run([0, 1, 10, 99, 999], 1)); // 7
        Console.WriteLine(Run([125, 17], 6)); // 22
        Console.WriteLine(Run([125, 17], 25)); // 55312

        ulong[] puzzleInput = File.ReadAllText("1.txt").Split(' ').Select(ulong.Parse).ToArray();

        // part 1
        Console.WriteLine(Run(puzzleInput, 25));

        // part 2
        Console.WriteLine(Run(puzzleInput, 75));
    }

    private static ulong Run(ulong[] values, byte iterations)
    {
        ulong result = 0;
        Dictionary<(ulong, byte), ulong> cache = [];
        foreach (ulong value in values) 
            result += Blink(value, iterations, cache);

        return result;
    }

    private static ulong Blink(ulong value, byte iterations, Dictionary<(ulong, byte), ulong> cache)
    {
        ulong result;
        
        if (cache.ContainsKey((value, iterations)))
            return cache[(value, iterations)];
        
        if (iterations == 0)
            return 1;

        if (value == 0)
        {
            result = Blink(1, (byte)(iterations - 1), cache);
            cache.Add((value, iterations), result);
            return result;
        }

        string v = value.ToString();
        if (v.Length % 2 == 0)
        {
            string left = v[..(v.Length / 2)];
            string right = v[(v.Length / 2)..];
            ulong leftVal = ulong.Parse(left);
            ulong rightVal = ulong.Parse(right);
            result = Blink(leftVal, (byte)(iterations - 1), cache) + Blink(rightVal, (byte)(iterations - 1), cache);
            cache.Add((value, iterations), result);
            return result;
        }

        result = Blink(value * 2024, (byte)(iterations -1), cache);
        cache.Add((value, iterations), result);
        return result;
    }
}