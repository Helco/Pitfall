using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text;

namespace Pitfall.Havok;

internal readonly struct HavokToken
{
    public readonly string? Text;
    public readonly uint Hash;

    public HavokToken(string text)
    {
        Text = text;
        Hash = HavokHash(text); 
    }

    public HavokToken(uint hash)
    {
        Text = null;
        Hash = hash;
    }

    public static bool operator ==(HavokToken a, HavokToken b) => a.Hash == b.Hash;
    public static bool operator ==(HavokToken token, uint hash) => token.Hash == hash;
    public static bool operator ==(HavokToken token, string text) => token == HavokHash(text);
    public static bool operator !=(HavokToken a, HavokToken b) => !(a == b);
    public static bool operator !=(HavokToken token, uint hash) => !(token == hash);
    public static bool operator !=(HavokToken token, string text) => !(token == text);

    public override bool Equals(object? obj) => obj switch
    {
        HavokToken tk => this == tk,
        string text => this == text,
        uint hash => this == hash,
        _ => false
    };

    public override int GetHashCode() => unchecked((int)Hash);

    public override string ToString() => $"{Text ?? ""}({Hash:X8})";

    private static uint HavokHash(string name)
    {

        var bytes = Encoding.UTF8.GetBytes(name);
        var hash = 0u;
        foreach (var b in bytes)
        {
            hash = (hash << 4) + b;
            var tmp = hash & 0xF000_0000;
            if (tmp != 0)
                hash ^= tmp >> 24;
            hash &= ~tmp;
        }
        return hash % 0x7FFF_FFFF;
    }
}

internal interface IHavokReader
{
    uint Version { get; set; }

    HavokToken ReadToken();
    string ReadString();
    uint ReadUInt();
    int ReadInt();
    float ReadFloat();
    bool ReadBoolean();

    Vector3 ReadVector3() => new(ReadFloat(), ReadFloat(), ReadFloat());
    Quaternion ReadAngleAxis()
    {
        float angle = ReadFloat();
        return Quaternion.CreateFromAxisAngle(ReadVector3(), angle);
    }

    void ExpectToken(string tokenText, HavokToken? actualOpt = null)
    {
        var expect = new HavokToken(tokenText);
        var actual = actualOpt ?? ReadToken();
        if (actual != expect)
            throw new InvalidDataException($"Expected {expect} but got {actual}");
    }
}

internal class BinaryHavokReader : IHavokReader
{
    private readonly BinaryReader binReader;
    public uint Version { get; set; }

    public BinaryHavokReader(BinaryReader binReader) => this.binReader = binReader;

    public uint ReadUInt() => binReader.ReadUInt32();
    public int ReadInt() => binReader.ReadInt32();
    public float ReadFloat() => binReader.ReadSingle();
    public string ReadString() => binReader.ReadCString();

    public HavokToken ReadToken()
    {
        var firstHash = ReadUInt();
        // this is original and would mark the token, but I do not know how this marking would be used
        if (firstHash == 0x12ABCDEF)
            return new(ReadUInt());
        return new(firstHash);
    }

    public bool ReadBoolean()
    {
        // weird I know...
        byte b = binReader.ReadByte();
        while (b == ' ' || b == '\t' || b == '\r' || b == '\n')
            b = binReader.ReadByte();
        return b != 0;
    }
}
