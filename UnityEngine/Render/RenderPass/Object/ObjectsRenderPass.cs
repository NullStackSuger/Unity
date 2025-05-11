using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.SPIRV;

namespace UnityEngine;

public sealed class ObjectsRenderPass : RenderPass
{
    public ObjectsRenderPass(GraphicsDevice device, uint width, uint height, IReadOnlyList<(GameObject, MeshComponent)> objs, Texture shadowMap)
    {
        // 更新输入信息
        cube = objs.First().Item1; // TODO Test
        lightComponent = Light.Main;
        
        MeshComponent meshComponent = objs.First().Item2;
        indices = meshComponent.indices;
        vertices = new Vertex[meshComponent.positions.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vertex() { position = meshComponent.positions[i], uv = meshComponent.uvs[i], normal = meshComponent.normals[i] };
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
        lightBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<LightUniform>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        shadowMapSampler = device.ResourceFactory.CreateSampler(new SamplerDescription(SamplerAddressMode.Clamp, SamplerAddressMode.Clamp, SamplerAddressMode.Clamp, SamplerFilter.MinLinear_MagLinear_MipPoint, null, 0, 0, 0, 0, SamplerBorderColor.OpaqueBlack));
        var resourcesLayout = device.ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription
        (
            new ResourceLayoutElementDescription("MVP", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("Light", ResourceKind.UniformBuffer, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("shadowMap", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("shadowMapSampler", ResourceKind.Sampler, ShaderStages.Fragment)
        ));
        resourceSet = device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(resourcesLayout, mvpBuffer, lightBuffer, shadowMap, shadowMapSampler));
        
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
                    new ShaderDescription(ShaderStages.Vertex, File.ReadAllBytes(meshComponent.VertPath), "main"),
                    new ShaderDescription(ShaderStages.Fragment, File.ReadAllBytes(meshComponent.FragPath), "main")
                )
            ),
            Outputs = frameBuffer.OutputDescription,
        });
    }
    
    public override void Tick(CommandList commandList)
    {
        // Update Uniform
        mvpUniform = new MVPUniform(cube.transform.Model, Camera.Main.View, Camera.Main.Projection);
        device.UpdateBuffer(mvpBuffer, 0, ref mvpUniform);
        lightUniform = new LightUniform(Light.Main.View, Light.Main.Projection, lightComponent.gameObject.transform.Forward, lightComponent.intensity, lightComponent.color);
        device.UpdateBuffer(lightBuffer, 0, ref lightUniform);
        
        commandList.SetFramebuffer(frameBuffer);
        commandList.ClearColorTarget(0, new RgbaFloat(0.1f, 0.1f, 0.1f, 1.0f));
        commandList.ClearDepthStencil(1, 0);
        commandList.SetVertexBuffer(0, vertexBuffer);
        commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt16);
        commandList.SetPipeline(pipeline);
        commandList.SetGraphicsResourceSet(0, resourceSet);
        commandList.DrawIndexed((uint)indices.Length, 1, 0, 0, 0);
    }
    
    public readonly Texture result;
    private readonly Framebuffer frameBuffer;
    private readonly GraphicsDevice device;
    private readonly DeviceBuffer vertexBuffer;
    private readonly DeviceBuffer indexBuffer;
    private readonly DeviceBuffer mvpBuffer;
    private readonly DeviceBuffer lightBuffer;
    private readonly Sampler shadowMapSampler;
    private readonly ResourceSet resourceSet;
    private readonly Pipeline pipeline;

    private readonly Vertex[] vertices;
    private readonly ushort[] indices;
    private MVPUniform mvpUniform;
    private LightUniform lightUniform;

    private readonly GameObject cube;
    private readonly DirectionLightComponent lightComponent;
    
    private struct Vertex
    {
        public Vector3 position;
        public Vector2 uv;
        public Vector3 normal;

        public static VertexLayoutDescription GetLayout()
        {
            return new VertexLayoutDescription
            (
                new VertexElementDescription("position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("uv", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3)
            );
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct LightUniform
    {
        public Matrix4x4 view;
        public Matrix4x4 projection;
        public Vector3 direction;
        public float intensity;
        public Vector4 color;

        public LightUniform(Matrix4x4 view, Matrix4x4 projection, Vector3 direction, float intensity, Vector4 color)
        {
            this.view = view;
            this.projection = projection;
            this.direction = direction;
            this.intensity = intensity;
            this.color = color;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct MVPUniform
    {
        public Matrix4x4 model;
        public Matrix4x4 view;
        public Matrix4x4 projection;

        public MVPUniform(Matrix4x4 model, Matrix4x4 view, Matrix4x4 projection)
        {
            this.model = model;
            this.view = view;
            this.projection = projection;
        }
    }
}