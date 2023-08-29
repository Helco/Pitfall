using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pitfall.Storables;

[StorableType]
public class EICameraWatch : EICameraPit
{
    public string ReferencedObjectName { get; private set; } = "";

    public override void ReadInstanceData(BinaryReader reader)
    {
        CameraPos = reader.ReadVector3();
        ReferencedObjectName = reader.ReadCString();
    }
}
