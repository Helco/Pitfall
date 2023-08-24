using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pitfall.Storables;

[StorableType]
public class EIParticleEmit : EIGameInstance
{
    public Vector3 emitPos;
    public Quaternion emitRot;
    public uint particleTypeID;
    public bool emitFlag;
    public uint emitI1, emitI2, emitI3;

    public override void ReadInstanceData(BinaryReader reader)
    {
        emitPos = reader.ReadVector3();
        emitRot = reader.ReadQuaternion();
        particleTypeID = reader.ReadUInt32();
        emitFlag = reader.ReadBoolean();
        emitI1 = reader.ReadUInt32();
        emitI2 = reader.ReadUInt32();
        emitI3 = reader.ReadUInt32();
    }
}
