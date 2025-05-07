using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.SPIRV;

namespace UnityEngine;

public class ObjectsRenderPass : RenderPass
{
    public ObjectsRenderPass(GraphicsDevice device, uint width, uint height)
    {
        // 更新输入信息
        cube = Scene.ActiveScene.Find("Cube"); // TODO Test
        mvpUniform = new MVPUniform()
        {
            model = cube.transform.Model,
            view = Camera.Main.View,
            projection = Camera.Main.Projection
        };
        MeshComponent meshComponent = cube.GetComponent<MeshComponent>();
        indices = meshComponent.indices;
        vertices = new Vertex[meshComponent.positions.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vertex() { position = meshComponent.positions[i], uv = meshComponent.uvs[i] };
        }
        
        // 创建Texture接收结果
        result = device.ResourceFactory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1, PixelFormat.B8_G8_R8_A8_UNorm, TextureUsage.RenderTarget | TextureUsage.Sampled));
        Texture depthResult = device.ResourceFactory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1, PixelFormat.D24_UNorm_S8_UInt, TextureUsage.DepthStencil));
        frameBuffer = device.ResourceFactory.CreateFramebuffer(new FramebufferDescription(depthResult, result));

        this.device = device;
        
        // 顶点Buffer
        vertexBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)(vertices.Length * Marshal.SizeOf<Vertex>()), BufferUsage.VertexBuffer));
        device.UpdateBuffer(vertexBuffer, 0, vertices);
        indexBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)(indices.Length * sizeof(ushort)), BufferUsage.IndexBuffer));
        device.UpdateBuffer(indexBuffer, 0, indices);
        
        // Uniform Buffer
        mvpBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<MVPUniform>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        device.UpdateBuffer(mvpBuffer, 0, ref mvpUniform);
        var resourcesLayout = device.ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription
        (
            new ResourceLayoutElementDescription("MVP", ResourceKind.UniformBuffer, ShaderStages.Vertex)
        ));
        resourceSet = device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(resourcesLayout, mvpBuffer));
        
        // Pipeline
        pipeline = device.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription()
        {
            BlendState = BlendStateDescription.SingleOverrideBlend,
            DepthStencilState = new DepthStencilStateDescription()
            {
                DepthTestEnabled = true,
                DepthWriteEnabled = true,
                DepthComparison = ComparisonKind.LessEqual,
            },
            RasterizerState = new RasterizerStateDescription
            (
                FaceCullMode.Back,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                true,
                false
            ),
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ResourceLayouts = [resourcesLayout],
            ShaderSet = new ShaderSetDescription
            (
                new VertexLayoutDescription[] { Vertex.GetLayout() },
                device.ResourceFactory.CreateFromSpirv
                (
                    new ShaderDescription(ShaderStages.Vertex, File.ReadAllBytes($"{Define.AssetPath}\\Shaders\\Object\\object.vert.spv"), "main"),
                    new ShaderDescription(ShaderStages.Fragment, File.ReadAllBytes($"{Define.AssetPath}\\Shaders\\Object\\object.frag.spv"), "main")
                )
            ),
            Outputs = frameBuffer.OutputDescription,
        });
    }
    
    public override void Tick(CommandList commandList)
    {
        commandList.SetFramebuffer(frameBuffer);
        commandList.ClearColorTarget(0, new RgbaFloat(0.1f, 0.1f, 0.1f, 1.0f));
        commandList.ClearDepthStencil(1, 0);
        
        // Update Uniform
        mvpUniform.model = cube.transform.Model;
        device.UpdateBuffer(mvpBuffer, 0, ref mvpUniform);
        
        commandList.SetVertexBuffer(0, vertexBuffer);
        commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt16);
        commandList.SetPipeline(pipeline);
        commandList.SetGraphicsResourceSet(0, resourceSet);
        commandList.DrawIndexed((uint)indices.Length, 1, 0, 0, 0);
    }
    
    public readonly Texture result;
    private readonly GraphicsDevice device;
    private readonly Framebuffer frameBuffer;
    private readonly DeviceBuffer vertexBuffer;
    private readonly DeviceBuffer indexBuffer;
    private readonly DeviceBuffer mvpBuffer;
    private readonly ResourceSet resourceSet;
    private readonly Pipeline pipeline;
    
    private readonly Vertex[] vertices;
    private readonly ushort[] indices;
    private MVPUniform mvpUniform;

    private GameObject cube;
}