using System.Diagnostics;
using System.Numerics;
using ImGuiNET;

namespace UnityEngine;

public class TransformComponent : MonoBehaviour
{
    public TransformComponent()
    {
        position = Vector3.Zero;
        rotation = Quaternion.Identity;
        scale = Vector3.One;
    }

    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public Vector3 worldPosition
    {
        get
        {
            Vector3 position = this.position;
            GameObject current = gameObject.Parent;
            while (current != null)
            {
                position += current.transform.position;
                current = current.Parent;
            }
            return position;
        }
    }
    public Quaternion worldRotation
    {
        get
        {
            Quaternion rotation = this.rotation;
            GameObject current = gameObject.Parent;
            while (current != null)
            {
                rotation = current.transform.rotation * rotation; // 父乘子
                current = current.Parent;
            }
            return rotation;
        }
    }

    public Matrix4x4 localMatrix => Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(position);

    public Matrix4x4 Model
    {
        get
        {
            Matrix4x4 matrix = localMatrix;
            GameObject current = gameObject.Parent;
            while (current != null)
            {
                matrix = current.transform.localMatrix * matrix;
                current = current.Parent;
            }
            return matrix;
        }
    }
    
    public override void DrawSetting()
    {
        Vector3 position = this.position;
        ImGui.Text("Position");
        if (ImGui.DragFloat3("##Position", ref position))
        {
            this.position = position;
        }
        
        Vector3 rotation = this.rotation.ToVector3();
        ImGui.Text("Rotation");
        if (ImGui.DragFloat3("##Rotation", ref rotation))
        {
            this.rotation = rotation.ToQuaternion();
        }
        
        Vector3 scale = this.scale;
        ImGui.Text("Scale");
        if (ImGui.DragFloat3("##Scale", ref scale))
        {
            this.scale = scale;
        }
    }

    public override string ToString()
    {
        return "Transform";
    }
}