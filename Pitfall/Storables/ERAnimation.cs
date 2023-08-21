using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pitfall.Storables;

[StorableType]
public class ERAnimation : EResource
{
    /* just a structural sketch for now,
     * the actual data is compressed, BoneInfo contains the information to extract 
     * the values from IntArr2
     * As seem various other values like Event.time
     */

    public struct BoneInfo
    {
        public int i1, i2, i3;
    }

    public struct AnimInfo
    {
        public Vector3 v1;
        public bool b1;
        public Vector4 v2;
        public bool b2, b3;
    }

    public enum EventType : uint
    {
        Script = 0,
        Sfx3d,
        Unknown
    }

    public struct Event
    {
        public Matrix4x4 relTransformation;
        public int atBone;
        public float time;
        public uint resourceId;
        public EventType eventType;
    }

    public uint Int1 { get; private set; }
    public float Float1 { get; private set; }
    public IReadOnlyList<BoneInfo> Bones { get; private set; } = Array.Empty<BoneInfo>();
    public float Float2 { get; private set; }
    public IReadOnlyList<uint> IntArr1 { get; private set; } = Array.Empty<uint>();
    public int UnalignedIntCount2 { get; private set; }
    public IReadOnlyList<uint> IntArr2 { get; private set; } = Array.Empty<uint>();
    public AnimInfo Info { get; private set; }
    public IReadOnlyList<Event> Events { get; private set; } = Array.Empty<Event>();

    public ERAnimation(BinaryReader reader)
    {
        Read(reader);
    }

    public override void Read(BinaryReader reader)
    {
        base.Read(reader);
        Int1 = reader.ReadUInt32();
        Float1 = reader.ReadSingle();
        Bones = reader.ReadArray(reader.ReadInt32(), ReadBoneInfo);
        Float2 = reader.ReadSingle();
        IntArr1 = reader.ReadArray(reader.ReadInt32(), r => r.ReadUInt32());
        UnalignedIntCount2 = reader.ReadInt32();
        int aligned = ((UnalignedIntCount2 + 31) & ~31) >> 5;
        IntArr2 = reader.ReadArray(aligned, r => r.ReadUInt32());
        Info = new()
        {
            v1 = reader.ReadVector3(),
            b1 = reader.ReadBoolean(),
            v2 = reader.ReadVector4(),
            b2 = reader.ReadBoolean(),
            b3 = reader.ReadBoolean()
        };
        Events = reader.ReadArray(reader.ReadInt32(), ReadFrame);
    }

    private static BoneInfo ReadBoneInfo(BinaryReader reader) => new()
    {
        i1 = reader.ReadInt32(),
        i2 = reader.ReadInt32(),
        i3 = reader.ReadInt32()
    };

    private static Event ReadFrame(BinaryReader reader) => new()
    {
        relTransformation = reader.ReadMatrix4x4(),
        atBone = reader.ReadInt32(),
        time = reader.ReadSingle(),
        resourceId = reader.ReadUInt32(),
        eventType = (EventType)reader.ReadUInt32()
    };
}
