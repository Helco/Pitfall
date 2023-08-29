using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pitfall.Storables;

[StorableType]
public class EINative : EIBeast
{
    public string nativeStr1 = "", nativeStr2 = "";
    public Vector3 nativePos;
    public Quaternion nativeRot;
    public uint nativeI2, nativeI3, nativeI4, nativeI5, nativeIA, nativeIB, nativeIC;
    public float nativeF0, nativeF1, nativeF2, nativeF3, nativeF4;
    public bool nativeFlag;
    public StringPair debugInfo1, debugInfo2;

    public override void ReadInstanceData(BinaryReader reader)
    {
        nativeStr1 = reader.ReadCString();
        nativeStr2 = reader.ReadCString();
        nativePos = reader.ReadVector3();
        nativeRot = reader.ReadQuaternion();
        nativeF0 = reader.ReadSingle();
        nativeI2 = reader.ReadUInt32();
        nativeI3 = reader.ReadUInt32();
        debugInfo1 = reader.ReadStringPair();
        nativeI4 = reader.ReadUInt32();
        nativeI5 = reader.ReadUInt32();
        nativeF1 = reader.ReadSingle();
        nativeFlag = reader.ReadBoolean();
        nativeF2 = reader.ReadSingle();
        nativeF3 = reader.ReadSingle();
        nativeF4 = reader.ReadSingle();
        nativeIA = reader.ReadUInt32();
        nativeIB = reader.ReadUInt32();
        nativeIC = reader.ReadUInt32();
        debugInfo2 = reader.ReadStringPair();
    }
}
