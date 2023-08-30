using System;
using System.IO;
using System.Text;
using static System.Buffers.Binary.BinaryPrimitives;

namespace Pitfall;

public class BinaryReaderBE : BinaryReader
{
    public BinaryReaderBE(Stream input) : base(input)
    {
    }

    public BinaryReaderBE(Stream input, Encoding encoding) : base(input, encoding)
    {
    }

    public BinaryReaderBE(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
    {
    }

    private static unsafe float ReverseEndiannessFloat(float value)
    {
        uint* valuePtr = (uint*)&value;
        *valuePtr = ReverseEndianness(*valuePtr);
        return value;
    }

    private static unsafe double ReverseEndiannessFloat(double value)
    {
        ulong* valuePtr = (ulong*)&value;
        *valuePtr = ReverseEndianness(*valuePtr);
        return value;
    }

    public override short ReadInt16() => ReverseEndianness(base.ReadInt16());
    public override ushort ReadUInt16() => ReverseEndianness(base.ReadUInt16());
    public override int ReadInt32() => ReverseEndianness(base.ReadInt32());
    public override uint ReadUInt32() => ReverseEndianness(base.ReadUInt32());
    public override long ReadInt64() => ReverseEndianness(base.ReadInt64());
    public override ulong ReadUInt64() => ReverseEndianness(base.ReadUInt64());
    public override float ReadSingle() => ReverseEndiannessFloat(base.ReadSingle());
    public override double ReadDouble() => ReverseEndiannessFloat(base.ReadDouble());

    public override decimal ReadDecimal() =>
        throw new NotImplementedException($"I did not implement big-endian decimal reading, is it necessary?");
}
