using System.Numerics;
using ImGuiNET;

namespace UnityEngine;

public class DirectionLightComponent : MonoBehaviour
{
    public float intensity = 0.8f;
    public Color color = Color.White;
    
    public Matrix4x4 View()
    {
        TransformComponent transform = gameObject.transform;
        Vector3 zAxis = Vector3.Normalize(transform.Forward);
        Vector3 xAxis = Vector3.Normalize(Vector3.Cross(transform.Up, zAxis));
        Vector3 yAxis = Vector3.Cross(zAxis, xAxis);

        return new Matrix4x4
        (
            xAxis.X, yAxis.X, zAxis.X, 0,
            xAxis.Y, yAxis.Y, zAxis.Y, 0,
            xAxis.Z, yAxis.Z, zAxis.Z, 0,
            -Vector3.Dot(xAxis, transform.position), -Vector3.Dot(yAxis, transform.position),
            -Vector3.Dot(zAxis, transform.position), 1
        );
    }
    
    public Matrix4x4 Projection()
    {
        // 正交投影适合方向光（视锥体平行）
        const float left = -5f;
        const float right = 5f;
        const float bottom = -5f;
        const float top = 5f;
        const float near = 0.1f;
        const float far = 100f;
        
        Matrix4x4 mat = Matrix4x4.Identity;
        mat.M11 = 2.0f / (right - left);
        mat.M22 = 2.0f / (top - bottom);
        mat.M33 = 1.0f / (far - near);
        mat.M41 = -(right + left) / (right - left);
        mat.M42 = -(top + bottom) / (top - bottom);
        mat.M43 = -near / (far - near);
        return mat;
    }

    public override void DrawSetting()
    {
        ImGui.DragFloat("Intensity", ref intensity);

        Vector4 color = this.color;
        if (ImGui.DragFloat4("Color", ref color))
        {
            this.color = color;
        }
    }

    public override string ToString()
    {
        return "Direction Light";
    }
}