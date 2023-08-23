namespace Pitfall.Havok;

public enum HavokODESolver
{
    Euler,
    RK45
}

public class HavokRigidBodyCollection : IHavokEntityCollection
{
    public string Name { get; }
    public HavokODESolver ODESolver { get; }
    public float RefreshRate { get; }
    public IReadOnlyList<HavokRigidBody> Bodies { get; }
    public IReadOnlyList<(string From, string To)> NoCollisions { get; }

    internal HavokRigidBodyCollection(IHavokReader hkReader)
    {
        Name = hkReader.ReadString();
        hkReader.ExpectToken("ODE_SOLVER");
        ODESolver = ReadODESolver(hkReader);
        hkReader.ExpectToken("REFRESH_RATE");
        RefreshRate = hkReader.ReadFloat();

        var bodies = new List<HavokRigidBody>();
        var noCollisions = new List<(string, string)>();
        Bodies = bodies;
        NoCollisions = noCollisions;

        HavokToken token;
        while ((token = hkReader.ReadToken()) == "BEGIN_RIGID_BODY")
            bodies.Add(new HavokRigidBody(hkReader));
        if (token == "NO_COLLISION")
        {
            do
            {
                noCollisions.Add((hkReader.ReadString(), hkReader.ReadString()));
            } while ((token = hkReader.ReadToken()) == "NO_COLLISION");
        }
        if (token != "END_COLLECTION")
            throw new InvalidDataException($"Unexpected token in rigid body collection {token}");
    }

    private static HavokODESolver ReadODESolver(IHavokReader hkReader) => hkReader.ReadToken() switch
    {
        var t when t == "Euler" => HavokODESolver.Euler,
        var t when t == "RK45" => HavokODESolver.RK45,
        var t => throw new InvalidDataException($"Invalid ODE solver type {t}")
    };
}
