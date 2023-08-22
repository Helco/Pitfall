using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pitfall.Storables;

// not original but StaticSubModel and StaticSubModelShader has a lot of duplicated code

public class EIStaticSubModelBase : EInstance
{
    public uint ModelResID { get; private set; }
    public uint modelI2;
    public Vector4 modelV1;
    public uint modelI3, modelI4;

    public override void Read(BinaryReader reader)
    {
        base.Read(reader);
        ModelResID = reader.ReadUInt32();
        modelI2 = reader.ReadUInt32();
        modelV1 = reader.ReadVector4();
        modelI3 = reader.ReadUInt32();
        modelI4 = reader.ReadUInt32();
    }
}

[StorableType]
public class EIStaticSubModel : EIStaticSubModelBase { }

[StorableType]
public class EIStaticSubModelShader : EIStaticSubModelBase { }
