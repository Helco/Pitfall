using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pitfall.Storables;

[StorableType]
public class ERDataset : EResource
{
    // only implementing mode 0,
    // for mode 1 we would need the compression scheme
    // for mode 2 we would need to read all contents correctly
    // neither seems to be necessary for pc.

    public class FileSet
    {
        public string Name { get; init; } = "";
        public IReadOnlyList<(uint ID, byte[] Content)> Files { get; init; } = Array.Empty<(uint, byte[])>();
    }

    public IReadOnlyList<FileSet> FileSets { get; private set; } = Array.Empty<FileSet>();

    public ERDataset(BinaryReader reader)
    {
        Read(reader);
    }

    public override void Read(BinaryReader reader)
    {
        var uncompressedSize = reader.ReadUInt32();
        var mode = reader.ReadByte();
        var compressedSize = reader.ReadUInt32();
        var setCount = reader.ReadUInt32();
        base.Read(reader);
        if (mode != 0)
            throw new NotSupportedException($"Unsupported ERDataset mode: {mode}");

        FileSets = reader.ReadArray((int)setCount, ReadFileSet);
    }

    private static FileSet ReadFileSet(BinaryReader reader)
    {
        var name = reader.ReadCString();
        var files = reader.ReadArray(ReadFileMode0);
        return new FileSet { Name = name, Files = files };
    }

    private static (uint, byte[]) ReadFileMode0(BinaryReader reader)
    {
        var id = reader.ReadUInt32();
        var size = reader.ReadInt32();
        var skipBytes = reader.ReadInt32();
        reader.BaseStream.Position += skipBytes;
        return (id, reader.ReadBytes(size));
    }
}
