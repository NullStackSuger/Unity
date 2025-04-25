using System.Numerics;
using System.Runtime.InteropServices;
using SharpVk;

namespace UnityEngine;

public partial struct VertexInput
{
    public Vector3 position;
    public Vector3 color;
    public Vector2 uv;

    // TODO VertexInput
    public static VertexInputBindingDescription[] GetBinding()
    {
        return new[]
        {
            new VertexInputBindingDescription()
            {
                Binding = 0,
                Stride = (uint)Marshal.SizeOf<VertexInput>(),
                InputRate = VertexInputRate.Vertex,
            }
        };
    }

    public static VertexInputAttributeDescription[] GetAttributes()
    {
        const Format vector3 = Format.R32G32B32SFloat;
        const Format vector2 = Format.R32G32SFloat;
        return new[]
        {
            new VertexInputAttributeDescription()
            {
                Binding = 0,
                Location = 0,
                Format = vector3,
                Offset = (uint)Marshal.OffsetOf<VertexInput>("position")
            },
            new VertexInputAttributeDescription
            {
                Binding = 0,
                Location = 1,
                Format = vector3,
                Offset = (uint)Marshal.OffsetOf<VertexInput>("color")
            },
            new VertexInputAttributeDescription()
            {
                Binding = 0,
                Location = 2,
                Format = vector2,
                Offset = (uint)Marshal.OffsetOf<VertexInput>("uv")
            }
        };
    }
}