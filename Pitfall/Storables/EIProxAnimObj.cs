using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pitfall.Storables;

[StorableType]
public class EIProxAnimObj : EIAnimLod
{
    public uint id1, modelID, characterID, animationID;
    public float f1;
    public uint i2, i3, i4;
    public Vector3 pos, scale;
    public Quaternion rot;
    public StringPair debugInfo;

    public override void ReadInstanceData(BinaryReader reader)
    {
        id1 = reader.ReadUInt32();
        modelID = reader.ReadUInt32();
        rot = reader.ReadQuaternion();
        pos = reader.ReadVector3();
        scale = reader.ReadVector3();
        characterID = reader.ReadUInt32();
        animationID = reader.ReadUInt32();
        f1 = reader.ReadSingle();
        i2 = reader.ReadUInt32();
        i3 = reader.ReadUInt32();
        i4 = reader.ReadUInt32();
        debugInfo = reader.ReadStringPair();
    }
}
