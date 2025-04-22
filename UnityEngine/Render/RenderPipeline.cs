/*using Vulkan;

namespace UnityEngine;

public class RenderPipeline
{
    public RenderPipeline(int width, int height, ShaderModule vertModule, ShaderModule fragModule, Device device, SwapChain swapChain)
    {
        GraphicsPipelineCreateInfo pipelineInfo = new();

        #region 1.顶点输入
        pipelineInfo.VertexInputState = new PipelineVertexInputStateCreateInfo()
        {
            VertexBindingDescriptions = null, // TODO
            VertexAttributeDescriptions = null,
        };
        #endregion

        #region 2.顶点聚集
        pipelineInfo.InputAssemblyState = new PipelineInputAssemblyStateCreateInfo()
        {
            PrimitiveRestartEnable = false,
            Topology = PrimitiveTopology.TriangleList,
        };
        #endregion

        #region 3.Shader
        pipelineInfo.Stages = new PipelineShaderStageCreateInfo[]
        {
            new PipelineShaderStageCreateInfo()
            {
                Stage = ShaderStageFlags.Vertex,
                Module = vertModule,
                Name = "main"
            },
            new PipelineShaderStageCreateInfo()
            {
                Stage = ShaderStageFlags.Fragment,
                Module = fragModule,
                Name = "main"
            }
        };
        #endregion

        #region 4.ViewPort
        pipelineInfo.ViewportState = new PipelineViewportStateCreateInfo()
        {
            Viewports = new Viewport[] { new Viewport(){ X = 0, Y = 0, Width = width, Height = height, MinDepth = 0, MaxDepth = 1 } },
            Scissors = new Rect2D[] { new Rect2D() { Offset = new Offset2D() { X = 0, Y = 0 }, Extent =  new Extent2D() { Width = (uint)width, Height = (uint)height } } },
        };
        #endregion
        
        #region 5.光栅化
        pipelineInfo.RasterizationState = new PipelineRasterizationStateCreateInfo()
        {
            RasterizerDiscardEnable = false,
            CullMode = CullModeFlags.Back,
            FrontFace = FrontFace.CounterClockwise,
            PolygonMode = PolygonMode.Fill,
            LineWidth = 1.0f,
        };
        #endregion

        #region 6.AA
        pipelineInfo.MultisampleState = new PipelineMultisampleStateCreateInfo()
        {
            SampleShadingEnable = false,
            RasterizationSamples = SampleCountFlags.Count1,
        };
        #endregion

        #region 7.深度测试模板测试
        pipelineInfo.DepthStencilState = new PipelineDepthStencilStateCreateInfo()
        {
            DepthTestEnable = true,
            DepthWriteEnable = true,
            DepthCompareOp = CompareOp.Less,
            DepthBoundsTestEnable = false,
            StencilTestEnable = false,
        };
        #endregion

        #region 8.颜色混合
        // newRGB = 1 * srcRGB + (1 - srcA) * dstRGB
        // newA = 1 * srcA + 0 * dstA
        pipelineInfo.ColorBlendState = new PipelineColorBlendStateCreateInfo()
        {
            LogicOpEnable = false,
            Attachments = new []
            {
                new PipelineColorBlendAttachmentState()
                {
                    BlendEnable = false,
                    ColorWriteMask = ColorComponentFlags.R | ColorComponentFlags.G | ColorComponentFlags.B | ColorComponentFlags.A,
                    SrcColorBlendFactor = BlendFactor.One,
                    DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha,
                    ColorBlendOp = BlendOp.Add,
                    SrcAlphaBlendFactor = BlendFactor.One,
                    DstAlphaBlendFactor = BlendFactor.Zero,
                    AlphaBlendOp = BlendOp.Add,
                }
            }
        };
        #endregion
        
        #region 9.RenderPass
        RenderPassCreateInfo renderPassInfo = new RenderPassCreateInfo();
        
        // 1.ColorAttachment
        AttachmentDescription colorAttachment = new AttachmentDescription()
        {
            Format = swapChain.info.format.Format,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.PresentSrcKhr,
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.Store,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            Samples = SampleCountFlags.Count1,
        };
        // ColorAttachmentReference
        AttachmentReference colorReference = new AttachmentReference()
        {
            Layout = ImageLayout.ColorAttachmentOptimal,
            Attachment = 0,
        };
        
        // 2.DepthAttachment
        AttachmentDescription depthAttachment = new AttachmentDescription()
        {
            Format = swapChain.info.depthFormat,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.DepthStencilAttachmentOptimal,
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.DontCare,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            Samples = SampleCountFlags.Count1,
        };
        // DepthAttachmentReference
        AttachmentReference depthReference = new AttachmentReference()
        {
            Layout = ImageLayout.DepthStencilAttachmentOptimal,
            Attachment = 1,
        };
        
        renderPassInfo.Attachments = new[] { colorAttachment, depthAttachment };
       
        // SubPass
        renderPassInfo.Subpasses = new[]
        {
            new SubpassDescription()
            {
                PipelineBindPoint = PipelineBindPoint.Graphics,
                ColorAttachments = new[] { colorReference },
                DepthStencilAttachment = depthReference,
            }
        };
        
        // SubPass顺序 InitSubPass(内置) -> SubPass0
        renderPassInfo.Dependencies = new[]
        {
            new SubpassDependency()
            {
                SrcSubpass = 0xFFFFFFFF,
                DstSubpass = 0,
                SrcAccessMask = AccessFlags.ColorAttachmentWrite | AccessFlags.DepthStencilAttachmentWrite,
                DstAccessMask = AccessFlags.ColorAttachmentWrite | AccessFlags.DepthStencilAttachmentRead,
                SrcStageMask = PipelineStageFlags.ColorAttachmentOutput | PipelineStageFlags.EarlyFragmentTests,
                DstStageMask = PipelineStageFlags.ColorAttachmentOutput | PipelineStageFlags.EarlyFragmentTests,
            }
        };
        
        renderPass = device.CreateRenderPass(renderPassInfo);
        pipelineInfo.RenderPass = renderPass;
        #endregion

        #region 10.Layout
        // 这里可以设置多个setLayout, 对应Shader中set
        setLayouts = new[] { device.CreateDescriptorSetLayout(new DescriptorSetLayoutCreateInfo()
        {
            // 这里设置多个bindings对应Shader中binding
            Bindings = null, // TODO
        }) };
        layout = device.CreatePipelineLayout(new PipelineLayoutCreateInfo()
        {
            SetLayouts = setLayouts,
            PushConstantRanges = null, // TODO
        });
        #endregion

        pipeline = device.CreateGraphicsPipelines(null, new[] { pipelineInfo })[0];
    }

    public static explicit operator Pipeline(RenderPipeline pipeline)
    {
        return pipeline.pipeline;
    }

    public readonly Pipeline pipeline;
    public readonly PipelineLayout layout;
    public readonly RenderPass renderPass;
    public readonly DescriptorSetLayout[] setLayouts;
}*/