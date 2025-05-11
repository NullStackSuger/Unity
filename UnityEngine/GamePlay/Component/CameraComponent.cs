using System.Numerics;
using ImGuiNET;

namespace UnityEngine;

public abstract class CameraComponent : MonoBehaviour
{
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
                -Vector3.Dot(xAxis, transform.position), -Vector3.Dot(yAxis, transform.position), -Vector3.Dot(zAxis, transform.position), 1
            );
        }
    
    public abstract Matrix4x4 Projection();
}

public class OrthographicCameraComponent : CameraComponent
{
    public override Matrix4x4 Projection()
    {
        Matrix4x4 mat = Matrix4x4.Identity;
        mat.M11 = 2.0f / (right - left);
        mat.M22 = 2.0f / (top - bottom);
        mat.M33 = 1.0f / (far - near);
        mat.M41 = -(right + left) / (right - left);
        mat.M42 = -(top + bottom) / (top - bottom);
        mat.M43 = -near / (far - near);
        return mat;
    }

    public float left = -5;
    public float right = 5;
    public float bottom = -5;
    public float top = 5;
    public float near = 0.1f;
    public float far = 100;

    public override void DrawSetting()
    {
        ImGui.PushItemWidth(125);
        
        ImGui.DragFloat("Left", ref left);
        ImGui.SameLine();
        ImGui.DragFloat("Right", ref right);
        
        ImGui.DragFloat("Bottom", ref bottom);
        ImGui.SameLine();
        ImGui.DragFloat("Top", ref top);
        
        ImGui.DragFloat("Near", ref near);
        ImGui.SameLine();
        ImGui.DragFloat("Far", ref far);
        
        ImGui.PopItemWidth();
    }

    public override string ToString()
    {
        return "Orthographic Camera";
    }
}

public class PerspectiveCameraComponent : CameraComponent
{
    public override Matrix4x4 Projection()
    {
        Debug.Assert(aspect - float.Epsilon <= 0.0f, $"({aspect}), ({float.Epsilon})");
        
        float tanHalfFovY = MathF.Tan(fovY * Helper.degToRad / 2.0f);
        
        Matrix4x4 mat = new Matrix4x4
        {
            [0, 0] = 1.0f / (aspect * tanHalfFovY),
            [1, 1] = 1.0f / tanHalfFovY,
            [2, 2] = far / (far - near),
            [2, 3] = 1.0f,
            [3, 2] = -(far * near) / (far - near)
        };
        return mat;
    }
    
    public float fovY = 60;
    public float aspect = 4f / 3f;
    public float near = 0.1f;
    public float far = 100;

    public override void DrawSetting()
    {
        ImGui.DragFloat("Fov(Angle)", ref fovY);

        ImGui.DragFloat("Aspect", ref aspect);
        
        ImGui.PushItemWidth(125);
        
        ImGui.DragFloat("Near", ref near);
        ImGui.SameLine();
        ImGui.DragFloat("Far", ref far);
        
        ImGui.PopItemWidth();
    }

    public override string ToString()
    {
        return "Perspective Camera";
    }
}