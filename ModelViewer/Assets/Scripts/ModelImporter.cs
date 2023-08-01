using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;
using System;

public class Header
{
    public Header(BinaryReader reader)
    {
        if (!reader.ReadBytes(6).All(b => b == 0))
            throw new System.Exception("Expected six zero bytes at start");

        var nameBytes = new List<byte>();
        do
        {
            nameBytes.Add(reader.ReadByte());
        } while (nameBytes.Last() != 0);
        Name = System.Text.Encoding.UTF8.GetString(nameBytes.ToArray(), 0, nameBytes.Count - 1);

        if (reader.ReadByte() != 0)
            throw new System.Exception("Expected another zero byte");
        if (reader.ReadSingle() != 1f)
            throw new System.Exception("Expected 1.0f");
    }

    public readonly string Name;
}

public class SubModel
{
    public readonly int i2;
    public readonly Variant[] variants;

    public SubModel(BinaryReader reader)
    {
        i2 = reader.ReadInt32();
        var variantCount = reader.ReadInt32();
        variants = Enumerable.Repeat(0, variantCount).Select(_ => new Variant(reader)).ToArray();
    }
}

[System.Flags]
public enum VertexFlags
{
    HasUV = 2,
    HasAttr4B1 = 4,
    HasAttr4B2 = 8,
    HasAttr4B3 = 0x40,
    HasIndices = 0x20
}

public class Variant
{
    public readonly uint ID;
    public readonly VertexFlags Flags;
    public readonly byte b1;
    public readonly int cflags1;
    public readonly byte cbyte1;
    public readonly ulong weirdstuff;
    public readonly byte materialType;

    public readonly Vector4[] vertices;
    public readonly Vector2[] uvs;
    public readonly uint[] attr4b1;
    public readonly uint[] attr4b2;
    public readonly uint[] attr4b3;
    public readonly ushort[] indices;

    public Variant(BinaryReader reader)
    {
        ID = reader.ReadUInt32();
        Flags = (VertexFlags)reader.ReadUInt32();

        b1 = reader.ReadByte();
        if (b1 == 0) { }
        else if (b1 == 1)
            cflags1 = reader.ReadInt32();
        else if (b1 == 4)
            cbyte1 = reader.ReadByte();
        else if (b1 > 4)
            weirdstuff = reader.ReadUInt64();

        var vertexCount = reader.ReadInt32();
        vertices = Enumerable.Repeat(0, vertexCount).Select(_ => new Vector4(
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle())).ToArray();

        if (Flags.HasFlag(VertexFlags.HasUV))
            uvs = Enumerable.Repeat(0, vertexCount).Select(_ => new Vector2(
                reader.ReadSingle(), reader.ReadSingle())).ToArray();
        if (Flags.HasFlag(VertexFlags.HasAttr4B1))
            attr4b1 = Enumerable.Repeat(0, vertexCount).Select(_ => reader.ReadUInt32()).ToArray();
        if (Flags.HasFlag(VertexFlags.HasAttr4B2))
            attr4b2 = Enumerable.Repeat(0, vertexCount).Select(_ => reader.ReadUInt32()).ToArray();
        if (Flags.HasFlag(VertexFlags.HasAttr4B3))
            attr4b3 = Enumerable.Repeat(0, vertexCount).Select(_ => reader.ReadUInt32()).ToArray();

        if (Flags.HasFlag(VertexFlags.HasIndices))
        {
            int indexCount = reader.ReadInt32();
            indices = Enumerable.Repeat(0, indexCount).Select(_ => reader.ReadUInt16()).ToArray();
        }

        materialType = reader.ReadByte();
    }
}

[ScriptedImporter(1, "pm")]
public class ModelImporter : ScriptedImporter
{
    public Material material;

    public override void OnImportAsset(AssetImportContext ctx)
    {
        using var stream = new FileStream(ctx.assetPath, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(stream);
        var header = new Header(reader);

        var subModelCount = reader.ReadInt32();
        var subModels = Enumerable.Repeat(0, subModelCount).Select(_ => new SubModel(reader)).ToArray();

        var mainGO = new GameObject(header.Name);
        for (int i = 0; i < subModels.Length; i++)
        {
            var subModelGO = new GameObject($"Submodel {i}");
            subModelGO.transform.parent = mainGO.transform;
            for (int j = 0; j < subModels[i].variants.Length; j++)
                CreateVariant(ctx, subModelGO, subModels[i].variants[j], j);
        }

        ctx.AddObjectToAsset(mainGO.name, mainGO);
        ctx.SetMainObject(mainGO);
    }

    private void CreateVariant(AssetImportContext ctx, GameObject subModelGO, Variant variant, int index)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = variant.vertices.Select(v => new Vector3(v.x, v.z, v.y)).ToArray();
        if (variant.uvs != null)
            mesh.uv = variant.uvs;
        mesh.subMeshCount = 1;
        if (variant.indices == null)
        {
            mesh.SetIndices(Enumerable.Range(0, mesh.vertexCount).ToArray(), MeshTopology.Triangles, 0);
        }
        else
        {
            List<ushort> newIndices = new List<ushort>(variant.indices.Length * 3);
            newIndices.Add(variant.indices[0]);
            newIndices.Add(variant.indices[1]);
            newIndices.Add(variant.indices[2]);
            for (int i = 3; i < variant.indices.Length; i++)
            {
                newIndices.Add(variant.indices[i - 1 - (i % 2)]);
                newIndices.Add(variant.indices[i - 2 + (i % 2)]);
                newIndices.Add(variant.indices[i - 0]);
            }
            mesh.SetIndices(newIndices, MeshTopology.Triangles, 0);
        }
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.name = $"Mesh {index} {variant.ID:X8}";
        ctx.AddObjectToAsset(mesh.name, mesh);

        var variantGO = new GameObject($"{index} {variant.ID:X8}");
        variantGO.transform.parent = subModelGO.transform;
        variantGO.AddComponent<MeshFilter>().sharedMesh = mesh;
        variantGO.AddComponent<MeshRenderer>().sharedMaterial = material;
    }
}

