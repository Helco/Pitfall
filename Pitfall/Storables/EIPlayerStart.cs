using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Pitfall.Storables;

[StorableType]
public class EIPlayerStart : EIGameInstance
{
    public Vector3 playerStartPos;
    public Quaternion playerStartRot;
    public uint playerStartI1;
    public uint playerStartI2;
    public uint playerStartI3;
    public uint playerStartI4;

    public override void ReadInstanceData(BinaryReader reader)
    {
        playerStartPos = reader.ReadVector3();
        playerStartRot = reader.ReadQuaternion();
        playerStartI1 = reader.ReadUInt32();
        playerStartI2 = reader.ReadUInt32();
        playerStartI3 = reader.ReadUInt32();
        playerStartI4 = reader.ReadUInt32();
    }
}
