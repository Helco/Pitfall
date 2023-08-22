using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pitfall.Storables;

[StorableType]
public class ELightOctree : EStorable
{
}

[StorableType]
public class ELightOctreeSmall : ELightOctree
{
    public int NodeCount { get; private set; }
    public ushort[] NodeChildren { get; private set; } = Array.Empty<ushort>(); // 8 children per node
    public EStorable?[][] Nodes { get; private set; } = Array.Empty<EStorable?[]>(); // probably only ELOLeaf and ELOSplit
    public AABB Bounds { get; private set; }

    public override void Read(BinaryReader reader)
    {
        NodeCount = reader.ReadInt32();
        NodeChildren = reader.ReadArray<ushort>(NodeCount * 8, 2);
        Nodes = reader.ReadArray(reader.ReadInt32(),
            r => r.ReadArray(reader.ReadInt32(), DynTypeInfo.ReadStorable));
        Bounds = reader.ReadAABB();
    }
}
