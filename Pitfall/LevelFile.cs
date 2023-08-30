using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pitfall.Storables;

namespace Pitfall;

// A .level file contains an ERLevel but also much more

public class Level
{
    public ERLevel LevelData { get; }
    public byte[] UnknownData { get; } // seems like a rather simple file format containing a BSP
    public EHavokWorld? HavokWorld { get; }
    public EStorable?[] Instances { get; }

    public Level(BinaryReader reader)
    {
        reader.ReadUInt32(); // unused int
        LevelData = reader.ExpectStorable<ERLevel>();
        UnknownData = reader.ReadBytes(reader.ReadInt32());
        HavokWorld = reader.ReadStorable<EHavokWorld>();
        Instances = reader.ReadInstanceArray();
    }
}
