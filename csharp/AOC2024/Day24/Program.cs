namespace Day24;

public static class Program
{
    public static void Main()
    {
        Console.WriteLine(Part1("sample1.txt")); // 4
        Console.WriteLine(Part1("sample2.txt")); // 2024
        Console.WriteLine(Part1("1.txt"));
    }

    private static long Part1(string filename)
    {
        (Dictionary<string, bool> wires, List<Operation> operations) = ReadInputs(filename);
        ApplyOperations(operations, wires);
        long result = CalculateResult(wires);
        return result;
    }

    private static long CalculateResult(Dictionary<string, bool> wires)
    {
        string[] outputWires = wires
            .Where(pair => pair.Key.StartsWith('z'))
            .Select(pair => pair.Key)
            .Order()
            .ToArray();

        long result = 0;
        
        for (int i = 0; i < outputWires.Length; i++)
        {
            string outputWire = outputWires[i];
            long outputValue = wires[outputWire] ? 1 : 0;
            result |= outputValue << i;
        }

        return result;
    }

    private static void ApplyOperations(List<Operation> operations, Dictionary<string, bool> wires)
    {
        while (operations.Count > 0)
        {
            for (int i = operations.Count - 1; i >= 0; i--)
            {
                Operation operation = operations[i];
                if (!wires.ContainsKey(operation.WireA) || !wires.ContainsKey(operation.WireB))
                    continue;

                operation.Apply(wires);
                operations.RemoveAt(i);
            }
        }
    }

    private static (Dictionary<string, bool> wires, List<Operation> operations) ReadInputs(string filename)
    {
        Dictionary<string, bool> wires = [];
        List<Operation> operations = [];
        bool readInputs = true;
        foreach (string line in AocUtils.FileReader.ReadLines(filename))
        {
            string[] parts;
            
            if (line.Length == 0)
            {
                readInputs = false;
                continue;
            }

            if (readInputs)
            {
                parts = line.Split(": ");
                string name = parts[0];
                bool value = byte.Parse(parts[1]) != 0;
            
                wires.Add(name, value);
                
                continue;
            }

            parts = line.Split(" ");
            string wireA = parts[0];
            string operation = parts[1];
            string wireB = parts[2];
            // skipping parts[3]: '->'
            string targetWire = parts[4];
            Operation op = new(wireA, wireB, targetWire, OperationNameToFunc(operation));
            operations.Add(op);
        }

        return (wires, operations);
    }

    private static Func<bool, bool, bool> OperationNameToFunc(string operation)
    {
        return operation switch
        {
            "AND" => (a, b) => a && b,
            "OR" => (a, b) => a || b,
            "XOR" => (a, b) => a ^ b,
            _ => throw new InvalidOperationException($"Unknown operation: {operation}")
        };
    }

    private readonly struct Operation
    {
        public readonly string WireA;
        public readonly string WireB;
        public readonly string TargetWire;
        public readonly Func<bool, bool, bool> Op;

        public Operation(string wireA, string wireB, string targetWire, Func<bool, bool, bool> op)
        {
            WireA = wireA;
            WireB = wireB;
            TargetWire = targetWire;
            Op = op;
        }

        public void Apply(Dictionary<string, bool> wires)
        {
            wires.Add(TargetWire, Op(wires[WireA], wires[WireB]));
        }
    }
}