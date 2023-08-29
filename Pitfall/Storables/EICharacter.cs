using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pitfall.Storables;

[StorableType]
public class EICharacter : EIGameInstance
{
    public uint characterI1, characterI2;
    public Vector3 characterV1, characterV2, characterV3;
    public Quaternion characterRot;

    public override void Read(BinaryReader reader)
    {
        characterI1 = reader.ReadUInt32();
        characterI2 = reader.ReadUInt32();
        characterV1 = reader.ReadVector3();
        characterV2 = reader.ReadVector3();
        characterV3 = reader.ReadVector3();
        characterRot = reader.ReadQuaternion();
    }
}

[StorableType]
public class EIBeast : EICharacter { }
