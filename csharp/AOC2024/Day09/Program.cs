using System.Text;

namespace Day09;

class Program
{
    static void Main()
    {
        Console.WriteLine("Hello, World!");

        Console.WriteLine(Part1("sample1.txt"));
        Console.WriteLine(Part1("1.txt"));
        
        Console.WriteLine(Part2("sample1.txt"));
        Console.WriteLine(Part2("1.txt"));
    }
    
    private static ulong Part1(string filename)
    {
        using FileStream fileStream = File.OpenRead(filename);
        using StreamReader streamReader = new(fileStream, Encoding.UTF8, true);
        
        FileSystem fs = FileSystem.FromMap(streamReader);
        fs.Compact();
        
        return fs.Checksum();
    }

    private static ulong Part2(string filename)
    {
        using FileStream fileStream = File.OpenRead(filename);
        using StreamReader streamReader = new(fileStream, Encoding.UTF8, true);
        
        FileSystem fs = FileSystem.FromMap(streamReader);
        fs.Defragment();
        
        return fs.Checksum();
    }

    private static void PrintFileSystem(FileSystem fs, int maxLineLength)
    {
        int charsWrittenOnLine = 0;
        foreach (uint? u in fs.Blocks)
        {
            if (u is null)
                Console.Write('.');
            else
            {
                char? c = (char?)(u + 48);
                Console.Write(c);
            }

            charsWrittenOnLine++;
            if (charsWrittenOnLine >= maxLineLength)
            {
                Console.WriteLine();
                charsWrittenOnLine = 0;
            }
        }

        Console.WriteLine();
    }
}

class FileSystem
{
    private readonly List<uint?> blocks = [];
    private readonly Dictionary<uint, FileObj> files = [];
    private uint lastId = 0;
    private int lastStartIndex = 0;
    
    public IReadOnlyList<uint?> Blocks => blocks;

    public static FileSystem FromMap(StreamReader streamReader)
    {
        FileSystem result = new();
        bool createFile = true;
        char[] buffer = new char[1024];
        int charsRead;
        while ((charsRead = streamReader.Read(buffer, 0, buffer.Length)) > 0)
        {
            for (int i = 0; i < charsRead; i++)
            {
                char c = buffer[i];
                uint length = (uint)c - 48;
                if (createFile)
                {
                    result.CreateFile(length);
                    createFile = false;
                }
                else
                {
                    result.CreateFreeSpace(length);
                    createFile = true;
                }
            }
        }

        return result;
    }

    public void Compact()
    {
        // This solves part1, but the file objects are broken after this method completes because
        // I'm too lazy to model fragmented files....
        
        int firstFreeBlock = 0;
        while (blocks[firstFreeBlock] is not null)
            firstFreeBlock++;
        
        for (int i = blocks.Count - 1; i >= firstFreeBlock; i--)
        {
            if (blocks[i] is null)
                continue;
            blocks[firstFreeBlock] = blocks[i];
            blocks[i] = null;
            while (blocks[firstFreeBlock] is not null)
                firstFreeBlock++;
        }
    }

    public void Defragment()
    {
        foreach (uint fileId in files.Keys.Reverse())
        {
            if (files[fileId].HasBeenMoved)
                continue;
            
            int startOfFreeSpace = FindStartOfFreeSpaceOfSize(files[fileId].Size);
            if (startOfFreeSpace < 0 || startOfFreeSpace >= files[fileId].StartIndex)
            {
                files[fileId].HasBeenMoved = true;
                continue;
            }
            
            Move(fileId, startOfFreeSpace);
        }
    }

    public ulong Checksum()
    {
        ulong hash = 0;
        for (int i = 0; i < blocks.Count; i++)
        {
            if (blocks[i] is null)
                continue;
            hash += (ulong)i * blocks[i]!.Value;
        }

        return hash;
    }

    private int FindStartOfFreeSpaceOfSize(uint size)
    {
        bool freeSpaceFound = false;
        int startOfFreeSpace = 0;
        int lengthOfFreeSpace = 0;
        for (int i = 0; i < blocks.Count; i++)
        {
            if (!freeSpaceFound && blocks[i].HasValue)
                continue;

            if (!freeSpaceFound && !blocks[i].HasValue)
            {
                freeSpaceFound = true;
                startOfFreeSpace = i;
                lengthOfFreeSpace = 1;
                if (lengthOfFreeSpace >= size)
                    return startOfFreeSpace;
                continue;
            }

            if (freeSpaceFound && !blocks[i].HasValue)
            {
                lengthOfFreeSpace++;
                if (lengthOfFreeSpace >= size)
                    return startOfFreeSpace;
            }

            if (freeSpaceFound && blocks[i].HasValue)
            {
                freeSpaceFound = false;
                startOfFreeSpace = 0;
                lengthOfFreeSpace = 0;
            }
        }

        return -1;
    }

    private void Move(uint fileId, int startOfFreeSpace)
    {
        FileObj file = files[fileId];
        int index = startOfFreeSpace;
        int blocksMoved = 0;
        foreach (uint? blockValue in file.GetBlockValues())
        {
            blocks[index + blocksMoved] = blockValue;
            blocks[file.StartIndex + blocksMoved] = null;
            blocksMoved++;
        }
        file.StartIndex = startOfFreeSpace;
        file.HasBeenMoved = true;
    }
    
    private void CreateFile(uint size)
    {
        FileObj file = new(lastId++, size, lastStartIndex);
        blocks.AddRange(file.GetBlockValues());
        files.Add(file.Id, file);
        lastStartIndex += (int)size;
    }

    private void CreateFreeSpace(uint size)
    {
        for (int i = 0; i < size; i++)
        {
            blocks.Add(null);
            lastStartIndex++;
        }
    }
}

class FileObj
{
    public readonly uint Id;
    public readonly uint Size;
    public int StartIndex;
    public int EndIndex => (int)(StartIndex + Size - 1);
    public bool HasBeenMoved = false;

    public FileObj(uint id, uint size, int startIndex)
    {
        Id = id;
        Size = size;
        StartIndex = startIndex;
    }

    public IEnumerable<uint?> GetBlockValues()
    {
        for (int i = 0; i < Size; i++)
            yield return Id;
    }
}