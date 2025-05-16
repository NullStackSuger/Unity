using Veldrid;

namespace UnityEngine;

public abstract class ObjectShader : AShader
{
    public void Awake(GraphicsDevice device, Framebuffer frameBuffer, MeshComponent mesh, Texture shadowMap)
    {
        this.shadowMap = shadowMap;
        Awake(device, frameBuffer, mesh);
    }
    
    protected Texture shadowMap;
}