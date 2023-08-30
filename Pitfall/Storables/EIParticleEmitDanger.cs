using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pitfall.Storables;

[StorableType]
public class EIParticleEmitDanger : EIParticleEmitLimpet
{
    public uint dangerI0, dangerI2, dangerI6;
    public int dangerSI1;
    public float dangerF1, dangerF2, dangerF3, dangerF4, dangerF5;
    public bool dangerFlag1, dangerFlag2, dangerFlag3;
    public Vector3 dangerV1, dangerV2;
    public string dangerStr = "";

    public override void ReadInstanceData(BinaryReader reader)
    {
        emitPos = reader.ReadVector3();
        emitRot = reader.ReadQuaternion();
        emitFlag = reader.ReadBoolean();
        dangerI0 = reader.ReadUInt32();
        dangerStr = reader.ReadCString();
        dangerF1 = reader.ReadSingle();
        dangerI2 = reader.ReadUInt32();
        particleTypeID = reader.ReadUInt32();
        dangerF2 = reader.ReadSingle();
        dangerF3 = reader.ReadSingle();
        dangerF4 = reader.ReadSingle();
        dangerV1 = reader.ReadVector3();
        dangerV2 = reader.ReadVector3();
        dangerFlag1 = reader.ReadBoolean();
        dangerFlag2 = reader.ReadBoolean();
        dangerI6 = reader.ReadUInt32();
        dangerSI1 = reader.ReadInt32();
        dangerF5 = reader.ReadSingle();
        dangerFlag3 = reader.ReadBoolean();
    }
}
