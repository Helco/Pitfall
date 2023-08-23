namespace Pitfall.Havok;

public class HavokDisabledCollisionList
{
    public enum SpecifierType
    {
        RigidBody,
        Primitive
    }
    
    public readonly record struct Specifier(string Name, SpecifierType Type);
    public readonly record struct Entry(Specifier From, Specifier To);

    public string Name { get; }
    public IReadOnlyList<Entry> Entries { get; }

    internal HavokDisabledCollisionList(IHavokReader hkReader)
    {
        Name = hkReader.ReadString();
        var entries = new List<Entry>();
        Entries = entries;
        while(true)
        {
            var token = hkReader.ReadToken();
            if (token == "NO_COLLISION")
                entries.Add(new(ReadSpecifier(hkReader), ReadSpecifier(hkReader)));
            else if (token == "END_DISABLED_COLLISIONS_LIST")
                break;
            else
                throw new InvalidDataException($"Unexpected token in disabled collision list {token}");
        }
    }

    private static Specifier ReadSpecifier(IHavokReader hkReader) =>
        new(hkReader.ReadString(), ReadSpecifierType(hkReader));

    private static SpecifierType ReadSpecifierType(IHavokReader hkReader) => hkReader.ReadToken() switch
    {
        var t when t == "RIGID_BODY" => SpecifierType.RigidBody,
        var t when t == "PRIMITIVE" => SpecifierType.Primitive,
        var t => throw new InvalidDataException($"Invalid disabled collision list specifier type {t}")
    };
}
