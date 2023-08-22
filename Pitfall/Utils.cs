using System.Numerics;
using System.Runtime.InteropServices;
using Pitfall.Storables;

namespace Pitfall;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
public readonly record struct Byte4(byte R, byte G, byte B, byte A)
{
    public Vector4 AsNormalized => new Vector4(R, G, B, A) / 255f;
}

public readonly record struct AABB(Vector3 Min, Vector3 Max);

internal static class Utils
{
    public static T[] ReadArray<T>(this BinaryReader reader, int count, Func<BinaryReader, T> readElement) =>
        Enumerable.Repeat(reader, count).Select(readElement).ToArray();

    public static unsafe T[] ReadArray<T>(this BinaryReader reader, int count, int expectedSize) where T : unmanaged
    {
        if (!BitConverter.IsLittleEndian || sizeof(T) != expectedSize)
            throw new NotSupportedException("Fast path is not available on this platform");
        var result = new T[count];
        var resultData = MemoryMarshal.AsBytes(result.AsSpan());
        if (reader.BaseStream.Read(resultData) != resultData.Length)
            throw new EndOfStreamException();
        return result;
    }

    public static Vector4 ReadVector4(this BinaryReader r) => new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
    public static Vector3 ReadVector3(this BinaryReader r) => new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
    public static Byte4 ReadByte4(this BinaryReader r) => new Byte4(r.ReadByte(), r.ReadByte(), r.ReadByte(), r.ReadByte());
    public static Vector2 ReadVector2(this BinaryReader r) => new Vector2(r.ReadSingle(), r.ReadSingle());
    public static AABB ReadAABB(this BinaryReader r) => new AABB(r.ReadVector3(), r.ReadVector3());

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
