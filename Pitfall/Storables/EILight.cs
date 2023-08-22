using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pitfall.Storables;

[StorableType]
public class EILight : EInstance
{
    public bool lightFlag1, lightFlag2, lightFlag3;
    public float lightF1;
    public Vector3 color;
    public EStorable? lightRelated; // could be EScriptData?

    public override void Read(BinaryReader reader)
    {
        base.Read(reader);
        lightFlag1 = reader.ReadBoolean();
        lightFlag2 = reader.ReadBoolean();
        lightFlag3 = reader.ReadBoolean();
        lightF1 = reader.ReadSingle();
        color = reader.ReadVector3();
        lightRelated = reader.ReadStorable();
    }
}

[StorableType]
public class EIAmbLight : EILight { }

[StorableType]
public class EIPointLight : EILight
{
    public Vector3 pointLightV1;
    public float pointLightF1, pointLightF2;
    public bool pointFlag;

    public override void Read(BinaryReader reader)
    {
        base.Read(reader);
        pointLightV1 = reader.ReadVector3();
        pointLightF1 = reader.ReadSingle();
        pointLightF2 = reader.ReadSingle();
        pointFlag = reader.ReadBoolean();
    }
}

[StorableType]
public class EIDirLight : EILight
{
    public Vector3 dirV1;

    public override void Read(BinaryReader reader)
    {
        base.Read(reader);
        dirV1 = reader.ReadVector3();
    }
}

[StorableType]
public class EISpotLight : EILight
{
    public Vector3 spotLightV1, spotLightV2;
    public float spotLightF1, spotLightF2, spotLightF3, spotLightF4;
    public bool spotFlag;

    public override void Read(BinaryReader reader)
    {
        base.Read(reader);
        spotLightV1 = reader.ReadVector3();
        spotLightV2 = reader.ReadVector3();
        spotLightF1 = reader.ReadSingle();
        spotLightF2 = reader.ReadSingle();
        spotLightF3 = reader.ReadSingle();
        spotLightF4 = reader.ReadSingle();
        spotFlag = reader.ReadBoolean();
    }
}
