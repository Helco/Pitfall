namespace Pitfall.Havok;

public class HavokDisplayBody
{
    public string Name { get; }
    public bool CastsShadows { get; }

    internal HavokDisplayBody(IHavokReader hkReader)
    {
        Name = hkReader.ReadString();
        if (hkReader.Version >= 160)
        {
            hkReader.ExpectToken("CASTS_SHADOWS");
            CastsShadows = hkReader.ReadBoolean();
        }
    }
}
