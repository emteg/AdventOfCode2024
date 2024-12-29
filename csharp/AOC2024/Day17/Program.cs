using System.Text;

namespace Day17;

public static class Program
{
    public static void Main()
    {
        TestCases();
        Console.WriteLine(Part1("sample1.txt"));
        Console.WriteLine(Part1("1.txt"));
    }

    private static string Part1(string filename)
    {
        using FileStream fileStream = File.OpenRead(filename);
        using StreamReader streamReader = new(fileStream, Encoding.UTF8, true);
        List<int> registers = [];
        int[] program = [];
        while (streamReader.ReadLine() is { } line)
        {
            if (line.Length == 0)
                continue;
            
            if (registers.Count < 3)
                registers.Add(int.Parse(line.Split(": ")[1]));
            else
                program = line.Split(": ")[1].Split(',').Select(int.Parse).ToArray();
        }
        Computer computer = new(registers[0], registers[1], registers[2], program);
        computer.Run();
        string result = computer.PrintOutput();
        return result;
    }

    private static void TestCases()
    {
        Computer computer = new(0, 0, 9, [2, 6]);
        computer.Run(); // B is set to 1
        
        computer = new(10, 0, 0, [5,0,5,1,5,4]);
        computer.Run(); // output = 0,1,2

        computer = new Computer(2024, 0, 0, [0,1,5,4,3,0]);
        computer.Run(); // output = 4,2,5,6,7,7,7,7,3,1,0 AND A = 0
        
        computer = new Computer(0, 29, 0, [1,7]);
        computer.Run(); // B is set to 26

        computer = new Computer(0, 2024, 43690, [4,0]);
        computer.Run(); // B is set to 44354
    }
}

public class Computer
{
    public int InstructionPointer { get; private set; }
    public int RegisterA { get; private set; }
    public int RegisterB { get; private set; }
    public int RegisterC { get; private set; }
    public readonly List<int> Output = [];
    public readonly int[] Program;

    public Computer(int a, int b, int c, int[] program)
    {
        RegisterA = a;
        RegisterB = b;
        RegisterC = c;
        Program = program;
    }

    public string PrintOutput() => string.Join(",", Output);

    public void Run()
    {
        while (true)
        {
            if (!Tick())
                break;
        }
    }
    
    public bool Tick()
    {
        if (InstructionPointer > Program.Length - 2)
            return false;
        
        int opCode = Program[InstructionPointer];
        int operand = Program[InstructionPointer + 1];

        InstructionPointer = opCode switch
        {
            0 => ExecuteAdv(operand),
            1 => ExecuteBxl(operand),
            2 => ExecuteBst(operand),
            3 => ExecuteJnz(operand),
            4 => ExecuteBxc(operand),
            5 => ExecuteOut(operand),
            6 => ExecuteBdv(operand),
            7 => ExecuteCdv(operand),
            _ => throw new InvalidOperationException($"Invalid opcode {opCode}!"),
        };

        return true;
    }

    private int ExecuteAdv(int comboOperand) // adv <combo>     A = A / 2^<combo>
    {
        RegisterA = (int)Math.Truncate(RegisterA / Math.Pow(2, FetchComboOperandValue(comboOperand)));
        return InstructionPointer + 2;
    }
    
    private int ExecuteBdv(int comboOperand) // adv <combo>     B = A / 2^<combo>
    {
        RegisterB = (int)Math.Truncate(RegisterA / Math.Pow(2, FetchComboOperandValue(comboOperand)));
        return InstructionPointer + 2;
    }
    
    private int ExecuteCdv(int comboOperand) // adv <combo>     C = A / 2^<combo>
    {
        RegisterC = (int)Math.Truncate(RegisterA / Math.Pow(2, FetchComboOperandValue(comboOperand)));
        return InstructionPointer + 2;
    }

    private int ExecuteBxl(int literalOperand) // bxl <literal>: B = B XOR <literal>
    {
        RegisterB ^= literalOperand;
        return InstructionPointer + 2;
    }

    private int ExecuteBst(int comboOperand) // bst <combo>: B = combo % 8
    {
        RegisterB = FetchComboOperandValue(comboOperand) % 8;
        return InstructionPointer + 2;
    }

    private int ExecuteJnz(int literalOperand) // jnz <literal>: nothing, if A == 0, else set instruction pointer to <literal> (dont increment IP)
    {
        if (RegisterA == 0)
            return InstructionPointer + 2;

        return literalOperand;
    }

    private int ExecuteBxc(int ignoredOperand) // bxc <ignored>: B = B XOR C (operand is ignored)
    {
        RegisterB ^= RegisterC;
        return InstructionPointer + 2;
    }

    private int ExecuteOut(int comboOperand) // out <combo>: output <combo> % 8
    {
        Output.Add(FetchComboOperandValue(comboOperand) % 8);
        return InstructionPointer + 2;
    }

    private int FetchComboOperandValue(int operand)
    {
        return operand switch
        {
            0 or 1 or 2 or 3 => operand,
            4 => RegisterA,
            5 => RegisterB,
            6 => RegisterC,
            _ => throw new InvalidOperationException($"Illegal operand {operand}")
        };
    }
}