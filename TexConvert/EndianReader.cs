using System;
using System.Text;
using System.IO;

namespace TexConvert;

internal class EndianReader : IDisposable
{
    private bool disposedValue;
    private readonly BinaryReader reader;

    public bool IsLittleEndian { get; set; }

    public EndianReader(Stream stream, bool leaveOpen = false)
    {
        reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen);
        IsLittleEndian = BitConverter.IsLittleEndian;
    }

    public void Skip(int bytes) => reader.BaseStream.Seek(bytes, SeekOrigin.Current);
    public byte ReadByte() => reader.ReadByte();
    public byte[] ReadBytes(int bytes) => reader.ReadBytes(bytes);
    public ushort ReadUInt16() => Swap(reader.ReadUInt16());
    public uint ReadUInt32() => Swap(reader.ReadUInt32());

    public string ReadCString()
    {
        var bytes = new List<byte>(32);
        while(true)
        {
            var b = reader.ReadByte();
            if (b == 0)
                break;
            bytes.Add(b);
        }
        return Encoding.UTF8.GetString(bytes.ToArray());
    }

    private ushort Swap(ushort raw)
    {
        if (IsLittleEndian == BitConverter.IsLittleEndian)
            return raw;
        return (ushort)((raw << 8) | (raw >> 8));
    }

    private uint Swap(uint raw)
    {
        if (IsLittleEndian == BitConverter.IsLittleEndian)
            return raw;
        raw = (uint)((raw >> 16) | (raw << 16));
        raw = ((raw & 0x00FF00FF) << 8) | ((raw & 0xFF00FF00) >> 8);
        return raw;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                reader.Dispose();
            }
            disposedValue = true;
        }
    }
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
