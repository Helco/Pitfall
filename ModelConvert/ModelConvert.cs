namespace ModelConvert;
using System;
using System.IO;
using System.Reflection.PortableExecutable;

internal class Program
{
    static void Main(string[] args) => MainFindVertexBlocks(args);

    static void MainCheckBoneCounts(string[] args)
    {
        var files = Directory.GetFiles(@"C:\Users\Helco\Downloads\PITFALL The Lost Expedition PC\PITFALL The Lost Expedition\Game\data\models");
        foreach (var file in files)
        {
            if (file.Contains("files.list"))
                continue;
            var name = Path.GetFileName(file);
            using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(fileStream);
            var bytes = reader.ReadBytes(6);
            while (reader.ReadByte() != 0) ;
            reader.ReadByte(); // these two properties are
            reader.ReadSingle(); // constant over all models

            int subModelCount = reader.ReadInt32();
            reader.ReadInt32();
            int variantCount = reader.ReadInt32();
            reader.ReadUInt32(); // id
            reader.ReadUInt32(); // vertexFlags

            if (subModelCount == 0 || variantCount == 0)
                continue;

            var boneCount = reader.ReadByte();
            if (boneCount != 1)
            Console.WriteLine($"{boneCount:d3} - {name}");
        }
    }

    static void MainFindVertexBlocks(string[] args)
    {
        var bytes = File.ReadAllBytes(@"C:\Users\Helco\Downloads\PITFALL The Lost Expedition PC\PITFALL The Lost Expedition\Game\data\models\leechmodel");
        var zeroWords = Enumerable
            .Range(16, bytes.Length - 4 - 16)
            .Where(i => bytes[i] < 4 && bytes[i + 1] == 0 && bytes[i + 2] == 0 && bytes[i + 3] == 0)
            .Where(i => isGoodFloat(i - 12) && isGoodFloat(i - 8) && isGoodFloat(i - 4))
            .Where(i => BitConverter.ToSingle(bytes, i - 12) != 0f || BitConverter.ToSingle(bytes, i - 8) != 0f || BitConverter.ToSingle(bytes, i - 4) != 0f)
            .Select(i => i - 12)
            .ToArray();

        var blocks = new List<(int start, int size)>();
        var curStart = zeroWords.First();
        var curEnd = curStart;
        foreach (var i in zeroWords.Skip(1))
        {
            if (i < curEnd + 16)
                continue;
            if (i == curEnd + 16)
                curEnd = i;
            else
            {
                while (Array.BinarySearch(zeroWords, curStart - 16) >= 0)
                    curStart -= 16;
                blocks.Add((curStart, curEnd - curStart + 16));
                curStart = curEnd = i;
            }
        }
        blocks.Add((curStart, curEnd - curStart + 16));

        var plausibleBlocks = blocks.Where(b => b.size >= 2 * 16);
        foreach (var block in plausibleBlocks)
            Console.WriteLine($"{block.start:X8} - {block.size:X4} - {block.size / 16:X4} vertices");

        var firstVariant = plausibleBlocks.Where(b => b.start < 0x17C47);
        Console.WriteLine($"There are {firstVariant.Count()} blocks before 0x17C47");

        foreach (var block in firstVariant)
            Console.WriteLine($"{block.start:X8} -> " + NicerHexString(block.start - 16, 16));

        bool isGoodFloat(int i)
        {
            float f = BitConverter.ToSingle(bytes, i);
            return float.IsFinite(f) && MathF.Abs(f) < 1000f;
        }

        string NicerHexString(int start, int count) => string.Join(" ",
            Enumerable.Range(0, count / 4).Select(i => Convert.ToHexString(bytes, start + i * 4, 4)));
    }

    static void MainMatch(string[] args)
    {
        var matchFinder = new MatchFinder();
        matchFinder.Actual.Load("actual.csv");

        string attr = "third-4b-attr";

        matchFinder.Expected.LoadForProperty(attr + ".txt", attr);

        for (uint i = 1; i <= 1; i++)
        {
            Console.WriteLine($"For {i}: " + string.Join(", ", matchFinder.FindActualPropertyForExpected(attr, i, filterZero: false)));
            Console.WriteLine($"For {i} zero filtered: " + string.Join(", ", matchFinder.FindActualPropertyForExpected(attr, i, filterZero: true)));
            Console.WriteLine();
        }

    }

    static void MainScan(string[] args)
    {
        var lists = new string[32];
        var nlists = new string[32];
        var counts = new int[32];
        /*void addByBit(string name, uint val, int i)
        {
            
            if ((val & (1 << i)) > 0)
            {
                counts![i]++;
                if (lists![i] == null)
                    lists[i] = name;
                else
                    lists[i] += "\n" + name;
            }
            else
            {
                if (nlists![i] == null)
                    nlists[i] = name;
                else
                    nlists[i] += "\n" + name;
            }
        }
        void addByBits(string name, uint val)
        {
            for (int i = 0; i < 32; i++)
            {
                addByBit(name, val, i);
            }
        }*/

        var t = 0;
        //var min = 1000;
        //var max = -10000;
        var set = new Dictionary<int, int>();

        var f = new Dictionary<string, uint>();
        var files = Directory.GetFiles(@"C:\Users\Helco\Downloads\PITFALL The Lost Expedition PC\PITFALL The Lost Expedition\Game\data\models");

        var propertySet = new PropertyValues();

        foreach (var file in files)
        {
            if (file.Contains("files.list"))
                continue;
            var name = Path.GetFileName(file);
            t++;
            using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(fileStream);
            var bytes = reader.ReadBytes(6);
            while (reader.ReadByte() != 0) ;
            reader.ReadByte(); // these two properties are
            reader.ReadSingle(); // constant over all models


            var count1 = reader.ReadUInt32();
            var i2 = reader.ReadUInt32();
            var count2 = reader.ReadUInt32();
            uint id = reader.ReadUInt32();
            var countOrFlags = reader.ReadUInt16();
            var pageSize = reader.ReadUInt16();
            var count4 = reader.ReadUInt32();
            var bb = reader.ReadByte();
            uint conditional1 = 0xCDCDCDCD;
            byte conditional2 = 0xCD;
            if (bb != 0 && bb != 4)
                conditional1 = reader.ReadUInt32();
            if (bb == 4)
                conditional2 = reader.ReadByte();

            propertySet.Add(name, nameof(count1), count1);
            propertySet.Add(name, nameof(i2), i2);
            propertySet.Add(name, nameof(count2), count2);
            propertySet.Add(name, nameof(countOrFlags), countOrFlags);
            propertySet.AddBits(name, nameof(countOrFlags), countOrFlags, 16);
            propertySet.Add(name, nameof(pageSize), pageSize);
            propertySet.AddBits(name, nameof(pageSize), pageSize, 16);
            propertySet.Add(name, nameof(count4), count4);
            propertySet.AddBits(name, nameof(count4), count4, 32);
            propertySet.Add(name, nameof(bb), bb);
            propertySet.AddBits(name, nameof(bb), bb, 8);
            propertySet.Add(name, nameof(conditional1), conditional1);
            propertySet.AddBits(name, nameof(conditional1), conditional1, 32);
            propertySet.Add(name, nameof(conditional2), conditional2);
            propertySet.AddBits(name, nameof(conditional2), conditional2, 8);
        }

        propertySet.Save("actual.csv");

        for (int i = 0; i < 32; i++)
        {
            File.WriteAllText($"{i}.txt", lists[i]);
            Console.WriteLine($"{i} {counts[i]} {counts[i] * 100 / t}");
        }

        foreach (var g in f.GroupBy(f => f.Value).OrderBy(g => g.Key))
        {
            Console.WriteLine(g.Key + " " + g.Count());
            Console.WriteLine(String.Join(", ", g.Select(k => k.Key)));
            Console.WriteLine();
        }
    }
}
