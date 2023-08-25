using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Pitfall.Storables;

[StorableType]
public class EIHavokBreakable : EIDynamicModel
{
    public Vector3 havokPos;
    public Quaternion havokRot;
    public (string, string) havokDebugInfo = ("", "");
    public string havokStr = "";

    public override void ReadInstanceData(BinaryReader reader)
    {
        havokPos = reader.ReadVector3();
        havokRot = reader.ReadQuaternion();
        havokDebugInfo = (reader.ReadCString(), reader.ReadCString());
        havokStr = reader.ReadCString();
    }
}
