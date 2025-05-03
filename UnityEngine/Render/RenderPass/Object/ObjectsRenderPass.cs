using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.SPIRV;

namespace UnityEngine;

public class ObjectsRenderPass : RenderPass
{
    public ObjectsRenderPass(GraphicsDevice device)
    {
        var window = Window.window;
        
        mvpUniform.projection = Helper.Perspective(Helper.ToRadians(50.0f), (float)window.Width / (float)window.Height, 0.1f, 100.0f);
        
        vertexBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)(vertices.Length * Marshal.SizeOf<Vertex>()), BufferUsage.VertexBuffer));
        device.UpdateBuffer(vertexBuffer, 0, vertices);
        indexBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)(indices.Length * sizeof(ushort)), BufferUsage.IndexBuffer));
        device.UpdateBuffer(indexBuffer, 0, indices);
        
        mvpBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<MVPUniform>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        device.UpdateBuffer(mvpBuffer, 0, ref mvpUniform);
        var resourcesLayout = device.ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription
        (
            new ResourceLayoutElementDescription("MVP", ResourceKind.UniformBuffer, ShaderStages.Vertex)
        ));
        resourceSet = device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(resourcesLayout, mvpBuffer));
        
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
                    new ShaderDescription(ShaderStages.Vertex, File.ReadAllBytes("Shaders/Object/object.vert.spv"), "main"),
                    new ShaderDescription(ShaderStages.Fragment, File.ReadAllBytes("Shaders/Object/object.frag.spv"), "main")
                )
            ),
            Outputs = device.SwapchainFramebuffer.OutputDescription,
        });
    }
    
    public override void Tick(CommandList commandList)
    {
        commandList.SetVertexBuffer(0, vertexBuffer);
        commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt16);
        commandList.SetPipeline(pipeline);
        commandList.SetGraphicsResourceSet(0, resourceSet);
        commandList.DrawIndexed((uint)indices.Length, 1, 0, 0, 0);
    }
    
    private readonly DeviceBuffer vertexBuffer;
    private readonly DeviceBuffer indexBuffer;
    private readonly DeviceBuffer mvpBuffer;
    private readonly ResourceSet resourceSet;
    private readonly Pipeline pipeline;
    
    private readonly Vertex[] vertices = new Vertex[]
    {
        new Vertex { position = new Vector3(-0.5f, 0.5f, 0), color = Color.Blue, uv = new Vector2(0f, 1f) },
        new Vertex { position = new Vector3(0.5f, 0.5f, 0), color = Color.Yellow, uv = new Vector2(1f, 1f) },
        new Vertex { position = new Vector3(-0.5f, -0.5f, 0), color = Color.Red, uv = new Vector2(0f, 0f) },
        new Vertex { position = new Vector3(0.5f, -0.5f, 0), color = Color.Green, uv = new Vector2(1f, 0f) },
    };
    private readonly ushort[] indices = new ushort[]
    {
        0, 1, 2, 2, 1, 3,
    };
    private readonly MVPUniform mvpUniform = new()
    {
        model = Helper.Model(new Vector3(0f, 0f, 0f)),
        view = Helper.View(new Vector3(0f, 0f, -2.5f)),
        projection = Matrix4x4.Identity,
    };
}