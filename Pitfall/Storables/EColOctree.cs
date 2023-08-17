using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pitfall.Storables;

public readonly record struct uint2(uint X, uint Y);
public readonly record struct uint4(uint X, uint Y, uint Z, uint W);

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

        Bytes = reader.ReadArray<byte>(reader.ReadInt32(), 1);
        Ints = reader.ReadArray<uint>(reader.ReadInt32(), 4);
        Int4s = reader.ReadArray<uint4>(reader.ReadInt32(), 16);
        Int2s = reader.ReadArray<uint2>(reader.ReadInt32(), 8);
        Vector1 = reader.ReadVector3();
        Vector2 = reader.ReadVector3();
    }
}
