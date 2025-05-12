using Veldrid;

namespace UnityEngine;

/// <summary>
/// 对应顶点片元着色器, 以及启动的设置
/// </summary>
public abstract class AShader
{
    public virtual void Awake(GraphicsDevice device, Framebuffer frameBuffer, MeshComponent mesh)
    {
        this.device = device;
        this.frameBuffer = frameBuffer;
        this.mesh = mesh;
    }

    public virtual void Update()
    {
        
    }

    protected GraphicsDevice device;
    protected Framebuffer frameBuffer;
    protected MeshComponent mesh;
    public uint IndexCount => (uint)mesh.indices.Length;
    public DeviceBuffer indexBuffer { get; protected set; }
    public DeviceBuffer vertexBuffer { get; protected set; }
    public ResourceSet resourceSet { get; protected set; }
    public Pipeline pipeline { get; protected set; }
}