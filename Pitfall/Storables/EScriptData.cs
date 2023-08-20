using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Web;

namespace Pitfall.Storables;

public enum ScriptDataType
{
    Invalid = 0,
    Float = 'f',
    Int = 'i',
    Matrix = 'm',
    Pointer = 'p',
    String = 's',
    Vector = 'v',
    Resource = 'r',
    Script = 'e',
    ParticleType = 't',
    Animation = 'a',
    Model = 'w',
    Shader = 'h',
    Sound = 'o'
}

[StorableType]
public class EScriptData : EStorable
{
    public override void Read(BinaryReader reader)
    {
        if (ReadVersion == 0)
            reader.ReadUInt32(); // unused in the original engine
    }

    public override string ToString() => "<error>";
    public virtual ScriptDataType DataType => ScriptDataType.Invalid;
}

[StorableType]
public class ESDResource : EScriptData
{
    public uint ResourceID { get; private set; }

    public override void Read(BinaryReader reader)
    {
        base.Read(reader);
        ResourceID = reader.ReadUInt32();
    }

    public override string ToString() => $"resource {ResourceID:X8}";
    public override ScriptDataType DataType => ScriptDataType.Resource;
}

[StorableType]
public class ESDAnimation : ESDResource
{
    public override string ToString() => $"animation {ResourceID:X8}";
    public override ScriptDataType DataType => ScriptDataType.Animation;
}

[StorableType]
public class ESDScript : ESDResource
{
    public override string ToString() => $"script {ResourceID:X8}";
    public override ScriptDataType DataType => ScriptDataType.Script;
}

[StorableType]
public class ESDModel : ESDResource
{
    public override string ToString() => $"model {ResourceID:X8}";
    public override ScriptDataType DataType => ScriptDataType.Model;
}

[StorableType]
public class ESDSound : ESDResource
{
    public override string ToString() => $"sound {ResourceID:X8}";
    public override ScriptDataType DataType => ScriptDataType.Sound;
}

[StorableType]
public class ESDShader : ESDResource
{
    public override string ToString() => $"shader {ResourceID:X8}";
    public override ScriptDataType DataType => ScriptDataType.Shader;
}

[StorableType]
public class ESDParticleType : ESDResource
{
    public override string ToString() => $"particle type {ResourceID:X8}";
    public override ScriptDataType DataType => ScriptDataType.ParticleType;
}

[StorableType]
public class ESDFloat : EScriptData
{
    public float Value { get; private set; }

    public override void Read(BinaryReader reader)
    {
        base.Read(reader);
        Value = reader.ReadSingle();
    }

    public override string ToString() => $"{Value.ToString(CultureInfo.InvariantCulture)}f";
    public override ScriptDataType DataType => ScriptDataType.Float;
}

[StorableType]
public class ESDVector : EScriptData
{
    public Vector3 Value { get; private set; }

    public override void Read(BinaryReader reader)
    {
        base.Read(reader);
        Value = reader.ReadVector3();
    }

    public override string ToString() => Value.ToString();
    public override ScriptDataType DataType => ScriptDataType.Vector;
}

[StorableType]
public class ESDInt : EScriptData
{
    public int Value { get; private set; }

    public override void Read(BinaryReader reader)
    {
        base.Read(reader);
        Value = reader.ReadInt32();
    }

    public override string ToString() => Value.ToString();
    public override ScriptDataType DataType => ScriptDataType.Int;
}

[StorableType]
public class ESDMatrix : EScriptData
{
    public Matrix4x4 Value { get; private set; }

    public override void Read(BinaryReader reader)
    {
        base.Read(reader);
        Value = reader.ReadMatrix4x4();
    }

    public override string ToString() => Value.ToString();
    public override ScriptDataType DataType => ScriptDataType.Matrix;
}

[StorableType]
public class ESDString : EScriptData
{
    public string Value { get; private set; } = "";

    public override void Read(BinaryReader reader)
    {
        base.Read(reader);
        Value = reader.ReadCString();
    }

    public override string ToString() => $"\"{HttpUtility.JavaScriptStringEncode(Value)}\"";
    public override ScriptDataType DataType => ScriptDataType.String;
}

[StorableType]
public class ESDPointer : EScriptData
{
    public EStorable? Value { get; private set; }

    public override void Read(BinaryReader reader)
    {
        base.Read(reader);
        Value = reader.ReadStorable();
    }

    public override string ToString() => Value == null ? "ptr null" : $"ptr {Value}";
    public override ScriptDataType DataType => ScriptDataType.Pointer;
}
