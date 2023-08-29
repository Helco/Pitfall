using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Pitfall.Storables;

[StorableType]
public class EICamera : EIGameInstance
{
    public Vector3 CameraPos { get; protected set; }
    public Quaternion CameraRot { get; protected set; }

    public override void ReadInstanceData(BinaryReader reader)
    {
        CameraPos = reader.ReadVector3();
        CameraRot = reader.ReadQuaternion();
    }
}

[StorableType]
public class EICameraPit : EICamera
{
    // yes - this subclass not reading the parent data is original
    public override void ReadInstanceData(BinaryReader reader) { }
}
