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
        this.device = device;
        this.objs = objs.ToList();
        lightComponent = Light.Main;
        
        // 创建Texture接收结果
        result = device.ResourceFactory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1, PixelFormat.B8_G8_R8_A8_UNorm, TextureUsage.RenderTarget | TextureUsage.Sampled));
        Texture depthResult = device.ResourceFactory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1, PixelFormat.D24_UNorm_S8_UInt, TextureUsage.DepthStencil));
        frameBuffer = device.ResourceFactory.CreateFramebuffer(new FramebufferDescription(depthResult, result));
        
        #region 顶点输入
        indexBuffers = new List<DeviceBuffer>();
        vertexBuffers = new List<DeviceBuffer>();
        for (int i = 0; i < objs.Count; i++)
        {
            MeshComponent meshComponent = objs[i].Item2;
            
            Vertex[] vs = new Vertex[meshComponent.positions.Length];
            for (int j = 0; j < vs.Length; j++)
            {
                vs[j] = new Vertex() { position = meshComponent.positions[j], uv = meshComponent.uvs[j], normal = meshComponent.normals[j] };
            }
            
            indexBuffers.Add(device.ResourceFactory.CreateBuffer(new BufferDescription((uint)(meshComponent.indices.Length * sizeof(ushort)), BufferUsage.IndexBuffer)));
            device.UpdateBuffer(indexBuffers[i], 0, meshComponent.indices);
            vertexBuffers.Add(device.ResourceFactory.CreateBuffer(new BufferDescription((uint)(vs.Length * Marshal.SizeOf<Vertex>()), BufferUsage.VertexBuffer)));
            device.UpdateBuffer(vertexBuffers[i], 0, vs);
        }
        #endregion

        #region Uniform Buffer
        mBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)objs.Count * MUniform.GetSize(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        vpBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<VPUniform>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        lightBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<LightUniform>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        shadowMapSampler = device.ResourceFactory.CreateSampler(new SamplerDescription(SamplerAddressMode.Clamp, SamplerAddressMode.Clamp, SamplerAddressMode.Clamp, SamplerFilter.MinLinear_MagLinear_MipPoint, null, 0, 0, 0, 0, SamplerBorderColor.OpaqueBlack));
        var resourcesLayout = device.ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription
        (
            new ResourceLayoutElementDescription("M", ResourceKind.UniformBuffer, ShaderStages.Vertex, ResourceLayoutElementOptions.DynamicBinding),
            new ResourceLayoutElementDescription("VP", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("Light", ResourceKind.UniformBuffer, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("shadowMap", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("shadowMapSampler", ResourceKind.Sampler, ShaderStages.Fragment)
        ));
        resourceSet = device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(resourcesLayout, mBuffer, vpBuffer, lightBuffer, shadowMap, shadowMapSampler));
        
        VPUniform vpUniform = new VPUniform(Camera.Main.View, Camera.Main.Projection);
        device.UpdateBuffer(vpBuffer, 0, ref vpUniform);
        LightUniform lightUniform = new LightUniform(Light.Main.View, Light.Main.Projection, lightComponent.gameObject.transform.Forward, lightComponent.intensity, lightComponent.color);
        device.UpdateBuffer(lightBuffer, 0, ref lightUniform);
        #endregion
        
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
                    new ShaderDescription(ShaderStages.Vertex, File.ReadAllBytes(objs.First().Item2.VertPath), "main"),
                    new ShaderDescription(ShaderStages.Fragment, File.ReadAllBytes(objs.First().Item2.FragPath), "main")
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
        commandList.SetPipeline(pipeline);
        
        for (int i = 0; i < objs.Count; i++)
        {
            GameObject obj = objs[i].Item1;
            MeshComponent meshComponent = objs[i].Item2;

            uint offset = 0;
            MUniform mUniform = new MUniform(obj.transform.Model);
            offset += (uint)i * MUniform.GetSize();
            device.UpdateBuffer(mBuffer, offset, ref mUniform);
            
            commandList.SetVertexBuffer(0, vertexBuffers[i]);
            commandList.SetIndexBuffer(indexBuffers[i], IndexFormat.UInt16);
            commandList.SetGraphicsResourceSet(0, resourceSet, [offset]);
            commandList.DrawIndexed((uint)meshComponent.indices.Length, 1, 0, 0, 0);
        }
    }
    
    public readonly Texture result;
    private readonly Framebuffer frameBuffer;
    private readonly GraphicsDevice device;
    private readonly List<DeviceBuffer> indexBuffers;
    private readonly List<DeviceBuffer> vertexBuffers;
    private readonly DeviceBuffer mBuffer;
    private readonly DeviceBuffer vpBuffer;
    private readonly DeviceBuffer lightBuffer;
    private readonly Sampler shadowMapSampler;
    private readonly ResourceSet resourceSet;
    private readonly Pipeline pipeline;
    
    private readonly List<(GameObject, MeshComponent)> objs;
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
    public struct MUniform
    {
        public Matrix4x4 model;

        public MUniform(Matrix4x4 model)
        {
            this.model = model;
        }

        public static uint GetSize()
        {
            uint minOffsetAlignment = 256;
            uint rowSize = (uint)Unsafe.SizeOf<MUniform>();
            return (rowSize + minOffsetAlignment - 1) / minOffsetAlignment * minOffsetAlignment;
        }
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct VPUniform
    {
        public Matrix4x4 view;
        public Matrix4x4 projection;

        public VPUniform(Matrix4x4 view, Matrix4x4 projection)
        {
            this.view = view;
            this.projection = projection;
        }
    }
}