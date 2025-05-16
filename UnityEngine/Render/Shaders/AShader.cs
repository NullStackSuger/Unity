using System.Runtime.CompilerServices;
using Veldrid;

namespace UnityEngine;

/// <summary>
/// 对应顶点片元着色器, 以及启动的设置
/// </summary>
public abstract class AShader
{
    public void Awake(GraphicsDevice device, Framebuffer frameBuffer, MeshComponent mesh)
    {
        this.device = device;
        this.frameBuffer = frameBuffer;
        this.mesh = mesh;
        parameters = new ShaderParameterLibrary();
        Awake();
    }
    
    protected virtual void Awake()
    {
        
    }

    public virtual void Update()
    {
        
    }

    /// <summary>
    /// Create & Update Uniform Buffer
    /// </summary>
    public DeviceBuffer Set<T>(string name, T value) where T : unmanaged
    {
        // Create
        if (!parameters.uniformBuffers.TryGetValue(name, out DeviceBuffer buffer))
        {
            uint size = (uint)Unsafe.SizeOf<T>();
            buffer = device.ResourceFactory.CreateBuffer(new BufferDescription(size, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            parameters.uniformBuffers[name] = buffer;

            var element = new ResourceLayoutElementDescription(name, ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment);
            parameters.layout.Add((element, buffer));
        }
        
        // Update
        device.UpdateBuffer(buffer, 0, ref value);

        return buffer;
    }

    /// <summary>
    /// Create Texture
    /// </summary>
    public Sampler Set(string name, Texture texture)
    {
        if (!parameters.textures.TryGetValue(name, out Texture _))
        {
            Sampler sampler = device.ResourceFactory.CreateSampler(new SamplerDescription(SamplerAddressMode.Clamp, SamplerAddressMode.Clamp, SamplerAddressMode.Clamp, SamplerFilter.MinLinear_MagLinear_MipPoint, null, 0, 0, 0, 0, SamplerBorderColor.OpaqueBlack));
            parameters.textures[name] = texture;
            parameters.samplers[name] = sampler;

            var textureElement = new ResourceLayoutElementDescription(name, ResourceKind.TextureReadOnly, ShaderStages.Vertex | ShaderStages.Fragment);
            var samplerElement = new ResourceLayoutElementDescription($"{name}Sampler", ResourceKind.Sampler, ShaderStages.Vertex | ShaderStages.Fragment);
            parameters.layout.Add((textureElement, texture));
            parameters.layout.Add((samplerElement, sampler));
            
            return sampler;
        }
        else
        {
            Debug.Warning($"{name} is already in use.");
            return parameters.samplers[name];
        }
    }

    /// <summary>
    /// Update Texture
    /// </summary>
    public void Set(string name, byte[] value)
    {
        if (!parameters.textures.TryGetValue(name, out Texture texture))
        {
            Debug.Warning($"{name} is not in use.");
        }
        else
        {
            device.UpdateTexture(texture, value, 0, 0, 0, texture.Width, texture.Height, 1, 0, 0);
        }
    }

    protected ResourceLayout CreateResourceLayout()
    {
        return device.ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription(parameters.layout.Select(kv => kv.Item1).ToArray()));
    }
    protected ResourceSet CreateResourceSet(ResourceLayout layout)
    {
        return device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(layout, parameters.layout.Select(kv => kv.Item2).ToArray()));
    }

    protected GraphicsDevice device;
    protected Framebuffer frameBuffer;
    protected MeshComponent mesh;
    protected ShaderParameterLibrary parameters;
    public uint IndexCount => (uint)mesh.indices.Length;
    public DeviceBuffer indexBuffer { get; protected set; }
    public DeviceBuffer vertexBuffer { get; protected set; }
    public ResourceSet resourceSet { get; protected set; }
    public Pipeline pipeline { get; protected set; }

    protected class ShaderParameterLibrary
    {
        public Dictionary<string, DeviceBuffer> uniformBuffers = new();
        public Dictionary<string, Texture> textures = new();
        public Dictionary<string, Sampler> samplers = new();
        public List<(ResourceLayoutElementDescription, BindableResource)> layout = new();
    }
}