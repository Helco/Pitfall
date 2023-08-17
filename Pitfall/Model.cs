﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Net.WebSockets;
using static Pitfall.Utils;
using System.Runtime.InteropServices;

namespace Pitfall;

internal static class Utils
{
    public static T[] ReadArray<T>(BinaryReader reader, int count, Func<BinaryReader, T> readElement) =>
        Enumerable.Repeat(reader, count).Select(readElement).ToArray();

    public static Vector4 ReadVector4(this BinaryReader r) => new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
    public static Vector3 ReadVector3(this BinaryReader r) => new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
    public static Byte4 ReadByte4(this BinaryReader r) => new Byte4(r.ReadByte(), r.ReadByte(), r.ReadByte(), r.ReadByte());
    public static Vector2 ReadVector2(this BinaryReader r) => new Vector2(r.ReadSingle(), r.ReadSingle());    
}

public interface IModelPart { }

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
public readonly record struct Byte4(byte R, byte G, byte B, byte A)
{
    public Vector4 AsNormalized => new Vector4(R, G, B, A) / 255f;
}

[Flags]
internal enum GeometryFlags : uint
{
    HasTexCoords = 0x02,
    HasColors = 0x04,
    HasNormals = 0x08,
    IsHalfPrecision = 0x10,
    HasIndices = 0x20,
    DisableIndices = 0x1000_0000
}

internal enum PartType : byte
{
    Geometry,
    SetByte,
    EnableUnknown2,
    DisableUnknown3,
    EnableUnknown4,
    DisableUnknown5,
    End
}

public class GeometryPart : IModelPart
{
    public Vector4[] Positions { get; }
    public Vector2[]? TexCoords { get; }
    public Byte4[]? Colors { get; }
    public Vector3[]? Normals { get; }
    public Vector4[]? UnknownVector { get; }
    public ushort[]? Indices { get; }

    private static float GetNormalized(uint uvalue, int offset, int bits)
    {
        uint maxValue = 1u << bits;
        long value = (uvalue >> offset) & (maxValue - 1u);
        if ((value & (maxValue >> 1)) > 0)
            value -= maxValue;
        return value / (float)(maxValue >> 1);
    }

    private static Vector3 ReadNormal(BinaryReader r)
    {
        var value = r.ReadUInt32();
        var x = GetNormalized(value, 0, 11);
        var y = GetNormalized(value, 11, 11);
        var z = GetNormalized(value, 22, 10);
        return Vector3.Normalize(new Vector3(x, y, z));
    }

    private static Vector4 ReadUnknownVector4(BinaryReader r) => r.ReadByte4().AsNormalized;
    private static ushort ReadUShort(BinaryReader r) => r.ReadUInt16();

    internal GeometryPart(BinaryReader reader, GeometryFlags flags, bool hasUnknownVectors)
    {
        if (flags.HasFlag(GeometryFlags.IsHalfPrecision))
            throw new NotSupportedException("Unsupported half-precision model");

        int vertexCount = reader.ReadInt32();
        Positions = ReadArray(reader, vertexCount, Utils.ReadVector4);
        TexCoords = flags.HasFlag(GeometryFlags.HasTexCoords)
            ? ReadArray(reader, vertexCount, Utils.ReadVector2)
            : null;
        Colors = flags.HasFlag(GeometryFlags.HasColors)
            ? ReadArray(reader, vertexCount, Utils.ReadByte4)
            : null;
        Normals = flags.HasFlag(GeometryFlags.HasNormals)
            ? ReadArray(reader, vertexCount, ReadNormal)
            : null;
        UnknownVector = hasUnknownVectors
            ? ReadArray(reader, vertexCount, ReadUnknownVector4)
            : null;
        if (flags.HasFlag(GeometryFlags.HasIndices) && !flags.HasFlag(GeometryFlags.DisableIndices))
        {
            var indexCount = reader.ReadInt32();
            Indices = ReadArray(reader, indexCount, ReadUShort);
        }
        else
            Indices = null;
    }

    public ushort[] GenerateImplicitIndices()
    {
        var vertexCount = Positions.Length;
        var indices = new ushort[6 * vertexCount];
        for (int i = 0; i < vertexCount - 2; i += 2)
        {
            indices[i * 6 + 0] = (ushort)(i + 0);
            indices[i * 6 + 1] = (ushort)((i + 2) % vertexCount);
            indices[i * 6 + 2] = (ushort)(i + 1);
            indices[i * 6 + 3] = (ushort)(i + 1);
            indices[i * 6 + 4] = (ushort)((i + 2) % vertexCount);
            indices[i * 6 + 5] = (ushort)((i + 3) % vertexCount);
        }
        return indices;
    }
}

public class SetUnknownBytePart : IModelPart
{
    public ushort Index { get; }
    public byte Value { get; }

    public SetUnknownBytePart(BinaryReader reader)
    {
        Index = reader.ReadUInt16();
        Value = reader.ReadByte();
    }
}

public class SubSubModel
{
    public uint ID { get; }
    public uint GeometryCount { get; } // as reported by the model
    public IReadOnlyList<IModelPart> Parts { get; }

    public SubSubModel(BinaryReader reader)
    {
        ID = reader.ReadUInt32();
        var flags = (GeometryFlags)reader.ReadUInt32();
        GeometryCount = reader.ReadUInt32();
        var parts = new List<IModelPart>();
        Parts = parts;
        var hasUnknownVectors = false;

        PartType type;
        do
        {
            type = (PartType)reader.ReadByte();
            switch (type)
            {
                case PartType.Geometry: parts.Add(new GeometryPart(reader, flags, hasUnknownVectors)); break;
                case PartType.SetByte: parts.Add(new SetUnknownBytePart(reader)); break;
                case PartType.EnableUnknown2:
                case PartType.EnableUnknown4: hasUnknownVectors = true; break;
                case PartType.DisableUnknown3:
                case PartType.DisableUnknown5: hasUnknownVectors = false; break;
                case PartType.End: break;
                default: throw new NotSupportedException($"Unsupported part type: {type}");
            }
        } while (type != PartType.End);
    }
}

public class SubModel
{
    public int Unknown { get; }
    public IReadOnlyList<SubSubModel> SubSubModels { get; }

    public SubModel(BinaryReader reader)
    {
        Unknown = reader.ReadInt32();
        var count = reader.ReadInt32();
        SubSubModels = ReadArray(reader, count, r => new SubSubModel(r));
    }
}

public class Model
{
    public string Name { get; }
    public float UnknownFloat { get; }
    public IReadOnlyList<SubModel> SubModels { get; }
    public Vector4 UnknownVec4_1 { get; }
    public (Vector3, Vector3) UnknownVecPair_1 { get; }
    public (Vector3, Vector3) UnknownVecPair_2 { get; }
    public bool UnknownFlag0 { get; }
    public bool UnknownFlag1 { get; }
    public bool UnknownFlag2 { get; }
    public bool UnknownFlag3 { get; }
    public uint UnknownInt { get; }
    public EStorable? Storable { get; }

    public Model(BinaryReader reader)
    {
        var zeroBytes = reader.ReadBytes(6);
        if (zeroBytes.Any(b => b != 0))
            throw new InvalidDataException("Expected first six bytes to be zero");

        var nameBytes = new List<byte>();
        while(true)
        {
            var b = reader.ReadByte();
            if (b == 0)
                break;
            nameBytes.Add(b);
        }
        Name = System.Text.Encoding.UTF8.GetString(nameBytes.ToArray());

        if (reader.ReadByte() != 0)
            throw new InvalidDataException("Expected another zero byte");
        UnknownFloat = reader.ReadSingle();

        var subModelCount = reader.ReadInt32();
        SubModels = ReadArray(reader, subModelCount, r => new SubModel(r));

        UnknownVec4_1 = reader.ReadVector4();
        UnknownVecPair_1 = (reader.ReadVector3(), reader.ReadVector3());
        UnknownVecPair_2 = (reader.ReadVector3(), reader.ReadVector3());
        UnknownFlag0 = reader.ReadByte() != 0;
        UnknownFlag1 = reader.ReadByte() != 0;
        UnknownFlag2 = reader.ReadByte() != 0;
        UnknownFlag3 = reader.ReadByte() != 0;
        UnknownInt = reader.ReadUInt32();

        Storable = reader.ReadStorable();
    }
}