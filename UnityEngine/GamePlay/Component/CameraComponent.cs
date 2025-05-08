using System.Numerics;
using ImGuiNET;

namespace UnityEngine;

public abstract class CameraComponent : MonoBehaviour
{
    public Matrix4x4 View()
    {
        TransformComponent transform = this.gameObject.transform;
        // 相机的 forward 是 -Z 轴方向
        Vector3 forward = Vector3.Transform(-Vector3.UnitZ, transform.rotation);
        Vector3 up = Vector3.Transform(Vector3.UnitY, transform.rotation);
        return Matrix4x4.CreateLookAt(transform.position, transform.position + forward, up);
    }
    
    public abstract Matrix4x4 Projection();
}

public class OrthographicCameraComponent : CameraComponent
{
    public override Matrix4x4 Projection()
    {
        Matrix4x4 mat = Matrix4x4.Identity;
        mat[0, 0] = 2.0f / (right - left);
        mat[1, 1] = 2.0f / (top - bottom);
        mat[2, 2] = 1.0f / (far - near);
        mat[3, 0] = -(right + left) / (right - left);
        mat[3, 1] = -(top + bottom) / (top - bottom);
        mat[3, 2] = -near / (far - near);
        return mat;
    }

    public float left = -5;
    public float right = 5;
    public float bottom = -5;
    public float top = 5;
    public float near = 1;
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