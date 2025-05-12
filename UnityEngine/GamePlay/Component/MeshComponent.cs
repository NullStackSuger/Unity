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
    public ShadowShader shadowShader;
    public ObjectShader objectShader;

    public AABB AABB { get; private set; }

    public ushort[] indices;
    public Vector3[] positions;
    public Vector2[] uvs;
    public Vector3[] normals;

    private bool showSelectObjPanel;
    private bool showSelectShadowShaderPanel;
    private bool showSelectObjectShaderPanel;

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

        #region Shader
        if (ImGui.Button($"{nameof(ShadowShader)}: "))
        {
            showSelectShadowShaderPanel = true;
        }
        ImGui.SameLine();
        ImGui.Text(shadowShader.ToString());
        if (showSelectShadowShaderPanel)
        {
            if (ImGui.Begin($"Select Shadow Shader", ref showSelectShadowShaderPanel))
            {
                foreach (var (name, type) in Define.TypeMap["Shader"])
                {
                    if (typeof(ShadowShader).IsAssignableFrom(type))
                    {
                        if (ImGui.Selectable(name))
                        {
                            shadowShader = Activator.CreateInstance(type) as ShadowShader;
                            showSelectShadowShaderPanel = false;
                        }
                    }
                }
                ImGui.End();
            }
        }
        
        if (ImGui.Button($"{nameof(ObjectShader)}: "))
        {
            showSelectObjectShaderPanel = true;
        }
        ImGui.SameLine();
        ImGui.Text(objectShader.ToString());
        if (showSelectObjectShaderPanel)
        {
            if (ImGui.Begin($"Select Object Shader", ref showSelectObjectShaderPanel))
            {
                foreach (var (name, type) in Define.TypeMap["Shader"])
                {
                    if (typeof(ObjectShader).IsAssignableFrom(type))
                    {
                        if (ImGui.Selectable(name))
                        {
                            objectShader = Activator.CreateInstance(type) as ObjectShader;
                            showSelectObjectShaderPanel = false;
                        }
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