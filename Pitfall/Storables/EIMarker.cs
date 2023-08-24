using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pitfall.Storables;

[StorableType]
public class EIMarker : EIGameInstance
{
    public Vector3 markerV1 { get; private set; }
    public Vector4 markerV2 { get; private set; }
    public uint markerI1 { get; private set; }
    public uint markerI2 { get; private set; }

    public override void ReadInstanceData(BinaryReader reader)
    {
        markerV1 = reader.ReadVector3();
        markerV2 = reader.ReadVector4();
        markerI1 = reader.ReadUInt32();
        markerI2 = reader.ReadUInt32();
    }
}
