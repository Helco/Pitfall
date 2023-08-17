using System;
using System.IO;
using System.Runtime.InteropServices;
using Assimp;
using Pitfall.Storables;

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
        var assimp = new AssimpContext();
        foreach (var f in assimp.GetSupportedExportFormats())
        {
            Console.WriteLine($"{f.FormatId} - {f.FileExtension} - {f.Description}");
        }

        var files = Directory.GetFiles(@"C:\Users\Helco\Downloads\PITFALL The Lost Expedition PC\PITFALL The Lost Expedition\Game\data\models");
        foreach (var file in files)
        {
            if (BannedFiles.Any(file.Contains))
                continue;
            var name = Path.GetFileName(file);
            using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(fileStream);

            var model = new ERModel(reader);
        }
    }

    private static Scene ConvertModelToAssimp(ERModel model)
    {
        var scene = new Scene();
        //scene.Metadata["Name"] = new(MetaDataType.String, model.Name);
        //scene.Metadata["UnknownFloat"] = new(MetaDataType.Float, model.UnknownFloat);
        scene.RootNode = new(model.Name);
        for (int i = 0; i < model.SubModels.Count; i++)
            ConvertSubModel(scene, i, model.SubModels[i]);
        return scene;
    }

    private static void ConvertSubModel(Scene scene, int index, ERModel.SubModel subModel)
    {
        var node = new Node($"SubModel {index}", scene.RootNode);
        scene.RootNode.Children.Add(node);
        node.Metadata.Add("Unknown", new(MetaDataType.Int32, subModel.Unknown));
        for(int i = 0; i < subModel.SubSubModels.Count; i++)
            ConvertSubSubModel(scene, node, i, subModel.SubSubModels[i]);
    }

    private static void ConvertSubSubModel(Scene scene, Node nodeParent, int index, ERModel.SubSubModel subSubModel)
    {
        var node = new Node($"{subSubModel.ID:X8}", nodeParent);
        nodeParent.Children.Add(node);
        node.Metadata.Add("GeometryCount", new(MetaDataType.Int32, subSubModel.GeometryCount));
        for (int i = 0; i < subSubModel.Parts.Count; i++)
        {
            switch (subSubModel.Parts[i])
            {
                case ERModel.GeometryPart geometryPart: ConvertGeometryPart(scene, node, i, geometryPart); break;
                case ERModel.SetUnknownBytePart setBytePart: ConvertSetUnknownBytePart(scene, node, i, setBytePart); break;
                default: throw new NotImplementedException($"Unimplemented subsubmodel part type");
            }
        }
    }

    private static void ConvertSetUnknownBytePart(Scene scene, Node nodeParent, int index, ERModel.SetUnknownBytePart part)
    {
        var node = new Node($"SetByte {index} {part.Index} = {part.Value}", nodeParent);
        nodeParent.Children.Add(node);
        node.Metadata.Add("Index", new(MetaDataType.Int32, (int)part.Index));
        node.Metadata.Add("Value", new(MetaDataType.Int32, (int)part.Value));
    }

    private static void ConvertGeometryPart(Scene scene, Node nodeParent, int index, ERModel.GeometryPart part)
    {
        var mesh = new Mesh($"{nodeParent.Name} - {index}", PrimitiveType.Triangle);
        mesh.Vertices.AddRange(part.Positions.Select(p => new Vector3D(p.X, p.Y, p.Z)));
        if (part.Colors != null)
        {
            mesh.VertexColorChannels[0].AddRange(part.Colors
                .Select(c => c.AsNormalized)
                .Select(c => new Color4D(c.X, c.Y, c.Z, c.W)));
        }
        if (part.TexCoords != null)
        {
            mesh.TextureCoordinateChannels[0].AddRange(part.TexCoords.Select(t => new Vector3D(t.X, t.Y, 0f)));
            mesh.UVComponentCount[0] = 2;
        }
        if (part.Normals != null)
            mesh.Normals.AddRange(part.Normals.Select(n => new Vector3D(n.X, n.Y, n.Z)));
        var indices = part.Indices == null
            ? part.GenerateImplicitIndices()
            : part.GenerateTrianglesFromTriangleStrip();
        mesh.SetIndices(indices.Select(i => (int)i).ToArray(), 3);

        var node = new Node($"Geometry {index}", nodeParent);
        nodeParent.Children.Add(node);
        node.MeshIndices.Add(scene.Meshes.Count);
        scene.Meshes.Add(mesh);
        node.Metadata.Add("HasUnknownVector", new(MetaDataType.Bool, part.UnknownVector != null));
    }
}
