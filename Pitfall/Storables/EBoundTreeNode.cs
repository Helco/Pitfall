using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pitfall.Storables;

[StorableType]
public class EBoundTreeNode : EStorable
{
    public Vector3 btnV1, btnV2;
    public EStorable? parentTreeNode, btnRel2, btnRel3;

    public override void Read(BinaryReader reader)
    {
        btnV1 = reader.ReadVector3();
        btnV2 = reader.ReadVector3();
        parentTreeNode = reader.ReadStorable();
        btnRel2 = reader.ReadStorable();
        btnRel3 = reader.ReadStorable();
    }
}
