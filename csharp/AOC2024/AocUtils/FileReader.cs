using System.Text;

namespace AocUtils;

public static class FileReader
{
    public static IEnumerable<string> ReadLines(string filename)
    {
        using FileStream fileStream = File.OpenRead(filename);
        using StreamReader streamReader = new(fileStream, Encoding.UTF8, true);

        while (streamReader.ReadLine() is { } line)
            yield return line;
    }

    public static IEnumerable<(char c, uint x, uint y, bool newLine)> ReadChars(string filename)
    {
        uint x = 0;
        uint y = 0;
        bool newLine = true;
        
        foreach (string line in ReadLines(filename))
        {
            foreach (char c in line)
            {
                yield return (c, x, y, newLine);
                newLine = false;
                x++;
            }

            x = 0;
            y++;
            newLine = true;
        }
    }
}