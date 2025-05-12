using Veldrid;

namespace UnityEngine;

public abstract class ObjectShader : AShader
{
    public virtual void Awake(GraphicsDevice device, Framebuffer frameBuffer, MeshComponent mesh, Texture shadowMap)
    {
        Awake(device, frameBuffer, mesh);
        this.shadowMap = shadowMap;
    }
    
    protected Texture shadowMap;
}