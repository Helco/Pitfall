namespace ModelConvert;
using System;
using System.IO;

internal class Program
{
    static void Main(string[] args) => MainMatch(args);

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
        void addByBit(string name, uint val, int i)
        {
            
            if ((val & (1 << i)) > 0)
            {
                counts[i]++;
                if (lists[i] == null)
                    lists[i] = name;
                else
                    lists[i] += "\n" + name;
            }
            else
            {
                if (nlists[i] == null)
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
        }

        var t = 0;
        var min = 1000;
        var max = -10000;
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
