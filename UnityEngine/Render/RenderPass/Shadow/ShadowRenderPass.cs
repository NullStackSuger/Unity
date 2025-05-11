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
        // 更新输入信息
        cube = objs.First().Item1; // TODO Test
        MeshComponent meshComponent = objs.First().Item2;
        indices = meshComponent.indices;
        vertices = new Vertex[meshComponent.positions.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vertex() { position = meshComponent.positions[i] };
        }
        
        this.device = device;
        
        shadowMap = device.ResourceFactory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1, PixelFormat.D24_UNorm_S8_UInt, TextureUsage.DepthStencil | TextureUsage.Sampled));
        frameBuffer = device.ResourceFactory.CreateFramebuffer(new FramebufferDescription(shadowMap));
        
        // 顶点Buffer
        vertexBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)(vertices.Length * Marshal.SizeOf<Vertex>()), BufferUsage.VertexBuffer));
        device.UpdateBuffer(vertexBuffer, 0, vertices);
        indexBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)(indices.Length * sizeof(ushort)), BufferUsage.IndexBuffer));
        device.UpdateBuffer(indexBuffer, 0, indices);

        // Uniform Buffer
        modelBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<ModelUniform>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        lightVpBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<LightVpUniform>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        resourceLayout = device.ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription
        (
            new ResourceLayoutElementDescription("Model", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("LightVp", ResourceKind.UniformBuffer, ShaderStages.Vertex)
        ));
        resourceSet = device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(resourceLayout, modelBuffer, lightVpBuffer));
        
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
            ResourceLayouts = [resourceLayout],
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
        // Update Uniform
        modelUniform = new ModelUniform(cube.transform.Model);
        device.UpdateBuffer(modelBuffer, 0, ref modelUniform);
        lightVpUniform = new LightVpUniform(Light.Main.View, Light.Main.Projection);
        device.UpdateBuffer(lightVpBuffer, 0, ref lightVpUniform);
        
        commandList.SetFramebuffer(frameBuffer);
        commandList.ClearDepthStencil(1, 0);
        commandList.SetPipeline(pipeline);
        commandList.SetVertexBuffer(0, vertexBuffer);
        commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt16);
        commandList.SetGraphicsResourceSet(0, resourceSet);
        commandList.DrawIndexed((uint)indices.Length, 1, 0, 0, 0);
    }
    
    public readonly Texture shadowMap;
    private readonly Framebuffer frameBuffer;
    private readonly GraphicsDevice device;
    private readonly DeviceBuffer vertexBuffer;
    private readonly DeviceBuffer indexBuffer;
    private readonly DeviceBuffer modelBuffer;
    private readonly DeviceBuffer lightVpBuffer;
    private readonly ResourceLayout resourceLayout;
    private readonly ResourceSet resourceSet;
    private readonly Pipeline pipeline;
    
    private readonly Vertex[] vertices;
    private readonly ushort[] indices;
    private ModelUniform modelUniform;
    private LightVpUniform lightVpUniform;
    
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
    [StructLayout(LayoutKind.Sequential)]
    private struct ModelUniform
    {
        public Matrix4x4 model;

        public ModelUniform(Matrix4x4 model)
        {
            this.model = model;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct LightVpUniform
    {
        public Matrix4x4 view;
        public Matrix4x4 projection;

        public LightVpUniform(Matrix4x4 view, Matrix4x4 projection)
        {
            this.view = view;
            this.projection = projection;
        }
    }
}