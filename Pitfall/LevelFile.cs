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
    public ERLevel storableLevel;
    public byte[] unknownData; // seems like a rather simple file format containing a BSP
    public EHavokWorld? havokWorld;
    public EStorable?[] extendables;

    public Level(BinaryReader reader)
    {
        reader.ReadUInt32(); // unused int
        var storable = reader.ReadStorable();
        if (storable is not ERLevel level)
            throw new InvalidDataException($"Expected an ERLevel but got {storable?.GetType()?.Name ?? "<null>"}");
        storableLevel = level;
        unknownData = reader.ReadArray<byte>(reader.ReadInt32(), 1);
        var havokWorldStorable = reader.ReadStorable();
        havokWorld = havokWorldStorable as EHavokWorld;
        if (havokWorldStorable is not null && havokWorld is null)
            throw new InvalidDataException($"Expected a EHavokWorld but got {havokWorldStorable.GetType()?.Name ?? "<null>"}");
        extendables = reader.ReadInstanceArray();
    }
}
