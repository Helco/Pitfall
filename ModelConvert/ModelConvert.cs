using System;
using System.IO;
using System.Runtime.InteropServices;
using SharpGLTF.IO;
using SharpGLTF.Schema2;
//using Assimp;

namespace Pitfall;

internal class Program
{
    static void Main(string[] args)
    {
        Directory.CreateDirectory("out");
        //var assimp = new AssimpContext();
        var files = Directory.GetFiles(@"C:\Users\Helco\Downloads\PITFALL The Lost Expedition PC\PITFALL The Lost Expedition\Game\data\models");
        foreach (var file in files)
        {
            if (file.Contains("files.list"))
                continue;
            var name = Path.GetFileName(file);
            using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(fileStream);

            var model = new Model(reader);
            var gltf = ConvertModel(model);
            gltf.SaveGLB("out/" + name + ".glb");
            //var assimpScene = ConvertModelToAssimp(model);
            //assimp.ExportFile(assimpScene, "out/" + name + ".glb", "glb2");
        }
    }

    /*private static Scene ConvertModelToAssimp(Model model)
    {
        var scene = new Scene();
        scene.Metadata["Name"] = new(MetaDataType.String, model.Name);
        scene.Metadata["UnknownFloat"] = new(MetaDataType.Float, model.UnknownFloat);
        scene.RootNode = new(model.Name);
        for (int i = 0; i < model.SubModels.Count; i++)
            ConvertSubModel(scene, i, model.SubModels[i]);
    }

    private static void ConvertSubModel(Scene scene, int index, SubModel subModel)
    {
        var node = new Node($"SubModel {index}", scene.RootNode);
        scene.RootNode.Children.Add(node);
        node.Metadata.Add("Unknown", new(MetaDataType.Int32, subModel.Unknown));
        for(int i = 0; i < subModel.SubSubModels.Count; i++)
            ConvertSubSubModel(scene, node, i, subModel.SubSubModels[i]);
    }

    private static void ConvertSubSubModel(Scene scene, Node nodeParent, int index, SubSubModel subSubModel)
    {
        var node = new Node($"{subSubModel.ID:X8}", nodeParent);
        nodeParent.Children.Add(node);
        node.Metadata.Add("GeometryCount", new(MetaDataType.Int32, subSubModel.GeometryCount));
        for (int i = 0; i < subSubModel.Parts.Count; i++)
        {
            switch (subSubModel.Parts[i])
            {
                case GeometryPart geometryPart: ConvertGeometryPart(scene, node, i, geometryPart); break;
                case SetUnknownBytePart setBytePart: ConvertSetUnknownBytePart(scene, node, i, setBytePart); break;
                default: throw new NotImplementedException($"Unimplemented subsubmodel part type");
            }
        }
    }

    private static void ConvertSetUnknownBytePart(Scene scene, Node nodeParent, int index, SetUnknownBytePart part)
    {
        var node = new Node($"SetByte {index} {part.Index} = {part.Value}", nodeParent);
        nodeParent.Children.Add(node);
        node.Metadata.Add("Index", new(MetaDataType.Int32, (int)part.Index));
        node.Metadata.Add("Value", new(MetaDataType.Int32, (int)part.Value));
    }

    private static void ConvertGeometryPart(Scene scene, Node nodeParent, int index, GeometryPart part)
    {
        var mesh = new Mesh($"{nodeParent.Name} - {index}");
        mesh.Vertices.AddRange(part.Positions.Select())
    }*/

    private static ModelRoot ConvertModel(Model model)
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
            switch(subSubModel.Parts[i])
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
        SetVertexAttribute(modelRoot, meshPrim, part.Positions, "POSITIONS", DimensionType.VEC4, EncodingType.FLOAT, normalized: false);
        if (part.TexCoords != null)
            SetVertexAttribute(modelRoot, meshPrim, part.TexCoords, "TEXCOORD_0", DimensionType.VEC2, EncodingType.FLOAT, normalized: false);
        if (part.Colors != null)
            SetVertexAttribute(modelRoot, meshPrim, part.Colors, "COLOR_0", DimensionType.VEC4, EncodingType.UNSIGNED_BYTE, normalized: true);
        if (part.Normals != null)
            SetVertexAttribute(modelRoot, meshPrim, part.Normals, "NORMAL", DimensionType.VEC3, EncodingType.FLOAT, normalized: false);
        if (part.UnknownVector != null)
            SetVertexAttribute(modelRoot, meshPrim, part.UnknownVector, "_UNKNOWN", DimensionType.VEC4, EncodingType.FLOAT, normalized: false);

        meshPrim.DrawPrimitiveType = part.Indices == null
            ? PrimitiveType.TRIANGLES
            : PrimitiveType.TRIANGLE_STRIP;
        var indices = part.Indices ?? part.GenerateImplicitIndices();
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
