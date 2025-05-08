using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.SPIRV;

namespace UnityEngine;

public class ShadowRenderPass : RenderPass
{
    public ShadowRenderPass(GraphicsDevice device, uint width, uint height,  IReadOnlyList<(GameObject, MeshComponent)> objs)
    {
        // 更新输入信息
        cube = objs.First().Item1; // TODO Test
        mvpUniform = new MVPUniform()
        {
            model = cube.transform.Model,
            view = Camera.Main.View,
            projection = Camera.Main.Projection
        };
        MeshComponent meshComponent = objs.First().Item2;
        indices = meshComponent.indices;
        vertices = new Vertex[meshComponent.positions.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vertex() { position = meshComponent.positions[i] };
        }
        
        // 创建Texture接收结果
        shadowMap = device.ResourceFactory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1, PixelFormat.D24_UNorm_S8_UInt, TextureUsage.DepthStencil | TextureUsage.Sampled));
        frameBuffer = device.ResourceFactory.CreateFramebuffer(new FramebufferDescription(shadowMap));
        
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
            BlendState = BlendStateDescription.Empty, // 不需要颜色混合
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
                    new ShaderDescription(ShaderStages.Vertex, File.ReadAllBytes($"{Define.AssetPath}\\Shaders\\Shadow\\shadow.vert.spv"), "main"),
                    new ShaderDescription(ShaderStages.Fragment, File.ReadAllBytes($"{Define.AssetPath}\\Shaders\\Shadow\\shadow.frag.spv"), "main")
                )
            ),
            Outputs = frameBuffer.OutputDescription,
        });
    }
    
    public override void Tick(CommandList commandList)
    {
        commandList.SetFramebuffer(frameBuffer);
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

    public readonly Texture shadowMap;
    private readonly Framebuffer frameBuffer;
    private readonly GraphicsDevice device;
    private readonly DeviceBuffer vertexBuffer;
    private readonly DeviceBuffer indexBuffer;
    private readonly DeviceBuffer mvpBuffer;
    private readonly ResourceSet resourceSet;
    private readonly Pipeline pipeline;
    
    private readonly Vertex[] vertices;
    private readonly ushort[] indices;
    private MVPUniform mvpUniform;
    
    private readonly GameObject cube;
    
    private struct Vertex
    {
        public Vector3 position;

        public static VertexLayoutDescription GetLayout()
        {
            return new VertexLayoutDescription
            (
                new VertexElementDescription("position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3)
            );
        }
    }
}