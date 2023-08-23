using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pitfall.Havok;

public abstract class HavokDeformableBody
{
    public class Attachment
    {
        public string RigidBody { get; }
        public IReadOnlyList<(uint, bool)> Points { get; }

        internal Attachment(IHavokReader hkReader)
        {
            hkReader.ExpectToken("RIGID_BODY");
            RigidBody = hkReader.ReadString();
            hkReader.ExpectToken("POINTS");
            var points = new (uint, bool)[hkReader.ReadUInt()];
            for (int i = 0; i < points.Length; i++)
                points[i] = (hkReader.ReadUInt(), hkReader.ReadBoolean());
            Points = points;
        }
    }

    public class Deflector
    {
        public string Name { get; }
        public float HeightMax { get; }
        public float HeightMin { get; }
        public float Angular { get; }
        public float Lateral { get; }
        public IReadOnlyList<uint>? Vertices { get; }
        public float Extension { get; }

        internal Deflector(IHavokReader hkReader)
        {
            hkReader.ExpectToken("DEFLECTOR");
            Name = hkReader.ReadString();
            hkReader.ExpectToken("HEIGHT_MAX");
            HeightMax = hkReader.ReadFloat();
            hkReader.ExpectToken("HEIGHT_MIN");
            HeightMin = hkReader.ReadFloat();
            hkReader.ExpectToken("ANGULAR");
            Angular = hkReader.ReadFloat();
            hkReader.ExpectToken("LATERAL");
            Lateral = hkReader.ReadFloat();
            hkReader.ExpectToken("USE_VERTS");
            if (hkReader.ReadBoolean())
            {
                hkReader.ExpectToken("NUM_VERTS");
                var vertices = new uint[hkReader.ReadUInt()];
                for (int i = 0; i < vertices.Length; i++)
                    vertices[i] = hkReader.ReadUInt();
                Vertices = vertices;
            }
            hkReader.ExpectToken("EXTENSION");
            Extension = hkReader.ReadFloat();
        }
    }

    public float Mass { get; private set; }
    public float AirResistance { get; private set; }
    public IReadOnlyList<Vector3>? InitialState { get; private set; }
    public IReadOnlyList<uint> FixedPoints { get; private set; } = Array.Empty<uint>();
    public IReadOnlyList<Attachment> Attachments { get; private set; } = Array.Empty<Attachment>();
    public IReadOnlyList<Deflector> Deflectors { get; private set; } = Array.Empty<Deflector>();
    public HavokDisplayBody? DisplayBody { get; private set; }

    internal HavokToken ReadDeformable(IHavokReader hkReader, int numVertices)
    {
        hkReader.ExpectToken("MASS");
        Mass = hkReader.ReadFloat();
        hkReader.ExpectToken("AIR_RESISTANCE");
        AirResistance = hkReader.ReadFloat();

        hkReader.ExpectToken("INITIAL_STATE");
        if (hkReader.ReadBoolean())
        {
            var initialState = new Vector3[numVertices];
            for (int i = 0; i < numVertices; i++)
                initialState[i] = hkReader.ReadVector3();
            InitialState = initialState;
        }

        hkReader.ExpectToken("FIXED_POINTS");
        var fixedPoints = new uint[hkReader.ReadUInt()];
        for (int i = 0; i < fixedPoints.Length; i++)
            fixedPoints[i] = hkReader.ReadUInt();
        FixedPoints = fixedPoints;

        hkReader.ExpectToken("NUM_ATTACHMENTS");
        var attachments = new Attachment[hkReader.ReadUInt()];
        for (int i = 0; i < attachments.Length; i++)
            attachments[i] = new(hkReader);
        Attachments = attachments;

        hkReader.ExpectToken("NUM_DEFLECTORS");
        var deflectors = new Deflector[hkReader.ReadUInt()];
        for (int i = 0; i < deflectors.Length; i++)
            deflectors[i] = new(hkReader);
        Deflectors = deflectors;

        var token = hkReader.ReadToken();
        if (token == "DISPLAY_SHADOWS")
        {
            DisplayBody = new(hkReader);
            token = hkReader.ReadToken();
        }
        return token;
    }
}
