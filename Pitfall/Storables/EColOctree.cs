using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pitfall.Storables;

[StorableType]
public class EColOctree : EStorable
{
    public byte[] Bytes { get; private set; } = Array.Empty<byte>();
    public uint[] Ints { get; private set; } = Array.Empty<uint>();
    public uint4[] Int4s { get; private set; } = Array.Empty<uint4>();
    public uint2[] Int2s { get; private set; } = Array.Empty<uint2>();
    public Vector3 Vector1 { get; private set; }
    public Vector3 Vector2 { get; private set; }

    public override void Read(BinaryReader reader)
    {
        if (ReadVersion != 0)
            throw new NotSupportedException($"Unsupported read version for EColOctree: {ReadVersion}");

        Bytes = reader.ReadBytes(reader.ReadInt32());
        Ints = reader.ReadArray(reader.ReadUInt32);
        Int4s = reader.ReadArray(reader.ReadUIntVector4);
        Int2s = reader.ReadArray(reader.ReadUIntVector2);
        Vector1 = reader.ReadVector3();
        Vector2 = reader.ReadVector3();
    }
}
