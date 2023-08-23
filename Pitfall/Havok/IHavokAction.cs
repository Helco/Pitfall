using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pitfall.Havok;

public interface IHavokAction { }
public interface IHavokConstraint { }

public class HavokDragAction : IHavokAction
{
    public string Name { get; }
    public float LinearDrag { get; }
    public float AngularDrag { get; }

    internal HavokDragAction(IHavokReader hkReader)
    {
        Name = hkReader.ReadString();
        hkReader.ExpectToken("LINEAR_DRAG");
        LinearDrag = hkReader.ReadFloat();
        hkReader.ExpectToken("ANGULAR_DRAG");
        AngularDrag = hkReader.ReadFloat();
        hkReader.ExpectToken("END_ACTION");
    }
}

public class HavokFastConstraintSolver : IHavokAction
{
    public string Name { get; }
    public IReadOnlyList<IHavokConstraint> Constraints { get; }
    public string RigidBodyCollection { get; }
    public float? DeactivationThreshold { get; }

    internal HavokFastConstraintSolver(IHavokReader hkReader)
    {
        Name = hkReader.ReadString();
        var constraints = new List<IHavokConstraint>();
        Constraints = constraints;

        HavokToken token;
        while ((token = hkReader.ReadToken()) == "BEGIN_CONSTRAINT")
            constraints.Add(ReadConstraint(hkReader));

        hkReader.ExpectToken("RB_COLLECTION", token);
        RigidBodyCollection = hkReader.ReadString();
        if (hkReader.Version >= 1340)
        {
            hkReader.ExpectToken("DEACTIVATION_THRESHOLD");
            DeactivationThreshold = hkReader.ReadFloat();
        }
        hkReader.ExpectToken("END_ACTION");
    }

    private static IHavokConstraint ReadConstraint(IHavokReader hkReader) => hkReader.ReadToken() switch
    {
        var t when t == "PointToPoint" => throw new NotSupportedException($"{t} is not yet supported"),
        var t when t == "FatPointToPoint" => throw new NotSupportedException($"{t} is not yet supported"),
        var t when t == "Ragdoll" => throw new NotSupportedException($"{t} is not yet supported"),
        var t when t == "Hinge" => throw new NotSupportedException($"{t} is not yet supported"),
        var t when t == "Carwheel" => throw new NotSupportedException($"{t} is not yet supported"),
        var t when t == "LimitedPointToPoint" => new HavokLimitedPointToPointConstraint(hkReader),
        var t when t == "StiffSpring" => throw new NotSupportedException($"{t} is not yet supported"),
        var t => throw new InvalidDataException($"Invalid constraint type {t}")
    };
}
