using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Pitfall.Storables;
using static System.Buffers.Binary.BinaryPrimitives;

namespace Pitfall;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
public readonly record struct Byte4(byte R, byte G, byte B, byte A)
{
    public Vector4 AsNormalized => new Vector4(R, G, B, A) / 255f;
}

public readonly record struct uint2(uint X, uint Y);
public readonly record struct uint4(uint X, uint Y, uint Z, uint W);

public readonly record struct AABB(Vector3 Min, Vector3 Max);

public readonly record struct StringPair(string A = "", string B = "");

internal static class Utils
{
    public static T[] ReadArray<T>(this BinaryReader _, int count, Func<T> readElement)
    {
        var result = new T[count];
        for (int i = 0; i < count; i++)
            result[i] = readElement();
        return result;
    }

    public static T[] ReadArray<T>(this BinaryReader reader, int count, Func<BinaryReader, T> readElement)
    {
        var result = new T[count];
        for (int i = 0; i < count; i++)
            result[i] = readElement(reader);
        return result;
    }

    public static T[] ReadArray<T>(this BinaryReader reader, Func<T> readElement) =>
        reader.ReadArray(reader.ReadInt32(), readElement);

    public static T[] ReadArray<T>(this BinaryReader reader, Func<BinaryReader, T> readElement) =>
        reader.ReadArray(reader.ReadInt32(), readElement);

    public static Vector4 ReadVector4(this BinaryReader r) => new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
    public static Quaternion ReadQuaternion(this BinaryReader r) => new Quaternion(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
    public static Vector3 ReadVector3(this BinaryReader r) => new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
    public static Byte4 ReadByte4(this BinaryReader r) => new Byte4(r.ReadByte(), r.ReadByte(), r.ReadByte(), r.ReadByte());
    public static Vector2 ReadVector2(this BinaryReader r) => new Vector2(r.ReadSingle(), r.ReadSingle());
    public static AABB ReadAABB(this BinaryReader r) => new AABB(r.ReadVector3(), r.ReadVector3());
    public static StringPair ReadStringPair(this BinaryReader r) => new(r.ReadCString(), r.ReadCString());
    public static uint2 ReadUIntVector2(this BinaryReader r) => new uint2(r.ReadUInt32(), r.ReadUInt32());
    public static uint4 ReadUIntVector4(this BinaryReader r) => new uint4(r.ReadUInt32(), r.ReadUInt32(), r.ReadUInt32(), r.ReadUInt32());


    public static Matrix4x4 ReadMatrix4x4(this BinaryReader r) => new Matrix4x4(
        r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
        r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
        r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
        r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());

    public static string ReadCString(this BinaryReader reader)
    {
        var nameBytes = new List<byte>();
        while (true)
        {
            var b = reader.ReadByte();
            if (b == 0)
                break;
            nameBytes.Add(b);
        }
        return System.Text.Encoding.UTF8.GetString(nameBytes.ToArray());
    }

    public static IEnumerable<(int, T)> Indexed<T>(this IEnumerable<T> set) => set.Select((v, i) => (i, v));
}
