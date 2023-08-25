using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pitfall.Storables;

[StorableType]
public class EIStaticModel : EIGameInstance
{
    public uint ModelID { get; private set; }
    public Matrix4x4 Transform { get; private set; }

    public override void Read(BinaryReader reader)
    {
        ModelID = reader.ReadUInt32();
        Transform = reader.ReadMatrix4x4();
    }
}

[StorableType]
public class EIDynamicModel : EIStaticModel { }
