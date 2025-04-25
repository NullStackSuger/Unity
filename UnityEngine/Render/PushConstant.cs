using System.Numerics;
using System.Runtime.InteropServices;
using SharpVk;

namespace UnityEngine;

[StructLayout(LayoutKind.Sequential)]
public partial struct PushConstant
{
    public Matrix4x4 model;

    public static PushConstantRange GetRange()
    {
        return new PushConstantRange()
        {
            Offset = 0,
            Size = (uint)Marshal.SizeOf<PushConstant>(),
            StageFlags = ShaderStageFlags.Vertex | ShaderStageFlags.Fragment,
        };
    }
}