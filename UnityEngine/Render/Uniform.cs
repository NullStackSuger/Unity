using System.Numerics;
using SharpVk;

namespace UnityEngine;

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