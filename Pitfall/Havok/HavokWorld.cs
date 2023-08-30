using System;
using System.Collections.Generic;
using System.Linq;

namespace Pitfall.Havok;

public interface IHavokGeometry
{
    string Name { get; }
    int VertexCount { get; }
}

public partial class HavokWorld
{
    public IReadOnlyList<byte[]> EmbeddedFiles { get; }
    public string Name { get; }
    public uint Version { get; }
    public float WorldScale { get; }
    //public HavokDisplayWorld? DisplayWorld { get; }
    public bool FastSubspace { get; }
    public IReadOnlyList<HavokSubspace> Subspaces { get; }
    public IReadOnlyList<IHavokGeometry> Geometries { get; }
    public IReadOnlyList<HavokMarkerList> MarkerLists { get; }

    public HavokWorld(BinaryReader binReader)
    {
        if (binReader.ReadUInt32() != 0x1765ABC2)
            throw new InvalidDataException("Invalid HavokWorld magic");
        EmbeddedFiles = binReader.ReadArray(() => binReader.ReadBytes(binReader.ReadInt32()));
        var hkReader = binReader.ReadByte() switch
        {
            (byte)'A' => throw new NotSupportedException("ASCII Havok files are not yet supported"),
            (byte)'B' => new BinaryHavokReader(binReader) as IHavokReader,
            _ => throw new InvalidDataException("Invalid Havok format specifier")
        };

        hkReader.ExpectToken("BEGIN_WORLD");
        Name = hkReader.ReadString();
        hkReader.ExpectToken("VERSION");
        Version = hkReader.Version = hkReader.ReadUInt();
        hkReader.ExpectToken("WORLD_SCALE");
        WorldScale = hkReader.ReadFloat();

        var token = hkReader.ReadToken();
        if (token == "BEGIN_DISPLAY_WORLD")
        {
            throw new NotSupportedException("HavokDisplayWorld is not yet supported");
            //DisplayWorld = new HavokDisplayWorld(hkReader);
            //token = hkReader.ReadToken();
        }

        var subspaces = new List<HavokSubspace>();
        var geometries = new List<IHavokGeometry>();
        var markerLists = new List<HavokMarkerList>();
        Subspaces = subspaces;
        Geometries = geometries;
        MarkerLists = markerLists;

        while(true)
        {
            if (token == "FAST_SUBSPACE")
                FastSubspace = hkReader.ReadBoolean();
            else if (token == "BEGIN_SUBSPACE")
                subspaces.Add(new HavokSubspace(hkReader, this));
            else if (token == "BEGIN_GEOMETRY")
                geometries.Add(ReadGeometry(hkReader));
            else if (token == "BEGIN_MARKER_LIST")
                markerLists.Add(new HavokMarkerList(hkReader));
            else if (token == "END_WORLD")
                break;
            else
                throw new InvalidDataException($"Unexpected token in world {token}");
            token = hkReader.ReadToken();
        }
    }

    internal int GetGeometryVertexCount(string name) =>
        Geometries.FirstOrDefault(g => g.Name == name)?.VertexCount
        ?? throw new InvalidDataException($"Geometry {name} is not defined before queried");

    private static IHavokGeometry ReadGeometry(IHavokReader hkReader) => hkReader.ReadToken() switch
    {
        var t when t == "Inline" => new HavokInlineGeometry(hkReader),
        var t when t == "TKFile" => throw new NotSupportedException("TKFile geometries are not yet supported"),
        var t => throw new InvalidDataException($"Invalid geometry type {t}")
    };
}
