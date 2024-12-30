namespace Day22;

public static class Program
{
    public static void Main()
    {
        Console.WriteLine(Mix(15, 42)); // 37
        Console.WriteLine(Prune(100000000)); // 16113920
        Console.WriteLine(GenerateNewSecret(123, 10)); // 5908254

        Console.WriteLine(Part1([1, 10, 100, 2024])); // 37327623
        
        Console.WriteLine(Part1(File.ReadAllLines("1.txt").Select(ulong.Parse).ToArray()));
        
        Console.WriteLine(OnesNumber(123)); // 3
        Console.WriteLine(OnesNumber(15887950)); // 0
        Console.WriteLine(OnesNumber(16495136)); // 6

        MakeOffers(123, 10);
    }

    private static ulong MakeOffers(ulong secret, ushort iterations)
    {
        short previousPrice = 0;
        for (ushort i = 0; i < iterations; i++)
        {
            byte price = OnesNumber(secret);
            short delta = (short)(price - previousPrice);
            Console.Write($"{secret}: {price}");
            if (i > 0)
                Console.Write($" ({delta})");
            Console.WriteLine();
            secret = NewSecretNumber(secret);
            previousPrice = price;
        }
        
        return secret;
    }

    private static ulong Part1(ulong[] buyers)
    {
        ulong sum = 0;
        foreach (ulong buyer in buyers)
        {
            ulong newBuyerSecret = GenerateNewSecret(buyer, 2000);
            sum += newBuyerSecret;
            Console.WriteLine($"{buyer}: {newBuyerSecret}");
        }

        return sum;
    }

    private static ulong GenerateNewSecret(ulong secret, ushort iterations)
    {
        for (ushort i = 0; i < iterations; i++) 
            secret = NewSecretNumber(secret);
        
        return secret;
    }

    private static ulong NewSecretNumber(ulong secretNumber)
    {
        ulong result = secretNumber * 64;
        secretNumber = Mix(result, secretNumber);
        secretNumber = Prune(secretNumber);
        
        result = (uint)Math.Floor(secretNumber / 32.0);
        secretNumber = Mix(result, secretNumber);
        secretNumber = Prune(secretNumber);
        result = secretNumber * 2048;
        
        secretNumber = Mix(result, secretNumber);
        secretNumber = Prune(secretNumber);
        return secretNumber;
    }

    private static ulong Mix(ulong secretNumber, ulong givenValue)
    {
        return secretNumber ^ givenValue;
    }

    private static ulong Prune(ulong secretNumber)
    {
        return secretNumber % 16777216;
    }

    private static byte OnesNumber(ulong number)
    {
        char c = number.ToString()[^1];
        byte b = (byte)(c - 48);
        return b;
    }
}