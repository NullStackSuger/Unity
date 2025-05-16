using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.SPIRV;

namespace UnityEngine;

public class DefaultShadowShader : ShadowShader
{
    protected override void Awake()
    {
        base.Awake();
        
        #region 顶点输入
        Vertex[] vs = new Vertex[mesh.positions.Length];
        for (int j = 0; j < vs.Length; j++)
        {
            vs[j] = new Vertex() { position = mesh.positions[j] };
        }
        indexBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)(mesh.indices.Length * sizeof(ushort)), BufferUsage.IndexBuffer));
        device.UpdateBuffer(indexBuffer, 0, mesh.indices);
        vertexBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)(vs.Length * Marshal.SizeOf<Vertex>()), BufferUsage.VertexBuffer));
        device.UpdateBuffer(vertexBuffer, 0, vs);
        #endregion
        
        #region Uniform Buffer
        Set("M", new MUniform(mesh.gameObject.transform.Model));
        Set("VP", new VPUniform(Light.Main.View, Light.Main.Projection));
        var resourceLayout = CreateResourceLayout();
        resourceSet = CreateResourceSet(resourceLayout);
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
                    new ShaderDescription(ShaderStages.Vertex, File.ReadAllBytes($"{Define.AssetPath}\\Shaders\\Shadow\\DefaultShadow.vert.spv"), "main"),
                    new ShaderDescription(ShaderStages.Fragment, File.ReadAllBytes($"{Define.AssetPath}\\Shaders\\Shadow\\DefaultShadow.frag.spv"), "main")
                )
            ),
            Outputs = frameBuffer.OutputDescription,
        });
    }

    public override void Update()
    {
        base.Update();

        Set("M", new MUniform(mesh.gameObject.transform.Model));
        Set("VP", new VPUniform(Light.Main.View, Light.Main.Projection));
    }
    
    public override string ToString()
    {
        return nameof(DefaultShadowShader);
    }

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
    private struct MUniform
    {
        public Matrix4x4 model;

        public MUniform(Matrix4x4 model)
        {
            this.model = model;
        }
    }
    
    [StructLayout(LayoutKind.Sequential)]
    private struct VPUniform
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