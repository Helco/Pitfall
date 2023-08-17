using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pitfall.Storables;

[StorableType]
public class ERHavokModel : EResource
{
    public class SubModel
    {
        public Vector4[] Vertices { get; }
        public ushort[] Indices { get; }

        public SubModel(BinaryReader reader)
        {
            var vertexCount = reader.ReadInt32();
            Vertices = reader.ReadArray<Vector4>(vertexCount, 16);
            var triangleCount = reader.ReadInt32();
            Indices = reader.ReadArray<ushort>(triangleCount * 3, 2);
        }
    }

    public SubModel[] SubModels { get; private set; } = Array.Empty<SubModel>();

    public override void Read(BinaryReader reader)
    {
        base.Read(reader);
        if (ReadVersion != 0)
            throw new NotSupportedException($"Unsupported ERHavokModel version: " + ReadVersion);

        var subModelCount = reader.ReadInt32();
        SubModels = reader.ReadArray(subModelCount, r => new SubModel(r));

        if (reader.ReadInt32() != 0)
            throw new NotSupportedException($"Second part of ERHavokModel is not supported yet");
    }
}
