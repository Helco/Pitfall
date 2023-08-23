namespace Pitfall.Havok;

public class HavokEnabledCollisionGroup
{
    public string Name { get; }
    public IReadOnlyList<(int From, int To)> Groups { get; }

    internal HavokEnabledCollisionGroup(IHavokReader hkReader)
    {
        Name = hkReader.ReadString();
        var groups = new List<(int, int)>();
        Groups = groups;
        while(true)
        {
            var token = hkReader.ReadToken();
            if (token == "COLLISIONS_ENABLED")
            {
                hkReader.ExpectToken("GROUP");
                int from = hkReader.ReadInt();
                hkReader.ExpectToken("GROUP");
                int to = hkReader.ReadInt();
                groups.Add((from, to));
            }
            else if (token == "END_ENABLED_COLLISION_GROUPS")
                break;
            else
                throw new InvalidDataException($"Unexpected token in enabled collision group {token}");
        }
    }
}
