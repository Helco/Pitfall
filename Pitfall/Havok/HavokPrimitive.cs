using System.Numerics;

namespace Pitfall.Havok;

public abstract class HavokPrimitive
{
    public string Name { get; }
    public float Mass { get; }
    public Quaternion Rotation { get; }
    public Vector3 Translation { get; }
    public uint CollisionMask { get; }
    public bool CollisionsDisabled { get; private set; }

    internal HavokPrimitive(IHavokReader hkReader)
    {
        Name = hkReader.ReadString();
        hkReader.ExpectToken("MASS");
        Mass = hkReader.ReadFloat();
        hkReader.ExpectToken("ROTATION");
        Rotation = hkReader.ReadAngleAxis();
        hkReader.ExpectToken("TRANSLATION");
        Translation = hkReader.ReadVector3();
        if (hkReader.Version >= 1350)
        {
            hkReader.ExpectToken("COLLISION_MASK");
            CollisionMask = hkReader.ReadUInt();
        }
    }

    internal void FinishReading(IHavokReader hkReader)
    {
        hkReader.ExpectToken("COLLISIONS_DISABLED");
        CollisionsDisabled = hkReader.ReadBoolean();
        hkReader.ExpectToken("END_PRIMITIVE");
    }
}

public class HavokGeometricPrimitive : HavokPrimitive
{
    public string Geometry { get; }
    public bool Convex { get; }

    internal HavokGeometricPrimitive(IHavokReader hkReader) : base(hkReader)
    {
        hkReader.ExpectToken("GEOMETRY");
        Geometry = hkReader.ReadString();
        if (hkReader.Version >= 140)
        {
            hkReader.ExpectToken("CONVEX");
            Convex = hkReader.ReadBoolean();
        }
        FinishReading(hkReader);
    }
}

public class HavokPlanarPrimitive : HavokPrimitive
{
    public Vector3 Normal { get; }
    public float Distance { get; }

    internal HavokPlanarPrimitive(IHavokReader hkReader) : base(hkReader)
    {
        hkReader.ExpectToken("PLANE");
        Normal = hkReader.ReadVector3();
        Distance = hkReader.ReadFloat();
        FinishReading(hkReader);
    }
}

public class HavokSphericalPrimitive : HavokPrimitive
{
    public float Radius { get; }

    internal HavokSphericalPrimitive(IHavokReader hkReader) : base(hkReader)
    {
        hkReader.ExpectToken("RADIUS");
        Radius = hkReader.ReadFloat();
        FinishReading(hkReader);
    }
}
