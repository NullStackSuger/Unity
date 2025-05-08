using System.Numerics;
using ImGuiNET;

namespace UnityEngine;

public class DirectionLightComponent : MonoBehaviour
{
    public float intensity = 0.8f;
    public Vector3 direction = Vector3.Normalize(new Vector3(-1, -1, -1));
    public Color color = Color.White;

    public Matrix4x4 View()
    {
        return Matrix4x4.CreateLookAt(gameObject.transform.position, gameObject.transform.position + direction, Vector3.UnitY);
    }

    public Matrix4x4 Projection()
    {
        // 正交投影适合方向光（视锥体平行）
        const float orthoSize = 20f;
        return Matrix4x4.CreateOrthographicOffCenter(-orthoSize, orthoSize, -orthoSize, orthoSize, 0.1f, 100f);
    }

    public override void DrawSetting()
    {
        ImGui.DragFloat("Intensity", ref intensity);
        
        Vector3 direction = this.direction;
        if (ImGui.DragFloat3("Direction", ref direction))
        {
            this.direction = Vector3.Normalize(direction);
        }

        Vector4 color = this.color;
        if (ImGui.DragFloat4("Color", ref color))
        {
            this.color = color;
        }
    }
}