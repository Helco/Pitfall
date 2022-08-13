using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PropertyValues
{
    public readonly List<(string file, string property, uint value)> values = new List<(string file, string property, uint value)>();
    public readonly HashSet<(string file, string property)> ValueExistence = new HashSet<(string file, string property)>();
    public readonly HashSet<string> AllFiles = new HashSet<string>();
    public readonly HashSet<string> AllProperties = new HashSet<string>();

    public void Add(string file, string property, uint value)
    {
        AllFiles.Add(file);
        AllProperties.Add(property);
        if (ValueExistence.Contains((file, property)))
            Console.WriteLine($"Property {property} is already set for {file}");
        else
        {
            values.Add((file, property, value));
            ValueExistence.Add((file, property));
        }
    }

    public void AddBits(string file, string property, uint value, int bitCount = 32)
    {
        for (int i = 0; i < bitCount; i++)
            Add(file, property + "_b" + i, (value & (1 << i)) > 0 ? 1u : 0u);
    }

    public IEnumerable<string> GetFilesWith(string property, uint value) =>
        values.Where(t => t.property == property && t.value == value).Select(t => t.file);

    public IEnumerable<string> GetFilesNotWith(string property, uint value) =>
        values.Where(t => t.property == property && t.value != value).Select(t => t.file);

    public IEnumerable<string> GetMatchingProperties(IReadOnlySet<string> positiveFiles, IReadOnlySet<string> negativeFiles, bool filterZero)
    {
        var potential = AllProperties.ToDictionary(p => p, p => null as uint?);
        foreach (var (file, property, value) in values)
        {
            if (!positiveFiles.Contains(file))
                continue;
            if (!potential.TryGetValue(property, out var expectedValue))
                continue;
            if (expectedValue == null && (!filterZero || value != 0))
            {
                potential[property] = value;
                continue;
            }
            if (expectedValue == value)
                continue;

            potential.Remove(property);
            if (potential.Count == 0)
                return potential.Keys;
        }

        foreach (var (file, property, value) in values)
        {
            if (!negativeFiles.Contains(file))
                continue;
            if (!potential.TryGetValue(property, out var expectedValue))
                continue;
            if (expectedValue == value)
                potential.Remove(property);
            if (potential.Count == 0)
                return potential.Keys;
        }
        return potential.Keys;
    }

    public void Save(string filename)
    {
        File.WriteAllLines(filename, values.Select(t => $"{t.property}\t{t.file}\t{t.value}"));
    }

    public void Load(string filename)
    {
        var tuples = File.ReadAllLines(filename).Select(l =>
        {
            try
            {
                var p = l.Split('\t');
                return (p[0], p[1], uint.Parse(p[2]));
            }
            catch (Exception)
            {
                return (null, null, 0u)!;
            }
        });
        foreach (var (property, file, value) in tuples)
            Add(file, property, value);
    }

    public void LoadForProperty(string filename, string property)
    {
        var tuples = File.ReadAllLines(filename).Select(l =>
        {
            try
            {
                var p = l.Split('\t');
                return (p[0], uint.Parse(p[1]));
            }
            catch (Exception)
            {
                return (null, 0u)!;
            }
        });
        foreach (var (file, value) in tuples)
            Add(file, property, value);
    }
}

public class MatchFinder
{
    public readonly PropertyValues Actual = new PropertyValues();
    public readonly PropertyValues Expected = new PropertyValues();

    public IEnumerable<string> FindActualPropertyForExpected(string expectedProp, uint expectedValue = 1, bool filterZero = true)
    {
        var positive = Expected.GetFilesWith(expectedProp, expectedValue).ToHashSet();
        var negative = Expected.GetFilesNotWith(expectedProp, expectedValue).ToHashSet();
        return Actual.GetMatchingProperties(positive, negative, filterZero);
    }
}
