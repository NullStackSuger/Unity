using System.Numerics;
using ImGuiNET;

namespace UnityEngine;

public class MeshComponent : MonoBehaviour
{
    public string ObjPath
    {
        get => objPath;
        set => LoadObj(value);
    }
    private string objPath = "";
    public string VertPath { get; private set; }
    public string FragPath { get; private set; }

    public AABB AABB { get; private set; }

    public ushort[] indices;
    public Vector3[] positions;
    public Vector2[] uvs;
    public Vector3[] normals;

    private bool showSelectObjPanel;
    private bool showSelectVertPanel;
    private bool showSelectFragPanel;

    public MeshComponent()
    {
        //TODO Test
        ObjPath = FileSystem.GetLikeFiles(".obj")?.First();
        VertPath = $"{Define.AssetPath}\\Shaders\\Object\\object.vert.spv";
        FragPath = $"{Define.AssetPath}\\Shaders\\Object\\object.frag.spv";
    }

    public void LoadObj(string objPath)
    {
        if (objPath == null) return;
        if (objPath == string.Empty) return;
        if (objPath.Equals(this.objPath)) return;
        
        this.objPath = objPath;
        Helper.LoadObj(objPath, out indices, out positions, out uvs, out normals);
        
        if (indices == null || indices.Length == 0 || positions == null || positions.Length == 0)
        {
            AABB = AABB.None;
        }
        else
        {
            AABB aabb = AABB.None;

            foreach (ushort i in indices)
            {
                Vector3 p = positions[i];
                aabb.Encapsulate(p);
            }
            
            AABB = aabb;
        }
    }

    public override void DrawSetting()
    {
        #region ObjPath
        if (ImGui.Button("Obj: "))
        {
            showSelectObjPanel = true;
        }
        ImGui.SameLine();
        ImGui.Text(objPath.Substring(Define.AssetPath.Length + 1));
        
        if (showSelectObjPanel)
        {
            if (ImGui.Begin($"Select##Obj", ref showSelectObjPanel))
            {
                foreach (string fullPath in FileSystem.GetLikeFiles(".obj"))
                {
                    if (ImGui.Selectable(fullPath.Substring(Define.AssetPath.Length + 1)))
                    {
                        ObjPath = fullPath;
                        showSelectObjPanel = false;
                    }
                }

                ImGui.End();
            }
        }
        #endregion
        
        #region ShaderPath
        if (ImGui.Button("Vert: "))
        {
            showSelectVertPanel = true;
        }
        ImGui.SameLine();
        ImGui.Text(VertPath.Substring(Define.AssetPath.Length + 1));
        
        if (showSelectVertPanel)
        {
            if (ImGui.Begin($"Select##Vert", ref showSelectVertPanel))
            {
                foreach (string fullPath in FileSystem.GetLikeFiles(".spv"))
                {
                    if (ImGui.Selectable(fullPath.Substring(Define.AssetPath.Length + 1)))
                    {
                        VertPath = fullPath;
                        showSelectVertPanel = false;
                    }
                }
                
                ImGui.End();
            }
        }
        
        if (ImGui.Button("Frag: "))
        {
            showSelectFragPanel = true;
        }
        ImGui.SameLine();
        ImGui.Text(FragPath.Substring(Define.AssetPath.Length + 1));
        
        if (showSelectFragPanel)
        {
            if (ImGui.Begin($"Select##Frag", ref showSelectFragPanel))
            {
                foreach (string fullPath in FileSystem.GetLikeFiles(".spv"))
                {
                    if (ImGui.Selectable(fullPath.Substring(Define.AssetPath.Length + 1)))
                    {
                        FragPath = fullPath;
                        showSelectFragPanel = false;
                    }
                }
                
                ImGui.End();
            }
        }
        #endregion

        if (positions != null && positions.Length > 0)
        {
            ImGui.Indent(20);
            if (ImGui.CollapsingHeader("Position", ImGuiTreeNodeFlags.None))
            {
                foreach (Vector3 position in positions)
                {
                    ImGui.Text(position.ToString());
                }
            }
            ImGui.Unindent(20);
        }
        
        if (uvs != null && uvs.Length > 0)
        {
            ImGui.Indent(20);
            if (ImGui.CollapsingHeader("UV", ImGuiTreeNodeFlags.None))
            {
                foreach (Vector2 uv in uvs)
                {
                    ImGui.Text(uv.ToString());
                }
            }
            ImGui.Unindent(20);
        }
        
        if (normals != null && normals.Length > 0)
        {
            ImGui.Indent(20);
            if (ImGui.CollapsingHeader("Normal", ImGuiTreeNodeFlags.None))
            {
                foreach (Vector3 normals in normals)
                {
                    ImGui.Text(normals.ToString());
                }
            }
            ImGui.Unindent(20);
        }
    }

    public override string ToString()
    {
        return "Mesh";
    }
}