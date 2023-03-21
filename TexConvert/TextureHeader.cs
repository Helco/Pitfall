﻿namespace TexConvert;

readonly struct TextureHeader
{
    public string Name { get; init; }
    public ushort FormatId { get; init; }
    public ushort Width { get; init; }
    public ushort Height { get; init; }
    public uint Flags { get; init; }
    public bool HasMipmaps => (Flags & 0x20) > 0;
    public uint Mipmaps { get; init; }

    public TextureHeader(EndianReader reader)
    {
        var magic = reader.ReadUInt32();
        if (magic == 0x5458464C)
            reader.IsLittleEndian = !reader.IsLittleEndian;
        else if (magic != 0x4C465854)
            throw new Exception("Invalid magic");

        Name = reader.ReadCString();
        FormatId = reader.ReadUInt16();
        Width = reader.ReadUInt16();
        Height = reader.ReadUInt16();
        reader.Skip(4);
        Flags = reader.ReadUInt32();
        Mipmaps = reader.ReadUInt32();
    }
}
