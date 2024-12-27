using System.Text;

namespace Day04;

public static class Program
{
    public static void Main()
    {
        Console.WriteLine($"Found {Part1("1.txt")} occurrences of 'XMAS' or 'SAMX'");
    }

    private static uint Part1(string filename)
    {
        using FileStream fileStream = File.OpenRead(filename);
        using StreamReader streamReader = new(fileStream, Encoding.UTF8, true);
        uint horizontalMatches = 0;
        uint verticalMatches = 0;
        uint topLeftDiagonalMatches = 0;
        uint topRightDiagonalMatches = 0;
        List<string> window = [];
        while (streamReader.ReadLine() is { } line)
        {
            window.Add(line);
            if (window.Count > 4)
                window.RemoveAt(0);
            
            horizontalMatches += CountXmas(line);
            
            if (window.Count != 4) 
                continue;
            
            foreach (string verticalLine in GetVerticalLines(window)) 
                verticalMatches += CountXmas(verticalLine);

            foreach (string topLeftDiagonalLine in GetTopLeftDiagonalLines(window)) 
                topLeftDiagonalMatches += CountXmas(topLeftDiagonalLine);
                
            foreach (string topRightDiagonalLine in GetTopRightDiagonalLines(window)) 
                topLeftDiagonalMatches += CountXmas(topRightDiagonalLine);
        }
        
        uint totalMatches = horizontalMatches + verticalMatches + topLeftDiagonalMatches + topRightDiagonalMatches;
        return totalMatches;
    }

    private static IEnumerable<string> GetTopRightDiagonalLines(List<string> lines)
    {
        for (int x = 0; x <= lines[0].Length -4; x++)
        {
            string s = new([lines[0][x + 3], lines[1][x + 2], lines[2][x + 1], lines[3][x]]);
            yield return s;
        }
    }

    private static IEnumerable<string> GetTopLeftDiagonalLines(List<string> lines)
    {
        for (int x = 0; x <= lines[0].Length -4; x++)
        {
            string s = new([lines[0][x], lines[1][x + 1], lines[2][x + 2], lines[3][x + 3]]);
            yield return s;
        }
    }

    private static IEnumerable<string> GetVerticalLines(List<string> lines)
    {
        for (int x = 0; x < lines[0].Length; x++)
        {
            string s = new([lines[0][x], lines[1][x], lines[2][x], lines[3][x]]);
            yield return s;
        }
    }

    private static uint CountXmas(string line)
    {
        uint result = 0;
        
        for (int i = 0; i <= line.Length - 4; i++)
        {
            string s = line.Substring(i, 4);
            if (s is "XMAS" or "SAMX") 
                result++;
        }

        return result;
    }
}