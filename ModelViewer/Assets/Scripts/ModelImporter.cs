using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;
using System;

[ScriptedImporter(1, "pm")]
public class ModelImporter : ScriptedImporter
{
    public Material material;

    public override void OnImportAsset(AssetImportContext ctx)
    {
        using var stream = new FileStream(ctx.assetPath, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(stream);
        var model = new Model(reader);

        var mainGO = new GameObject(model.Name);
        for (int i = 0; i < model.SubModels.Count; i++)
        {
            var subModelGO = new GameObject($"Submodel {i}");
            subModelGO.transform.parent = mainGO.transform;
            for (int j = 0; j < model.SubModels[i].SubSubModels.Count; j++)
                CreateSubSubModel(ctx, subModelGO, model.SubModels[i].SubSubModels[j], j);
        }

        ctx.AddObjectToAsset(mainGO.name, mainGO);
        ctx.SetMainObject(mainGO);
    }

    private void CreateSubSubModel(AssetImportContext ctx, GameObject subModelGO, SubSubModel subSubModel, int index)
    {
        var subSubModelGO = new GameObject($"{index} {subSubModel.ID:X8}");
        subSubModelGO.transform.parent = subModelGO.transform;
        for (int i = 0; i < subSubModel.Parts.Count; i++)
        {
            switch(subSubModel.Parts[i])
            {
                case GeometryPart geom: CreateGeometryPart(ctx, subSubModelGO, geom, index); break;
                case SetUnknownBytePart setByte:
                    var go = new GameObject($"SetByte {i} {setByte.Index} = {setByte.Value}");
                    go.transform.parent = subSubModelGO.transform;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    private void CreateGeometryPart(AssetImportContext ctx, GameObject subModelGO, GeometryPart part, int index)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = part.Positions.Select(v => new Vector3(v.X, v.Z, v.Y)).ToArray();
        if (part.TexCoords != null)
            mesh.uv = part.TexCoords.Select(t => new Vector2(t.X, t.Y)).ToArray();
        if (part.Normals != null)
            mesh.normals = part.Normals.Select(n => new Vector3(n.X, n.Z, n.Y)).ToArray();
        if (part.Colors != null)
            mesh.colors32 = part.Colors.Select(c => new Color32(c.R, c.G, c.B, c.A)).ToArray();
        mesh.subMeshCount = 1;
        if (part.Indices == null)
        {
            // looping quad strip
            mesh.SetIndices(part.GenerateImplicitIndices(), MeshTopology.Triangles, 0);
        }
        else
        {
            // triangle strip
            var indices = part.Indices;
            List<ushort> newIndices = new List<ushort>(indices.Length * 3);
            newIndices.Add(indices[0]);
            newIndices.Add(indices[1]);
            newIndices.Add(indices[2]);
            for (int i = 2; i < indices.Length; i++)
            {
                newIndices.Add(indices[i - 1 - (i % 2)]);
                newIndices.Add(indices[i - 2 + (i % 2)]);
                newIndices.Add(indices[i - 0]);
            }
            mesh.SetIndices(newIndices, MeshTopology.Triangles, 0);
        }

        mesh.RecalculateBounds();
        //mesh.RecalculateNormals();
        mesh.name = $"{subModelGO.name} Geometry {index}";
        ctx.AddObjectToAsset(mesh.name, mesh);

        var variantGO = new GameObject($"Geometry {index}");
        variantGO.transform.parent = subModelGO.transform;
        variantGO.AddComponent<MeshFilter>().sharedMesh = mesh;
        var ren = variantGO.AddComponent<MeshRenderer>();
        ren.sharedMaterial = material;
        ren.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        ren.receiveShadows = false;
    }
}

