using System.Text;

namespace Day23;

public static class Program
{
    public static void Main()
    {
        Part1("sample1.txt"); // 7
        Part1("1.txt");
    }

    private static void Part1(string filename)
    {
        Dictionary<string, HashSet<string>> computersConnectedToOtherComputers = ReadConnections(filename);
        HashSet<string> groups = FindGroupsOfThree(computersConnectedToOtherComputers);
        Console.WriteLine($"There are {groups.Count} groups of three that contain at least 1 computer starting with the letter 't'.");
    }

    private static HashSet<string> FindGroupsOfThree(Dictionary<string, HashSet<string>> computersConnectedToOtherComputers)
    {
        HashSet<string> groups = [];
        foreach (string candidateA in computersConnectedToOtherComputers.Keys)
        {
            foreach (string candidateB in computersConnectedToOtherComputers[candidateA])
            {
                foreach (string candidateC in computersConnectedToOtherComputers[candidateB])
                {
                    if (!computersConnectedToOtherComputers[candidateA].Contains(candidateC) ||
                        !computersConnectedToOtherComputers[candidateB].Contains(candidateC))
                        continue;
                    
                    if (!candidateA.StartsWith('t') && !candidateB.StartsWith('t') && !candidateC.StartsWith('t'))
                        continue;
                    
                    List<string> candidates = [candidateA, candidateB, candidateC];
                        
                    candidates.Sort();
                    groups.Add(string.Join(',', candidates));
                }
            }
        }

        return groups;
    }

    private static Dictionary<string, HashSet<string>> ReadConnections(string filename)
    {
        using FileStream fileStream = File.OpenRead(filename);
        using StreamReader streamReader = new(fileStream, Encoding.UTF8, true);

        Dictionary<string, HashSet<string>> computersConnectedToOtherComputers = [];
        while (streamReader.ReadLine() is { } line)
        {
            string[] names = line.Split('-');
            string a = names[0];
            string b = names[1];
            if (computersConnectedToOtherComputers.TryGetValue(a, out HashSet<string>? otherComputers))
                otherComputers.Add(b);
            else
                computersConnectedToOtherComputers[a] = [b];
            if (computersConnectedToOtherComputers.TryGetValue(b, out otherComputers))
                otherComputers.Add(a);
            else
                computersConnectedToOtherComputers[b] = [a];
        }

        return computersConnectedToOtherComputers;
    }
}