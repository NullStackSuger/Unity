using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.SPIRV;

namespace UnityEngine;

public class DefaultObjectShader : ObjectShader
{
    protected override void Awake()
    {
        base.Awake();

        DirectionLightComponent light = Light.Main;
        
        #region 顶点输入
        Vertex[] vs = new Vertex[mesh.positions.Length];
        for (int i = 0; i < vs.Length; i++)
        {
            vs[i] = new Vertex() { position = mesh.positions[i], uv = mesh.uvs[i], normal = mesh.normals[i] };
        }
        indexBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)(mesh.indices.Length * sizeof(ushort)), BufferUsage.IndexBuffer));
        device.UpdateBuffer(indexBuffer, 0, mesh.indices);
        vertexBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)(vs.Length * Marshal.SizeOf<Vertex>()), BufferUsage.VertexBuffer));
        device.UpdateBuffer(vertexBuffer, 0, vs);
        #endregion
        
        #region Uniform Buffer
        Set("M", new MUniform(mesh.gameObject.transform.Model));
        Set("VP", new VPUniform(Camera.Main.View, Camera.Main.Projection));
        Set("Light", new LightUniform(Light.Main.View, Light.Main.Projection, light.gameObject.transform.Forward, light.intensity, light.color));
        Set("shadowMap", shadowMap);
        
        var resourceLayout = CreateResourceLayout();
        resourceSet = CreateResourceSet(resourceLayout);
        #endregion
        
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
                    new ShaderDescription(ShaderStages.Vertex, File.ReadAllBytes($"{Define.AssetPath}\\Shaders\\Object\\DefaultObject.vert.spv"), "main"),
                    new ShaderDescription(ShaderStages.Fragment, File.ReadAllBytes($"{Define.AssetPath}\\Shaders\\Object\\DefaultObject.frag.spv"), "main")
                )
            ),
            Outputs = frameBuffer.OutputDescription,
        });
    }

    public override void Update()
    {
        base.Update();
        
        DirectionLightComponent light = Light.Main;

        Set("M", new MUniform(mesh.gameObject.transform.Model));
        Set("VP", new VPUniform(Camera.Main.View, Camera.Main.Projection));
        Set("Light", new LightUniform(Light.Main.View, Light.Main.Projection, light.gameObject.transform.Forward, light.intensity, light.color));
    }

    public override string ToString()
    {
        return nameof(DefaultObjectShader);
    }
    
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