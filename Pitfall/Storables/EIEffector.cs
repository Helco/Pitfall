using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pitfall.Storables;

[StorableType]
public class EIEffector : EIStaticModel
{
    public Vector3 effPos;
    public Quaternion effRot;
    public uint effID1, effID2;
    public float effF1;
    public uint modelID;
    public bool effFlag1, effFlag2, effFlag3, effFlag4;
    public Vector3 effV1;
    public string effString = "";

    public override void ReadInstanceData(BinaryReader reader)
    {
        effPos = reader.ReadVector3();
        effRot = reader.ReadQuaternion();
        effID1 = reader.ReadUInt32();
        effID2 = reader.ReadUInt32();
        modelID = reader.ReadUInt32();
        effFlag1 = reader.ReadBoolean();
        effF1 = reader.ReadUInt32();
        effFlag2 = reader.ReadBoolean();
        effFlag3 = reader.ReadBoolean();
        effFlag4 = reader.ReadBoolean();
        effV1 = reader.ReadVector3();
        effString = reader.ReadCString();
    }
}
