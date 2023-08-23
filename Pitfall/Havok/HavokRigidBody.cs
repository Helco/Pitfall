using System.Numerics;

namespace Pitfall.Havok;

public class HavokRigidBody
{
    public string Name { get; }
    public float Ellasticity { get; }
    public float StaticFriction { get; }
    public float DynamicFriction { get; }
    public Quaternion Rotation { get; }
    public Vector3 Translation { get; }
    public Vector3 LinearVelocity { get; }
    public Vector3 AngularVelocity { get; }
    public bool CollisionsDisabled { get; }
    public bool Active { get; }
    public Vector3 Displacement { get; }
    public HavokDisplayBody? DisplayBody { get; }
    public IReadOnlyList<HavokPrimitive> Primitives { get; }

    internal HavokRigidBody(IHavokReader hkReader)
    {
        Name = hkReader.ReadString();
        hkReader.ExpectToken("ELLASTICITY");
        Ellasticity = hkReader.ReadFloat();
        hkReader.ExpectToken("STATIC_FRICTION");
        StaticFriction = hkReader.ReadFloat();
        hkReader.ExpectToken("DYNAMIC_FRICTION");
        DynamicFriction = hkReader.ReadFloat();
        hkReader.ExpectToken("ROTATION");
        Rotation = hkReader.ReadAngleAxis();
        hkReader.ExpectToken("TRANSLATION");
        Translation = hkReader.ReadVector3();
        hkReader.ExpectToken("LINEAR_VELOCITY");
        LinearVelocity = hkReader.ReadVector3();
        hkReader.ExpectToken("ANGULAR_VELOCITY");
        AngularVelocity = hkReader.ReadVector3();
        hkReader.ExpectToken("COLLISIONS_DISABLED");
        CollisionsDisabled = hkReader.ReadBoolean();
        hkReader.ExpectToken("ACTIVE");
        Active = hkReader.ReadBoolean();
        hkReader.ExpectToken("DISPLACEMENT");
        Displacement = hkReader.ReadVector3();

        var token = hkReader.ReadToken();
        if (token == "DISPLAY_BODY")
        {
            DisplayBody = new HavokDisplayBody(hkReader);
            token = hkReader.ReadToken();
        }

        var primitives = new List<HavokPrimitive>();
        Primitives = primitives;
        while (true)
        {
            if (token == "BEGIN_PRIMITIVE")
                primitives.Add(ReadPrimitive(hkReader));
            else if (token == "END_RIGID_BODY")
                break;
            else
                throw new InvalidDataException($"Unexpected token in rigid body {token}");
            token = hkReader.ReadToken();
        }
    }

    private static HavokPrimitive ReadPrimitive(IHavokReader hkReader) => hkReader.ReadToken() switch
    {
        var t when t == "GeometricPrimitive" => new HavokGeometricPrimitive(hkReader),
        var t when t == "PlanarPrimitive" => new HavokPlanarPrimitive(hkReader),
        var t when t == "SphericalPrimitive" => new HavokSphericalPrimitive(hkReader),
        var t => throw new InvalidDataException($"Invalid primitive type {t}")
    };
}
