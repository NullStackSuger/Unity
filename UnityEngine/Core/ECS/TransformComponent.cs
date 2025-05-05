using System.Numerics;

namespace UnityEngine;

public class TransformComponent : MonoBehaviour
{
    public Vector3 position { get; private set; }
    public Quaternion rotation { get; private set; }
    public Vector3 scale { get; private set; }

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

    public Matrix4x4 worldMatrix
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
}