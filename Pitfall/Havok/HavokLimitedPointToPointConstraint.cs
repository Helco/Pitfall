using System.Numerics;

namespace Pitfall.Havok;

public class HavokLimitedPointToPointConstraint : IHavokConstraint
{
    public string Name { get; }
    public string RigidBodyA { get; }
    public string? RigidBodyB { get; }
    public Vector3 LocalPosA { get; }
    public Vector3 LocalPosB { get; }
    public Quaternion LocalRotA { get; }
    public Quaternion LocalRotB { get; }
    public float TwistMin { get; }
    public float TwistMax { get; }
    public float ParallelMin { get; }
    public float ParallelMax { get; }
    public float PerpendicularMin { get; }
    public float PerpendicularMax { get; }
    public bool IsBreakable { get; }
    public float LinearStrength { get; }
    public float AngularStrength { get; }
    public float Strength { get; }
    public float Tau { get; }

    internal HavokLimitedPointToPointConstraint(IHavokReader hkReader)
    {
        Name = hkReader.ReadString();
        bool twoBodied = true;
        if (hkReader.Version >= 1330)
        {
            hkReader.ExpectToken("TWO_BODIED");
            twoBodied = hkReader.ReadBoolean();
        }
        hkReader.ExpectToken("RIGID_BODY_A");
        RigidBodyA = hkReader.ReadString();
        hkReader.ExpectToken("LOCAL_POS_A");
        LocalPosA = hkReader.ReadVector3();
        hkReader.ExpectToken("LOCAL_ROT_A");
        LocalRotA = hkReader.ReadAngleAxis();
        if (twoBodied)
        {
            hkReader.ExpectToken("RIGID_BODY_B");
            RigidBodyB = hkReader.ReadString();
        }
        hkReader.ExpectToken("LOCAL_POS_B");
        LocalPosB = hkReader.ReadVector3();
        hkReader.ExpectToken("LOCAL_ROT_B");
        LocalRotB = hkReader.ReadAngleAxis();
        hkReader.ExpectToken("TWIST_MIN");
        TwistMin = hkReader.ReadFloat();
        hkReader.ExpectToken("TWIST_MAX");
        TwistMax = hkReader.ReadFloat();
        hkReader.ExpectToken("PARALLEL_MIN");
        ParallelMin = hkReader.ReadFloat();
        hkReader.ExpectToken("PARALLEL_MAX");
        ParallelMax = hkReader.ReadFloat();
        hkReader.ExpectToken("PERPENDICULAR_MIN");
        PerpendicularMin = hkReader.ReadFloat();
        hkReader.ExpectToken("PERPENDICULAR_MAX");
        PerpendicularMax = hkReader.ReadFloat();
        if (hkReader.Version >= 1370)
        {
            hkReader.ExpectToken("IS_BREAKABLE");
            IsBreakable = hkReader.ReadBoolean();
            if (IsBreakable)
            {
                hkReader.ExpectToken("LINEAR_STRENGTH");
                LinearStrength = hkReader.ReadFloat();
                hkReader.ExpectToken("ANGULAR_STRENGTH");
                AngularStrength = hkReader.ReadFloat();
            }
            hkReader.ExpectToken("STRENGTH");
            Strength = hkReader.ReadFloat();
            hkReader.ExpectToken("TAU");
            Tau = hkReader.ReadFloat();
        }
        hkReader.ExpectToken("END_CONSTRAINT");
    }
}
