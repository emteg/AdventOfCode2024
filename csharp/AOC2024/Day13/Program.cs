using AocUtils;

namespace Day13;

public static class Program
{
    public static void Main()
    {
        Console.WriteLine($"Total tokens spent: {Part1("sample1.txt")}"); // 480
        Console.WriteLine($"Total tokens spent: {Part1("1.txt")}");
    }

    private static uint Part1(string filename)
    {
        List<ClawMachine> clawMachines = ReadClawMachines(filename);
        uint tokensSpent = 0;
        foreach (ClawMachine machine in clawMachines)
        {
            uint nA = 1;
            while (true)
            {
                if (nA > 100)
                {
                    Console.WriteLine($"Giving up on claw machine {clawMachines.IndexOf(machine)} after 100 button presses!");
                    break;
                }
                
                uint x = machine.ButtonA.X * nA;
                uint dx = machine.Prize.X - x;
                uint y = machine.ButtonA.Y * nA;
                uint dy = machine.Prize.Y - y;
                if (dx % machine.ButtonB.X == 0 && 
                    dy % machine.ButtonB.Y == 0 &&
                    dx / machine.ButtonB.X == dy / machine.ButtonB.Y)
                {
                    uint bButtonPresses = dx / machine.ButtonB.X;
                    if (bButtonPresses > 100)
                        Console.WriteLine("BUTTON B PRESSED MORE THAN 100 TIMES!");
                    
                    uint aButtonCost = 3 * nA;
                    uint machineTokensSpent = bButtonPresses + aButtonCost;
                    tokensSpent += machineTokensSpent;
                    Console.WriteLine($"Claw machine {clawMachines.IndexOf(machine)} reaches prize after pressing button A {nA} times ({aButtonCost} T) and pressing button B {bButtonPresses} times ({bButtonPresses} T) for a total of {machineTokensSpent} T.");
                    break;
                }

                nA++;
            }
        }

        return tokensSpent;
    }

    private static List<ClawMachine> ReadClawMachines(string filename)
    {
        List<ClawMachine> clawMachines = [];
        (uint x, uint y) a = (0, 0);
        (uint x, uint y) b = (0, 0);
        int counter = 0;
        int clawMachineCounter = 0;
        foreach (string line in FileReader.ReadLines(filename))
        {
            if (line.Length == 0)
                continue;
            
            string[] values = line.Split(": ")[1].Split(", ");
            if (counter == 0)
            {
                a = (uint.Parse(values[0][2..]), uint.Parse(values[1][2..]));
                counter++;
                continue;
            }

            if (counter == 1)
            {
                b = (uint.Parse(values[0][2..]), uint.Parse(values[1][2..]));
                counter++;
                continue;
            }
            
            counter = 0;
            (uint x, uint y) prize = (uint.Parse(values[0][2..]), uint.Parse(values[1][2..]));
            ClawMachine clawMachine = new(a, b, prize);
            clawMachineCounter++;

            if (clawMachine.PrizePotentiallyReachableThroughButtonCombination())
                clawMachines.Add(clawMachine);
            else
                Console.WriteLine($"Prize of claw machine {clawMachineCounter} is not reachable.");
        }

        return clawMachines;
    }
}

public sealed class ClawMachine
{
    public readonly (uint X, uint Y) Prize;
    public readonly (uint X, uint Y) ButtonA;
    public readonly (uint X, uint Y) ButtonB;

    public ClawMachine((uint x, uint y) a, (uint x, uint y) b, (uint x, uint y) prize)
    {
        Prize = prize;
        ButtonA = a;
        ButtonB = b;
    }

    public bool PrizePotentiallyReachableThroughButtonCombination()
    {
        double yA = (ButtonA.Y / (double)ButtonA.X) * Prize.X;
        double yB = (ButtonB.Y / (double)ButtonB.X) * Prize.X;
        
        return yA <= Prize.Y && yB >= Prize.Y || yA >= Prize.Y && yB <= Prize.Y;
    }
}