using System.Text;

namespace Day07;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        //Console.WriteLine(Part1("sample1.txt"));
        //Console.WriteLine(Part1("1.txt"));
        Console.WriteLine(Part2("sample1.txt"));
        Console.WriteLine(Part2("1.txt"));
    }

    private static ulong Part1(string filename)
    {
        return Execute(filename, [Operation.Add, Operation.Multiply]);
    }

    private static ulong Part2(string filename)
    {
        return Execute(filename, [Operation.Add, Operation.Multiply, Operation.Concatenate]);
    }

    private static ulong Execute(string filename, Operation[] operations)
    {
        using FileStream fileStream = File.OpenRead(filename);
        using StreamReader streamReader = new(fileStream, Encoding.UTF8, true);
        ulong sumOfValidEquations = 0;
        while (streamReader.ReadLine() is { } line)
        {
            string[] goalAndOperands = line.Split(": ");
            ulong goal = ulong.Parse(goalAndOperands[0]);
            IEnumerable<ulong> operands = goalAndOperands[1].Split(" ").Select(ulong.Parse);
            if (CanBeTrue(goal, new Queue<ulong>(operands), operations))
            {
                sumOfValidEquations += goal;
            }
        }

        return sumOfValidEquations;
    }

    static bool CanBeTrue(ulong goal, Queue<ulong> operands, Operation[] operations)
    {
        Node? node = new(operands.Dequeue(), goal);
        PriorityQueue<Node, ulong> priorityQueue = new();
        List<Node> newNodes = [];
        priorityQueue.Enqueue(node, node.DiffToGoal);
        while (operands.TryDequeue(out ulong nextOperand))
        {
            while (priorityQueue.TryDequeue(out node, out _))
            {
                foreach (Operation operation in operations)
                {
                    Node newNode = new(node, nextOperand, operation);
                    if (operands.Count == 0 && newNode.HasReachedGoal)
                        return true;
                    if (newNode.CanReachGoal)
                        newNodes.Add(newNode);
                }
            }

            if (newNodes.Count == 0)
                break;
            
            foreach (Node newNode in newNodes) 
                priorityQueue.Enqueue(newNode, newNode.DiffToGoal);
            newNodes.Clear();
        }

        return false;
    }
}

public enum Operation
{
    None,
    Add,
    Multiply,
    Concatenate,
}

class Node
{
    public readonly ulong Goal;
    public readonly Node? Previous;
    public bool HasPrevious => Previous is not null;
    public readonly ulong Value;
    public readonly Operation Operation = Operation.None;
    public readonly ulong Result;
    public bool HasReachedGoal => Result == Goal;
    public bool CanReachGoal => Result <= Goal;
    public ulong DiffToGoal => Goal - Result;

    public Node(ulong value, ulong goal)
    {
        Goal = goal;
        Value = value;
        Result = Value;
    }

    public Node(Node previous, ulong value, Operation operation)
    {
        Previous = previous;
        Value = value;
        Goal = previous.Goal;
        Operation = operation;

        Result = operation switch
        {
            Operation.Add => Value + previous.Result,
            Operation.Multiply => Value * previous.Result,
            Operation.Concatenate => ulong.Parse($"{previous.Result}{Value}"),
            _ => Result = value
        };
    }

    public override string ToString()
    {
        if (!HasPrevious)
            return Value.ToString();

        string opStr = Operation switch
        {
            Operation.Add => "+",
            Operation.Multiply => "*",
            _ => "||"
        };
        return $"{Previous} {opStr} {Value}";
    }
}