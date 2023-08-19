using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using Pitfall.Storables;
using SharpGLTF.IO;
using SharpGLTF.Schema2;
using static Pitfall.Storables.ERModel;

namespace Pitfall;

internal class Program
{
    static readonly string[] BannedFiles = new[]
    {
        "files.list",
        "0-evan" // ERHavokWorld(0)
    };

    static void Main(string[] args)
    {
        Directory.CreateDirectory("out");

        var files = Directory.GetFiles(@"C:\Users\Helco\Downloads\PITFALL The Lost Expedition PC\PITFALL The Lost Expedition\Game\data\models");
        foreach (var file in files)
        {
            if (BannedFiles.Any(file.Contains))
                continue;
            if (!file.Contains("explorermodel_type3"))
                continue;
            var name = Path.GetFileName(file);
            using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(fileStream);

            var model = new ERModel(reader);
            var gltf = ConvertModel(model);
            gltf.SaveGLB("out/" + name + ".glb");
        }
    }
    private static ModelRoot ConvertModel(ERModel model)
    {
        var root = ModelRoot.CreateModel();
        var scene = root.UseScene(0);
        scene.Name = model.Name;
        scene.Extras = JsonContent.CreateFrom(new Dictionary<string, object>()
        {
            { "UnknownFloat", model.UnknownFloat }
        });
        for (int i = 0; i < model.SubModels.Count; i++)
            ConvertSubModel(root, scene, i, model.SubModels[i]);
        return root;
    }
    private static Node ConvertSubModel(ModelRoot modelRoot, Scene scene, int index, SubModel subModel)
    {
        var node = scene.CreateNode($"SubModel {index}");
        node.Extras = JsonContent.CreateFrom(new Dictionary<string, object>()
        {
            { "Unknown", subModel.Unknown }
        });
        for (int i = 0; i < subModel.SubSubModels.Count; i++)
            ConvertSubSubModel(modelRoot, node, i, subModel.SubSubModels[i]);
        return node;
    }
    private static Node ConvertSubSubModel(ModelRoot modelRoot, Node nodeParent, int index, SubSubModel subSubModel)
    {
        var node = nodeParent.CreateNode($"SubSubModel {index} {subSubModel.ID:X8}");
        node.Extras = JsonContent.CreateFrom(new Dictionary<string, object>()
        {
            { "ID", subSubModel.ID },
            { "GeometryCount", subSubModel.GeometryCount }
        });
        for (int i = 0; i < subSubModel.Parts.Count; i++)
        {
            switch (subSubModel.Parts[i])
            {
                case GeometryPart geometryPart: ConvertGeometryPart(modelRoot, node, i, geometryPart); break;
                case SetUnknownBytePart setBytePart: ConvertSetUnknownBytePart(node, i, setBytePart); break;
                default: throw new NotImplementedException($"Unimplemented subsubmodel part type");
            }
        }
        return node;
    }
    private static Node ConvertSetUnknownBytePart(Node nodeParent, int index, SetUnknownBytePart part)
    {
        var node = nodeParent.CreateNode($"SetByte {nodeParent.Name} - {index}");
        node.Extras = JsonContent.CreateFrom(new Dictionary<string, object>()
        {
            { "Index", part.Index },
            { "Value", part.Value }
        });
        return node;
    }
    private static Node ConvertGeometryPart(ModelRoot modelRoot, Node nodeParent, int index, GeometryPart part)
    {
        var mesh = modelRoot.CreateMesh();
        mesh.Name = $"Mesh {nodeParent.Name} - {index}";
        var meshPrim = mesh.CreatePrimitive();
        SetVertexAttribute(modelRoot, meshPrim, part.Positions.Select(v => new Vector3(v.X, v.Y, v.Z)).ToArray(), "POSITION", DimensionType.VEC3, EncodingType.FLOAT, normalized: false);
        if (part.TexCoords != null)
            SetVertexAttribute(modelRoot, meshPrim, part.TexCoords, "TEXCOORD_0", DimensionType.VEC2, EncodingType.FLOAT, normalized: false);
        if (part.Colors != null)
            SetVertexAttribute(modelRoot, meshPrim, part.Colors, "COLOR_0", DimensionType.VEC4, EncodingType.UNSIGNED_BYTE, normalized: true);
        if (part.Normals != null)
            SetVertexAttribute(modelRoot, meshPrim, part.Normals, "NORMAL", DimensionType.VEC3, EncodingType.FLOAT, normalized: false);
        if (part.UnknownVector != null)
            SetVertexAttribute(modelRoot, meshPrim, part.UnknownVector, "_UNKNOWN", DimensionType.VEC4, EncodingType.FLOAT, normalized: false);
        meshPrim.DrawPrimitiveType = part.Indices == null || true
            ? PrimitiveType.TRIANGLES
            : PrimitiveType.TRIANGLE_STRIP;
        var indices = part.Indices == null
            ? part.GenerateImplicitIndices()
            : part.GenerateTrianglesFromTriangleStrip();
        var bufferView = CreateBufferViewFromData(modelRoot, indices, withByteStride: false);
        var accessor = modelRoot.CreateAccessor();
        accessor.SetIndexData(bufferView, 0, indices.Length, IndexEncodingType.UNSIGNED_SHORT);
        accessor.UpdateBounds();
        meshPrim.SetIndexAccessor(accessor);
        var node = nodeParent.CreateNode(mesh.Name);
        node.Mesh = mesh;
        return node;
    }
    private static unsafe void SetVertexAttribute<T>(
        ModelRoot modelRoot,
        MeshPrimitive meshPrim,
        T[] array,
        string attributeKey,
        DimensionType dim,
        EncodingType enc,
        bool normalized)
        where T : unmanaged
    {
        var bufferView = CreateBufferViewFromData(modelRoot, array);
        var accessor = modelRoot.CreateAccessor();
        accessor.SetVertexData(bufferView, 0, array.Length, dim, enc, normalized);
        accessor.UpdateBounds();
        meshPrim.SetVertexAccessor(attributeKey, accessor);
    }
    private static unsafe BufferView CreateBufferViewFromData<T>(ModelRoot modelRoot, T[] array, bool withByteStride = true) where T : unmanaged
    {
        var bufferView = modelRoot.CreateBufferView(sizeof(T) * array.Length, withByteStride ? sizeof(T) : 0);
        array.CopyTo(MemoryMarshal.Cast<byte, T>(bufferView.Content.AsSpan()));
        return bufferView;
    }
}
