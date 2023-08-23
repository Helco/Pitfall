using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pitfall.Havok;

public class HavokFastCloth : HavokDeformableBody
{
    public string Name { get; }
    public string Geometry { get; }
    public Vector3 Translation { get; }
    public Quaternion Quaternion { get; }
    public float Stiffness { get; }
    public float BendStiffness { get; }

    internal HavokFastCloth(IHavokReader hkReader, HavokWorld world)
    {
        Name = hkReader.ReadString();
        hkReader.ExpectToken("GEOMETRY");
        Geometry = hkReader.ReadString();
        hkReader.ExpectToken("TRANSLATION");
        Translation = hkReader.ReadVector3();
        hkReader.ExpectToken("ROTATION");
        Quaternion = hkReader.ReadAngleAxis();
        hkReader.ExpectToken("STIFFNESS");
        Stiffness = hkReader.ReadFloat();
        hkReader.ExpectToken("BENDSTIFFNESS");
        BendStiffness = hkReader.ReadFloat();

        int vertexCount = world.GetGeometryVertexCount(Geometry);
        var token = ReadDeformable(hkReader, vertexCount);
        hkReader.ExpectToken("END_FASTCLOTH", token);
    }
}
