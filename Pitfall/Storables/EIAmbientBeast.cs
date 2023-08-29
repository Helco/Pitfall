using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pitfall.Storables;

[StorableType]
public class EIAmbientBeast : EIBeast
{
    public string beastStr1 = "";
    public Vector3 beastPos;
    public Quaternion beastRot;
    public string beastStr2 = "";
    public float beastF1;
    public uint beastI2, beastI3;
    public StringPair debugInfo;

    public override void ReadInstanceData(BinaryReader reader)
    {
        beastStr1 = reader.ReadCString();
        beastPos = reader.ReadVector3();
        beastRot = reader.ReadQuaternion();
        beastStr2 = reader.ReadCString();
        beastF1 = reader.ReadSingle();
        beastI2 = reader.ReadUInt32();
        beastI3 = reader.ReadUInt32();
        debugInfo = reader.ReadStringPair();
    }
}
