using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Pitfall.Storables;

[StorableType]
public class EIPlatform : EIDynamicModel { }

[StorableType]
public class EIGamePlatform : EIPlatform
{
    public Vector3 platPos;
    public Vector3 platScale;
    public Quaternion platRot;
    public bool platFlag1;
    public bool platFlag2;
    public bool platFlag3;
    public uint platI1;
    public uint platI2;
    public uint platID1;
    public uint platID2;
    public uint platI5;
    public uint platID3;
    public float platF1;
    public bool platFlag4;
    public bool platFlag5;
    public uint platI8;
    public float platF2;
    public bool platFlag6;
    public float platF3;
    public Vector3 platV1;
    public bool platFlag7;
    public uint platIB;
    public uint platIC;
    public bool platFlag8;
    public (string, string) platDebugInfo;

    public override void ReadInstanceData(BinaryReader reader)
    {
        platPos = reader.ReadVector3();
        platScale = reader.ReadVector3();
        platRot = reader.ReadQuaternion();
        platFlag1 = reader.ReadBoolean();
        platFlag2 = reader.ReadBoolean();
        platFlag3 = reader.ReadBoolean();
        platI1 = reader.ReadUInt32();
        platI2 = reader.ReadUInt32();
        platID1 = reader.ReadUInt32();
        platID2 = reader.ReadUInt32();
        platI5 = reader.ReadUInt32();
        platID3 = reader.ReadUInt32();
        platF1 = reader.ReadSingle();
        platFlag4 = reader.ReadBoolean();
        platFlag5 = reader.ReadBoolean();
        platI8 = reader.ReadUInt32();
        platF2 = reader.ReadSingle();
        platFlag6 = reader.ReadBoolean();
        platF3 = reader.ReadSingle();
        platV1 = reader.ReadVector3();
        platFlag7 = reader.ReadBoolean();
        platIB = reader.ReadUInt32();
        platIC = reader.ReadUInt32();
        platFlag8 = reader.ReadBoolean();
        platDebugInfo = (reader.ReadCString(), reader.ReadCString());
    }
}
