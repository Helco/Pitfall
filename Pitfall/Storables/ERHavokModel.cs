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
            Vertices = reader.ReadArray(reader.ReadVector4);
            var triangleCount = reader.ReadInt32();
            Indices = reader.ReadArray(triangleCount * 3, reader.ReadUInt16);
        }
    }

    public SubModel[] SubModels { get; private set; } = Array.Empty<SubModel>();

    public override void Read(BinaryReader reader)
    {
        base.Read(reader);
        if (ReadVersion != 0)
            throw new NotSupportedException($"Unsupported ERHavokModel version: " + ReadVersion);

        SubModels = reader.ReadArray(() => new SubModel(reader));

        if (reader.ReadInt32() != 0)
            throw new NotSupportedException($"Second part of ERHavokModel is not supported yet");
    }
}
