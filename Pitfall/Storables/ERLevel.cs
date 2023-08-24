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
        // usually EBoundTreeNode to build up a BSP of mostly StaticSubModel(Shader)
        public EStorable? Storable { get; init; }

        // these are always null/empty for PC files
        public EStorable? NullStorable { get; init; }
        public IReadOnlyList<EStorable?> EmptyList1 { get; init; } = Array.Empty<EStorable?>();
        public IReadOnlyList<EStorable?> EmptyList2 { get; init; } = Array.Empty<EStorable?>();
    }

    public IReadOnlyDictionary<uint, EStorable?> EmptyMap { get; private set; } = new Dictionary<uint, EStorable?>();
    public IReadOnlyList<EILight> Lights { get; private set; } = Array.Empty<EILight>();
    public IReadOnlyList<Section> Sections { get; private set; } = Array.Empty<Section>();
    public ELightOctree? LightOctree { get; private set; }

    public override void Read(BinaryReader reader)
    {
        if (ReadVersion != 0)
            throw new NotSupportedException($"Unsupported ERLevel read version: {ReadVersion}");

        base.Read(reader);
        var mapCount = reader.ReadInt32();
        var map = new Dictionary<uint, EStorable?>();
        for (int i = 0; i < mapCount; i++)
            map.Add(reader.ReadUInt32(), reader.ReadStorable());
        EmptyMap = map;

        Lights = reader.ReadArray(reader.ReadInt32(), DynTypeInfo.ExpectStorable<EILight>);
        Sections = reader.ReadArray(reader.ReadInt32(), ReadSection);
        LightOctree = reader.ExpectStorable<ELightOctree>();
    }

    private static Section ReadSection(BinaryReader reader) => new Section
    {
        Storable = reader.ReadStorable(),
        NullStorable = reader.ReadStorable(),
        EmptyList1 = reader.ReadArray(reader.ReadInt32(), DynTypeInfo.ReadStorable),
        EmptyList2 = reader.ReadArray(reader.ReadInt32(), DynTypeInfo.ReadStorable)
    };
}
