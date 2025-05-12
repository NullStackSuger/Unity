using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.SPIRV;

namespace UnityEngine;

public class ShadowRenderPass : RenderPass
{
    public ShadowRenderPass(GraphicsDevice device, uint width, uint height, IReadOnlyList<(GameObject, MeshComponent)> objs)
    {
        // 创建Texture接收结果
        shadowMap = device.ResourceFactory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1, PixelFormat.D24_UNorm_S8_UInt, TextureUsage.DepthStencil | TextureUsage.Sampled));
        frameBuffer = device.ResourceFactory.CreateFramebuffer(new FramebufferDescription(shadowMap));

        shaders = new ShadowShader[objs.Count];
        for (int i = 0; i < shaders.Length; i++)
        {
            shaders[i] = objs[i].Item2.shadowShader;
            shaders[i].Awake(device, frameBuffer, objs[i].Item2);
        }
    }
    
    public override void Tick(CommandList commandList)
    {
        commandList.SetFramebuffer(frameBuffer);
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
    
    public readonly Texture shadowMap;
    private readonly Framebuffer frameBuffer;
    private readonly ShadowShader[] shaders;
}