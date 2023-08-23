using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Pitfall.Havok;

public class HavokInlineGeometry : IHavokGeometry
{
    public string Name { get; }
    public int VertexCount => Vertices.Count;
    public IReadOnlyList<Vector3> Vertices { get; }
    public IReadOnlyList<ushort> Indices { get; } // triangles

    internal HavokInlineGeometry(IHavokReader hkReader)
    {
        Name = hkReader.ReadString();
        var vertices = new Vector3[hkReader.ReadUInt()];
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] = hkReader.ReadVector3();
        Vertices = vertices;
        var indices = new ushort[3 * hkReader.ReadUInt()];
        for (int i = 0; i < indices.Length; i++)
            indices[i] = (ushort)hkReader.ReadUInt();
        Indices = indices;
        hkReader.ExpectToken("END_GEOMETRY");
    }
}
