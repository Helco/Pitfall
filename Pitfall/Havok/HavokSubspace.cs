using System.Numerics;

namespace Pitfall.Havok;

public interface IHavokResolver { }
public interface IHavokEntityCollection { }
public interface IHavokDeflector { }

public class HavokSubspace
{
    public string Name { get; }
    public Vector3 Gravity { get; }
    public float Tolerance { get; }
    public bool HasDeactivator { get; }
    public float DeactivatorShortFrequency { get; }
    public float DeactivatorLongFrequency { get; }
    public IHavokResolver Resolver { get; }
    public IReadOnlyList<IHavokEntityCollection> Collections { get; }
    public IReadOnlyList<HavokDisabledCollisionList> DisabledCollisionLists { get; }
    public IReadOnlyList<HavokEnabledCollisionGroup> EnabledCollisionGroups { get; }
    public IReadOnlyList<IHavokAction> Actions { get; }
    public IReadOnlyList<IHavokDeflector> Deflectors { get; }

    internal HavokSubspace(IHavokReader hkReader, HavokWorld world)
    {
        Name = hkReader.ReadString();
        hkReader.ExpectToken("GRAVITY");
        Gravity = hkReader.ReadVector3();
        hkReader.ExpectToken("TOLERANCE");
        Tolerance = hkReader.ReadFloat();
        if (hkReader.Version >= 1340)
        {
            hkReader.ExpectToken("HAS_DEACTIVATOR");
            HasDeactivator = hkReader.ReadBoolean();
            if (HasDeactivator)
            {
                hkReader.ExpectToken("DEACTIVATOR_SHORT_FREQUENCY");
                DeactivatorShortFrequency = hkReader.ReadFloat();
                hkReader.ExpectToken("DEACTIVATOR_LONG_FREQUENCY");
                DeactivatorLongFrequency = hkReader.ReadFloat();
            }
        }
        hkReader.ExpectToken("RESOLVER");
        Resolver = ReadResolver(hkReader);

        var collections = new List<IHavokEntityCollection>();
        var disabledCollisionLists = new List<HavokDisabledCollisionList>();
        var enabledCollisionGroups = new List<HavokEnabledCollisionGroup>();
        var actions = new List<IHavokAction>();
        var deflectors = new List<IHavokDeflector>();
        Collections = collections;
        DisabledCollisionLists = disabledCollisionLists;
        EnabledCollisionGroups = enabledCollisionGroups;
        Actions = actions;
        Deflectors = deflectors;

        while(true)
        {
            var token = hkReader.ReadToken();
            if (token == "BEGIN_COLLECTION")
                collections.Add(ReadCollection(hkReader, world));
            else if (token == "NO_COLLISION")
            {
                // this is also ignored in original
                hkReader.ReadString();
                hkReader.ReadString();
            }
            else if (token == "BEGIN_DISABLED_COLLISIONS_LIST")
                disabledCollisionLists.Add(new HavokDisabledCollisionList(hkReader));
            else if (token == "BEGIN_ENABLED_COLLISION_GROUPS")
                enabledCollisionGroups.Add(new HavokEnabledCollisionGroup(hkReader));
            else if (token == "BEGIN_ACTION")
                actions.Add(ReadAction(hkReader));
            else if (token == "BEGIN_DEFLECTOR")
                deflectors.Add(ReadDeflector(hkReader));
            else if (token == "END_SUBSPACE")
                break;
            else
                throw new InvalidDataException($"Unexpected token in subspace {token}");
        }
    }

    private static IHavokResolver ReadResolver(IHavokReader hkReader) => hkReader.ReadToken() switch
    {
        var t when t == "ComplexFriction" => new HavokComplexFrictionResolver(hkReader),
        var t => throw new InvalidDataException($"Invalid resolver type {t}")
    };

    private static IHavokEntityCollection ReadCollection(IHavokReader hkReader, HavokWorld world) => hkReader.ReadToken() switch
    {
        var t when t == "RBCollection" => new HavokRigidBodyCollection(hkReader),
        var t when t == "FastClothCollection" => new HavokFastClothCollection(hkReader, world),
        var t when t == "FastSoftCollection" => throw new NotSupportedException($"{t} is not yet supported"),
        var t when t == "FastRopeCollection" => new HavokFastRopeCollection(hkReader, world),
        var t => throw new InvalidDataException($"Invalid entity collection type {t}")
    };

    private static IHavokAction ReadAction(IHavokReader hkReader) => hkReader.ReadToken() switch
    {
        var t when t == "Deactivator" => throw new NotSupportedException($"{t} is not yet supported"),
        var t when t == "Spring" => throw new NotSupportedException($"{t} is not yet supported"),
        var t when t == "Drag" => new HavokDragAction(hkReader),
        var t when t == "Dashpot" => throw new NotSupportedException($"{t} is not yet supported"),
        var t when t == "AngularDashpot" => throw new NotSupportedException($"{t} is not yet supported"),
        var t when t == "Motor" => throw new NotSupportedException($"{t} is not yet supported"),
        var t when t == "FastConstraintSolver" => new HavokFastConstraintSolver(hkReader),
        var t when t == "KeyFrame" => throw new NotSupportedException($"{t} is not yet supported"),
        var t => throw new InvalidDataException($"Invalid action type {t}")
    };

    private static IHavokDeflector ReadDeflector(IHavokReader hkReader) => hkReader.ReadToken() switch
    {
        var t when t == "CylinderDeflector" => throw new NotSupportedException($"{t} is not yet supported"),
        var t when t == "PlaneDeflector" => throw new NotSupportedException($"{t} is not yet supported"),
        var t => throw new InvalidDataException($"Invalid deflector type {t}")
    };
}
