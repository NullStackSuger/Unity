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
        this.device = device;
        this.objs = objs.ToList();
        
        // 创建Texture接收结果
        shadowMap = device.ResourceFactory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1, PixelFormat.D24_UNorm_S8_UInt, TextureUsage.DepthStencil | TextureUsage.Sampled));
        frameBuffer = device.ResourceFactory.CreateFramebuffer(new FramebufferDescription(shadowMap));
        
        #region 顶点输入
        indexBuffers = new List<DeviceBuffer>();
        vertexBuffers = new List<DeviceBuffer>();
        for (int i = 0; i < objs.Count; i++)
        {
            MeshComponent meshComponent = objs[i].Item2;
            
            Vertex[] vs = new Vertex[meshComponent.positions.Length];
            for (int j = 0; j < vs.Length; j++)
            {
                vs[j] = new Vertex() { position = meshComponent.positions[j] };
            }
            
            indexBuffers.Add(device.ResourceFactory.CreateBuffer(new BufferDescription((uint)(meshComponent.indices.Length * sizeof(ushort)), BufferUsage.IndexBuffer)));
            device.UpdateBuffer(indexBuffers[i], 0, meshComponent.indices);
            vertexBuffers.Add(device.ResourceFactory.CreateBuffer(new BufferDescription((uint)(vs.Length * Marshal.SizeOf<Vertex>()), BufferUsage.VertexBuffer)));
            device.UpdateBuffer(vertexBuffers[i], 0, vs);
        }
        #endregion
        
        #region Uniform Buffer
        modelBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)objs.Count * ModelUniform.GetSize(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        lightVpBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<LightVpUniform>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        resourceLayout = device.ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription
        (
            new ResourceLayoutElementDescription("Model", ResourceKind.UniformBuffer, ShaderStages.Vertex, ResourceLayoutElementOptions.DynamicBinding),
            new ResourceLayoutElementDescription("LightVp", ResourceKind.UniformBuffer, ShaderStages.Vertex)
        ));
        resourceSet = device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(resourceLayout, modelBuffer, lightVpBuffer));
        
        LightVpUniform lightVpUniform = new LightVpUniform(Light.Main.View, Light.Main.Projection);
        device.UpdateBuffer(lightVpBuffer, 0, ref lightVpUniform);
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
        commandList.SetFramebuffer(frameBuffer);
        commandList.ClearDepthStencil(1, 0);
        commandList.SetPipeline(pipeline);

        for (int i = 0; i < objs.Count; i++)
        {
            GameObject obj = objs[i].Item1;
            MeshComponent meshComponent = objs[i].Item2;

            uint offset = 0;
            ModelUniform mUniform = new ModelUniform(obj.transform.Model);
            offset += (uint)i * ModelUniform.GetSize();
            device.UpdateBuffer(modelBuffer, offset, ref mUniform);
            
            commandList.SetVertexBuffer(0, vertexBuffers[i]);
            commandList.SetIndexBuffer(indexBuffers[i], IndexFormat.UInt16);
            commandList.SetGraphicsResourceSet(0, resourceSet, [offset]);
            commandList.DrawIndexed((uint)meshComponent.indices.Length, 1, 0, 0, 0);
        }
    }
    
    public readonly Texture shadowMap;
    private readonly Framebuffer frameBuffer;
    private readonly GraphicsDevice device;
    private readonly List<DeviceBuffer> vertexBuffers;
    private readonly List<DeviceBuffer> indexBuffers;
    private readonly DeviceBuffer modelBuffer;
    private readonly DeviceBuffer lightVpBuffer;
    private readonly ResourceLayout resourceLayout;
    private readonly ResourceSet resourceSet;
    private readonly Pipeline pipeline;

    private readonly List<(GameObject, MeshComponent)> objs;
    
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
        
        public static uint GetSize()
        {
            uint minOffsetAlignment = 256;
            uint rowSize = (uint)Unsafe.SizeOf<ModelUniform>();
            return (rowSize + minOffsetAlignment - 1) / minOffsetAlignment * minOffsetAlignment;
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