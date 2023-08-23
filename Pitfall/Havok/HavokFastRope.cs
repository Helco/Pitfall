using System.Numerics;

namespace Pitfall.Havok;

public class HavokFastRope : HavokDeformableBody
{
    public enum RopeType
    {
        Spring,
        Constraint
    }

    public string Name { get; }
    public float Thickness { get; }
    public RopeType Type { get; }
    public float SpringStiffness { get; }
    public bool SpringSetBendStiffness { get; }
    public float SpringBendStiffness { get; }
    public uint SpringWeave { get; }
    public bool SpringKeepCurves { get; }
    public IReadOnlyList<Vector3> Points { get; }

    internal HavokFastRope(IHavokReader hkReader)
    {
        Name = hkReader.ReadString();
        hkReader.ExpectToken("THICKNESS");
        Thickness = hkReader.ReadFloat();
        hkReader.ExpectToken("TYPE");
        Type = ReadRopeType(hkReader);

        if (Type == RopeType.Spring)
        {
            hkReader.ExpectToken("STIFFNESS");
            SpringStiffness = hkReader.ReadFloat();
            hkReader.ExpectToken("SETBENDSTIFFNESS");
            if (hkReader.ReadToken() == "true")
            {
                hkReader.ExpectToken("BENDSTIFFNESS");
                SpringBendStiffness = hkReader.ReadFloat();
                hkReader.ExpectToken("WEAVE");
                SpringWeave = hkReader.ReadUInt();
                hkReader.ExpectToken("KEEP_CURVES");
                SpringKeepCurves = hkReader.ReadBoolean();
            }
        }

        hkReader.ExpectToken("NUM_POINTS");
        var points = new Vector3[hkReader.ReadUInt()];
        for (int i = 0; i < points.Length; i++)
            points[i] = hkReader.ReadVector3();
        Points = points;

        var token = ReadDeformable(hkReader, Points.Count);
        hkReader.ExpectToken("END_FASTROPE", token);
    }

    private static RopeType ReadRopeType(IHavokReader hkReader) => hkReader.ReadToken() switch
    {
        var t when t == "SPRING" => RopeType.Spring,
        var t when t == "CONSTRAINT" => RopeType.Constraint,
        var t => throw new InvalidDataException($"Invalid rope type {t}")
    };
}
