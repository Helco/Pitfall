using System.Numerics;

namespace Pitfall.Havok;

public class HavokMarker
{
    public string Name { get; }
    public Vector3 Translation { get; }
    public Quaternion Rotation { get; }

    internal HavokMarker(IHavokReader hkReader)
    {
        Name = hkReader.ReadString();
        Translation = hkReader.ReadVector3();
        Rotation = hkReader.ReadAngleAxis();
    }
}

public class HavokMarkerList
{
    public string Name { get; }
    public IReadOnlyList<HavokMarker> Markers { get; }

    internal HavokMarkerList(IHavokReader hkReader)
    {
        Name = hkReader.ReadString();
        var markers = new List<HavokMarker>();
        Markers = markers;
        while (true)
        {
            var token = hkReader.ReadToken();
            if (token == "BEGIN_MARKER")
                markers.Add(new(hkReader));
            else if (token == "END_MARKER_LIST")
                break;
            else
                throw new InvalidDataException($"Unexpected token in marker list {token}");
        }
    }
}
