using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pitfall.Storables;

[StorableType]
public class EILookatPoint : EIGameInstance
{
    public string Name { get; private set; } = "";
    public Vector3 Pos { get; private set; }
    public float f1;
    public bool flag1;

    public override void ReadInstanceData(BinaryReader reader)
    {
        Name = reader.ReadCString();
        Pos = reader.ReadVector3();
        f1 = reader.ReadSingle();
        flag1 = reader.ReadBoolean();
    }
}
