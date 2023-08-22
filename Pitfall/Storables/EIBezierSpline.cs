using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pitfall.Storables;

[StorableType]
public class EIBezierSpline : EIGameInstance
{
    public uint splineI1, splineI2;
    public Vector3[] Points { get; private set; } = Array.Empty<Vector3>();

    public override void Read(BinaryReader reader)
    {
        base.Read(reader);
        splineI1 = reader.ReadUInt32();
        splineI2 = reader.ReadUInt32();
        Points = reader.ReadArray(reader.ReadInt32(), Utils.ReadVector3);
    }
}
