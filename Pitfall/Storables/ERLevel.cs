using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pitfall.Storables;

[StorableType]
public class ERLevel : EResource
{
    public class Section
    {
        public EStorable? Storable1 { get; init; }
        public EStorable? Storable2 { get; init; }
        public IReadOnlyList<EStorable?> List1 { get; init; } = Array.Empty<EStorable?>();
        public IReadOnlyList<EStorable?> List2 { get; init; } = Array.Empty<EStorable?>();
    }

    public IReadOnlyDictionary<uint, EStorable?> Map { get; private set; } = new Dictionary<uint, EStorable?>();
    public IReadOnlyList<EStorable?> List { get; private set; } = Array.Empty<EStorable?>();
    public IReadOnlyList<Section> Sections { get; private set; } = Array.Empty<Section>();
    public EStorable? Final { get; private set; }

    public override void Read(BinaryReader reader)
    {
        if (ReadVersion != 0)
            throw new NotSupportedException($"Unsupported ERLevel read version: {ReadVersion}");

        base.Read(reader);
        var mapCount = reader.ReadInt32();
        var map = new Dictionary<uint, EStorable?>();
        for (int i = 0; i < mapCount; i++)
            map.Add(reader.ReadUInt32(), reader.ReadStorable());
        Map = map;

        List = reader.ReadArray(reader.ReadInt32(), DynTypeInfo.ReadStorable);
        Sections = reader.ReadArray(reader.ReadInt32(), ReadSection);
        Final = reader.ReadStorable();
    }

    private static Section ReadSection(BinaryReader reader) => new Section
    {
        Storable1 = reader.ReadStorable(),
        Storable2 = reader.ReadStorable(),
        List1 = reader.ReadArray(reader.ReadInt32(), DynTypeInfo.ReadStorable),
        List2 = reader.ReadArray(reader.ReadInt32(), DynTypeInfo.ReadStorable)
    };
}
