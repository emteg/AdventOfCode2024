using System.Text;

namespace Day05;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        Part1("sample1.txt");
        Part1("1.txt");

        Part2("sample1.txt");
        Part2("1.txt");
    }

    private static void Part1(string filename)
    {
        long result = ParseInput(filename)
            .UpdatesInTheRightOrder()
            .Select(it => it[MiddleOf(it)])
            .Sum(it => it);
        Console.WriteLine($"Result of Part 1 for {filename}: {result}");
    }

    private static void Part2(string filename)
    {
        Update update = ParseInput(filename);
        long result = update
            .UpdatesNotInTheRightOrder()
            .Select(it => update.Fix(it))
            .Select(it => it[MiddleOf(it)])
            .Sum(it => it);
        Console.WriteLine($"Result of Part 2 for {filename}: {result}");
    }
    
    private static int MiddleOf(List<uint> list)
    {
        return (int)Math.Floor(list.Count / 2.0);
    }

    private static Update ParseInput(string filename)
    {
        using FileStream fileStream = File.OpenRead(filename);
        using StreamReader streamReader = new(fileStream, Encoding.UTF8, true);
        Update update = new();
        Action<string> action = update.ParsePageOrderingRule;
        while(streamReader.ReadLine() is { } line)
        {
            if (line.Length == 0)
            {
                action = update.ParsePagesToProduce;
                continue;
            }
            
            action(line);
        }
        
        return update;
    }
}

class Update
{
    // list of pages that must come after page <key>
    private readonly Dictionary<uint, List<uint>> pageOrderingRules = [];
    private readonly List<List<uint>> pagesToProduce = [];

    public void ParsePageOrderingRule(string line)
    {
        uint[] items = line.Split("|").Select(uint.Parse).ToArray();
        if (pageOrderingRules.ContainsKey(items[0]))
            pageOrderingRules[items[0]].Add(items[1]);
        else
            pageOrderingRules.Add(items[0], [items[1]]);
    }

    public void ParsePagesToProduce(string line)
    {
        uint[] items = line.Split(",").Select(uint.Parse).ToArray();
        pagesToProduce.Add([..items]);
    }

    public IEnumerable<List<uint>> UpdatesInTheRightOrder()
    {
        return pagesToProduce.Where(UpdateIsInRightOrder);
    }

    public IEnumerable<List<uint>> UpdatesNotInTheRightOrder()
    {
        return pagesToProduce.Where(it => !UpdateIsInRightOrder(it));
    }

    private bool UpdateIsInRightOrder(List<uint> pagesToUpdate)
    {
        for (int i = 0; i < pagesToUpdate.Count; i++)
        {
            uint currentPage = pagesToUpdate[i];
            uint? nextPage = i < pagesToUpdate.Count - 1 ? pagesToUpdate[i + 1] : null;

            if (nextPage is not null && PairIsNotInRightOrder(currentPage, nextPage.Value))
            {
                return false;
            }
        }

        return true;
    }

    public List<uint> Fix(List<uint> pagesToUpdate)
    {
        while (true)
        {
            bool swapped = false;
            for (int i = 0; i < pagesToUpdate.Count; i++)
            {
                uint currentPage = pagesToUpdate[i];
                uint? nextPage = i < pagesToUpdate.Count - 1 ? pagesToUpdate[i + 1] : null;
                if (nextPage is not null && PairIsNotInRightOrder(currentPage, nextPage.Value))
                {
                    pagesToUpdate[i] = nextPage.Value;
                    pagesToUpdate[i + 1] = currentPage;
                    swapped = true;
                }
            }

            if (!swapped)
                break;
        }
        
        return pagesToUpdate;
    }

    private bool PairIsNotInRightOrder(uint currentPage, uint nextPage)
    {
        return pageOrderingRules.TryGetValue(nextPage, out List<uint>? pagesThatMustComeAfter) && pagesThatMustComeAfter.Contains(currentPage);
    }
}