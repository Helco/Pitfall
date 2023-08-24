using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pitfall.Storables;

[StorableType]
public class EIPlant : EIStaticModel
{
    public Vector3 plantPos;
    public Quaternion plantRot;
    public Vector3 plantScale;
    public (string, string) plantStrPair;

    public override void ReadInstanceData(BinaryReader reader)
    {
        plantPos = reader.ReadVector3();
        plantRot = reader.ReadQuaternion();
        plantScale = reader.ReadVector3();
        plantStrPair = (reader.ReadCString(), reader.ReadCString());
    }
}
