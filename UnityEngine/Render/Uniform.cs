using System.Numerics;
using System.Runtime.InteropServices;
using SharpVk;

namespace UnityEngine;

[StructLayout(LayoutKind.Sequential)]
public partial struct Uniform
{
    public Matrix4x4 view;
    public Matrix4x4 projection;
    
    // TODO Uniform
    public static DescriptorSetLayoutBinding GetBinding()
    {
        return new DescriptorSetLayoutBinding()
        {
            Binding = 0,
            DescriptorType = DescriptorType.UniformBuffer,
            StageFlags = ShaderStageFlags.Vertex | ShaderStageFlags.Fragment,
            DescriptorCount = 1,
        };
    }
}