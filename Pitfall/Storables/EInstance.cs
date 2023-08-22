using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pitfall.Storables;

[StorableType]
public class EInstance : EStorable
{
    public uint instI1, instI2;
    public EStorable? instanceParent { get; set; }
    public uint instFlags1;
    public Vector3 instV1, instV2;
    public uint instI3;
    public uint instFlags2;

    public override void Read(BinaryReader reader)
    {
        instI1 = reader.ReadUInt32();
        instI2 = reader.ReadUInt32();
        instanceParent = reader.ReadStorable();
        instFlags1 = reader.ReadUInt32();
        instV1 = reader.ReadVector3();
        instV2 = reader.ReadVector3();
        instI3 = reader.ReadUInt32();
        instFlags2 = reader.ReadUInt32();
    }
}

[StorableType]
public class EIGameInstance : EInstance { }
