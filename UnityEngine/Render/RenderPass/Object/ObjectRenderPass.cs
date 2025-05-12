using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.SPIRV;

namespace UnityEngine;

public sealed class ObjectRenderPass : RenderPass
{
    public ObjectRenderPass(GraphicsDevice device, uint width, uint height, IReadOnlyList<(GameObject, MeshComponent)> objs, Texture shadowMap)
    {
        // 创建Texture接收结果
        result = device.ResourceFactory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1, PixelFormat.B8_G8_R8_A8_UNorm, TextureUsage.RenderTarget | TextureUsage.Sampled));
        Texture depthResult = device.ResourceFactory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1, PixelFormat.D24_UNorm_S8_UInt, TextureUsage.DepthStencil));
        frameBuffer = device.ResourceFactory.CreateFramebuffer(new FramebufferDescription(depthResult, result));

        shaders = new ObjectShader[objs.Count];
        for (int i = 0; i < shaders.Length; i++)
        {
            shaders[i] = objs[i].Item2.objectShader;
            shaders[i].Awake(device, frameBuffer, objs[i].Item2, shadowMap);
        }
    }
    
    public override void Tick(CommandList commandList)
    {
        commandList.SetFramebuffer(frameBuffer);
        commandList.ClearColorTarget(0, new RgbaFloat(0.1f, 0.1f, 0.1f, 1.0f));
        commandList.ClearDepthStencil(1, 0);

        foreach (var shader in shaders)
        {
            shader.Update();
            commandList.SetPipeline(shader.pipeline);
            commandList.SetVertexBuffer(0, shader.vertexBuffer);
            commandList.SetIndexBuffer(shader.indexBuffer, IndexFormat.UInt16);
            commandList.SetGraphicsResourceSet(0, shader.resourceSet);
            commandList.DrawIndexed(shader.IndexCount, 1, 0, 0, 0);
        }
    }
    
    public readonly Texture result;
    private readonly Framebuffer frameBuffer;
    private readonly ObjectShader[] shaders;
}