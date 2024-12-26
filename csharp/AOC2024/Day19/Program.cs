using System.Text;

namespace Day19;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        Console.WriteLine($"Possible Designs: {Part1("sample1.txt")}");
        //Console.WriteLine($"Possible Designs: {Part1("1.txt")}");
    }

    private static uint Part1(string filename)
    {
        using FileStream fileStream = File.OpenRead(filename);
        using StreamReader streamReader = new(fileStream, Encoding.UTF8, true);

        string[] availablePatterns = streamReader.ReadLine()!.Split(", ").ToArray();
        uint possibleDesigns = 0;
        while (streamReader.ReadLine() is { } line)
        {
            if (line.Length == 0)
                continue;

            Console.WriteLine(line);
            List<string> possiblePatterns = availablePatterns
                .Where(it => line.Contains(it))
                .OrderByDescending(it => it.Length)
                .ToList();
            (string remainder, bool finished, bool successfully) = TryBuildPattern(line, possiblePatterns);
            Console.WriteLine(remainder);
            
            if (finished && successfully)
                possibleDesigns++;
        }

        return possibleDesigns;
    }

    private static (string remainder, bool finished, bool successfully) TryBuildPattern(string s, List<string> possiblePatterns)
    {
        if (s.Count(it => it == '_') == s.Length)
            return (s, true, true);
        
        if (possiblePatterns.Count == 0)
            return (s, true, false);
        
        foreach (string possiblePattern in possiblePatterns)
        {
            if (!s.Contains(possiblePattern))
                continue;
            
            (string remainder, bool finished, bool successfully) = TryBuildPattern(
                s.Replace(possiblePattern, new string('_', possiblePattern.Length)), 
                possiblePatterns.Where(it => it != possiblePattern).ToList());
            
            if (finished && successfully)
                return (remainder, true, true);
        }
        
        return (s, true, false);
    }
}

