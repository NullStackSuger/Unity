using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharpVk;
using SharpVk.Glfw;
using SharpVk.Khronos;
using SharpVk.Multivendor;
using Buffer = SharpVk.Buffer;
using Semaphore = SharpVk.Semaphore;
using Version = SharpVk.Version;

namespace UnityEngine;

public unsafe class RenderSystem
{
    public RenderSystem(Window window, uint maxFlightCount = 2)
    {
        pushConstant.model = Helper.Model(new Vector3(0, 0, 0));
        uniform.view = Helper.View(new Vector3(0, 0, -2.5f));
        uniform.projection = Helper.Perspective(Helper.ToRadians(50.0f), (float)window.Width / (float)window.Height, 0.1f, 100.0f);
        
        this.maxFlightCount = maxFlightCount;
        
        this.window = window;
        instance = CreateInstance();
        surface = CreateSurface(window, instance);
        phyDevice = CreatePhysicalDevice(instance, surface);
        QueueFamilyIndices queueFamilies = FindQueueFamilies(phyDevice, surface);
        device = CreateDevice(queueFamilies, phyDevice);
        graphicsQueue = device.GetQueue(queueFamilies.graphicsQueue!.Value, 0);
        presentQueue = device.GetQueue(queueFamilies.presentQueue!.Value, 0);
        commandPool = device.CreateCommandPool(queueFamilies.graphicsQueue!.Value, CommandPoolCreateFlags.ResetCommandBuffer);
        commandBuffers = device.AllocateCommandBuffers(commandPool, CommandBufferLevel.Primary, maxFlightCount);
        (swapChain, colorImageViews, colorFormat, depthImageViews, depthFormat) = CreateSwapChain(window.Width, window.Height, maxFlightCount, queueFamilies, phyDevice, device, surface, commandPool, graphicsQueue);
        ShaderModule vertShader = CreateShader(@".\Shaders\shader.vert.spv", device);
        ShaderModule fragShader = CreateShader(@".\Shaders\shader.frag.spv", device);
        (renderPass, setLayout, pipelineLayout, pipeline) = CreatePipeline(window.Width, window.Height, vertShader, fragShader, colorFormat, depthFormat, device);
        frameBuffers = CreateFrameBuffers(window.Width, window.Height, colorImageViews, depthImageViews, renderPass, device);
        (imageAvailableSemaphores, renderFinishedSemaphores, fences) = CreateSync(maxFlightCount, device); 
        
        vertexBuffer = CreateBuffer(input, BufferUsageFlags.VertexBuffer, phyDevice, device, commandPool, graphicsQueue);
        indexBuffer = CreateBuffer(indices, BufferUsageFlags.IndexBuffer, phyDevice, device, commandPool, graphicsQueue);
        uniformBuffer = CreateBuffer(uniform, BufferUsageFlags.UniformBuffer, phyDevice, device, commandPool, graphicsQueue);

        // TODO Uniform
        (descriptorPool, descriptorSet) = CreateDescriptor(1, new[]
        {
            (uniformBuffer, Unsafe.SizeOf<Uniform>(), 0),
        }, device, setLayout);
        
        // 更新顶点输入示例
        input[0].color = Color.White;
        vertexBuffer = CreateBuffer(input, vertexBuffer, phyDevice, device, commandPool, graphicsQueue);
    }

    private static Instance CreateInstance()
    {
        Instance instance = Instance.Create(
            new[] { "VK_LAYER_KHRONOS_validation" },
            Glfw3.GetRequiredInstanceExtensions().Append(ExtExtensions.DebugReport).ToArray(),
            applicationInfo: new ApplicationInfo
            {
                ApplicationVersion = new Version(1, 0, 0),
                EngineVersion = new Version(1, 0, 0),
                ApiVersion = new Version(1, 0, 0)
            });

        if (Define.IsDebug)
        {
            instance.CreateDebugReportCallback((flags, _, _, _, _, _, message, _) =>
            {
                switch (flags)
                {
                    case DebugReportFlags.Debug:
                        Debug.Log(message);
                        break;
                    case DebugReportFlags.Warning:
                        Debug.Warning(message);
                        break;
                    case DebugReportFlags.Error:
                        Debug.Error(message);
                        break;
                    default:
                        Debug.Warning(message);
                        break;
                }
                return false;
            }, DebugReportFlags.Error | DebugReportFlags.Warning);
        }
        
        return instance;
    }
    private static Surface CreateSurface(Window window, Instance instance)
    {
        return instance.CreateGlfw3Surface(window);
    }
    private static PhysicalDevice CreatePhysicalDevice(Instance instance, Surface surface)
    {
        PhysicalDevice[] availableDevices = instance.EnumeratePhysicalDevices();
        return availableDevices.First(phyDevice => IsSuitableDevice(phyDevice, surface));
        
        static bool IsSuitableDevice(PhysicalDevice phyDevice, Surface surface)
        {
            return phyDevice.EnumerateDeviceExtensionProperties(null).Any(extension => extension.ExtensionName == "VK_KHR_swapchain")
                   && FindQueueFamilies(phyDevice, surface);
        }
    }
    private static QueueFamilyIndices FindQueueFamilies(PhysicalDevice phyDevice, Surface surface)
    {
        QueueFamilyIndices indices = new QueueFamilyIndices();
        var queueFamilies = phyDevice.GetQueueFamilyProperties();
        for (uint i = 0; i < queueFamilies.Length; ++i)
        {
            if (queueFamilies[i].QueueFlags.HasFlag(QueueFlags.Graphics))
            {
                indices.graphicsQueue = i;
            }

            if (phyDevice.GetSurfaceSupport(i, surface))
            {
                indices.presentQueue = i;
            }

            if (indices) break;
        }

        if (!indices)
        {
            Debug.Error($"QueueFamilyIndices does not exist (Graphics:{indices.graphicsQueue.HasValue}, Present:{indices.presentQueue.HasValue} Transfer:{indices.presentQueue.HasValue})");
        }
        return indices;
    }
    private static Device CreateDevice(QueueFamilyIndices queueFamilies, PhysicalDevice phyDevice)
    {
        return phyDevice.CreateDevice(queueFamilies.Indices.Select(index => new DeviceQueueCreateInfo
        {
            QueueFamilyIndex = index,
            QueuePriorities = new[] { 1f }
        }).ToArray(), null, KhrExtensions.Swapchain);
    }
    private static (Swapchain, ImageView[], Format, ImageView[], Format) CreateSwapChain(int width, int height, uint size, QueueFamilyIndices queueFamilies, PhysicalDevice phyDevice, Device device, Surface surface, CommandPool transientCommandPool, Queue transferQueue, Swapchain oldSwapChain = null)
    {
        SwapChainSupportDetails swapChainSupport = new SwapChainSupportDetails
        {
            Capabilities = phyDevice.GetSurfaceCapabilities(surface),
            Formats = phyDevice.GetSurfaceFormats(surface),
            PresentModes = phyDevice.GetSurfacePresentModes(surface)
        };
        uint imageCount = Math.Clamp(size, swapChainSupport.Capabilities.MinImageCount, swapChainSupport.Capabilities.MaxImageCount);
        SurfaceFormat surfaceFormat = ChooseSwapSurfaceFormat(swapChainSupport.Formats);
        var indices = queueFamilies.Indices.ToArray();
        Extent2D extent = ChooseSwapExtent(width, height, swapChainSupport.Capabilities);
        Format depthFormat = ChooseDepthFormat(phyDevice);
        
        Swapchain swapChain = device.CreateSwapchain(
            surface, imageCount, surfaceFormat.Format, surfaceFormat.ColorSpace, extent, 1, 
            ImageUsageFlags.ColorAttachment, indices.Length == 1 ? SharingMode.Exclusive : SharingMode.Concurrent, 
            indices, swapChainSupport.Capabilities.CurrentTransform, CompositeAlphaFlags.Opaque, 
            ChooseSwapPresentMode(swapChainSupport.PresentModes), true, oldSwapChain);
        
        // Color
        Image[] colorImages = swapChain.GetImages();
        ImageView[] colorImageViews = colorImages.Select(image => device.CreateImageView(image, ImageViewType.ImageView2d, surfaceFormat.Format, ComponentMapping.Identity, new ImageSubresourceRange(ImageAspectFlags.Color, 0, 1, 0, 1))).ToArray();

        Image[] depthImages = new Image[imageCount];
        ImageView[] depthImageViews = new ImageView[imageCount];
        DeviceMemory[] depthMemories = new DeviceMemory[imageCount];
        for (int i = 0; i < imageCount; i++)
        {
            // Image
            depthImages[i] = device.CreateImage(
                ImageType.Image2d, depthFormat, new Extent3D(extent.Width, extent.Height, 1), 
                1, 1, SampleCountFlags.SampleCount1, ImageTiling.Optimal, 
                ImageUsageFlags.DepthStencilAttachment, SharingMode.Exclusive, indices, ImageLayout.Undefined);
            
            // Memory
            var memReq = depthImages[i].GetMemoryRequirements();
            depthMemories[i] = device.AllocateMemory(memReq.Size, FindMemoryType(memReq.MemoryTypeBits, MemoryPropertyFlags.DeviceLocal, phyDevice));
            depthImages[i].BindMemory(depthMemories[i], 0);
            
            // ImageView
            depthImageViews[i] = device.CreateImageView(
                depthImages[i], ImageViewType.ImageView2d, depthFormat, new ComponentMapping(), 
                new ImageSubresourceRange(ImageAspectFlags.Depth, 0, 1, 0, 1));
            
            // 转换图像布局
            // 前面创建Image时的ImageLayout只有在"跨队列共享模式(用不到)"才有用, 所以我们需要自己去设置布局(ImageLayout)
            var transferBuffers = device.AllocateCommandBuffers(transientCommandPool, CommandBufferLevel.Primary, 1);
            transferBuffers[0].Begin(CommandBufferUsageFlags.OneTimeSubmit);

            ImageMemoryBarrier barrier = new()
            {
                Image = depthImages[i],
                OldLayout = ImageLayout.Undefined,
                NewLayout = ImageLayout.DepthStencilAttachmentOptimal,
                SourceQueueFamilyIndex = Constants.QueueFamilyIgnored,
                DestinationQueueFamilyIndex = Constants.QueueFamilyIgnored,
                SourceAccessMask = AccessFlags.None,
                DestinationAccessMask = AccessFlags.DepthStencilAttachmentWrite,
                SubresourceRange = new ImageSubresourceRange()
                {
                    LayerCount = 1,
                    BaseArrayLayer = 0,
                    LevelCount = 1,
                    BaseMipLevel = 0,
                    AspectMask = ImageAspectFlags.Depth,
                }
            };
            PipelineStageFlags srcStage = PipelineStageFlags.TopOfPipe;
            PipelineStageFlags dstStage = PipelineStageFlags.EarlyFragmentTests;
            transferBuffers[0].PipelineBarrier(srcStage, dstStage, null, null, barrier);

            transferBuffers[0].End();
            transferQueue.Submit(new[] { new SubmitInfo { CommandBuffers = transferBuffers } }, null);
            transferQueue.WaitIdle();
            transientCommandPool.FreeCommandBuffers(transferBuffers);
        }
        
        return (swapChain, colorImageViews, surfaceFormat.Format, depthImageViews, depthFormat);
        
        static SurfaceFormat ChooseSwapSurfaceFormat(SurfaceFormat[] availableFormats)
        {
            if (availableFormats.Length == 1 && availableFormats[0].Format == Format.Undefined)
            {
                return new SurfaceFormat
                {
                    Format = Format.B8G8R8A8UNorm,
                    ColorSpace = ColorSpace.SrgbNonlinear
                };
            }

            foreach (var format in availableFormats)
            {
                if (format.Format == Format.B8G8R8A8Srgb && format.ColorSpace == ColorSpace.SrgbNonlinear)
                {
                    return format;
                }
            }

            return availableFormats[0];
        }
        static Extent2D ChooseSwapExtent(int width, int height, SurfaceCapabilities capabilities)
        {
            if (capabilities.CurrentExtent.Width != uint.MaxValue)
            {
                return capabilities.CurrentExtent;
            }
            else
            {
                return new Extent2D
                {
                    Width = Math.Max(capabilities.MinImageExtent.Width, Math.Min(capabilities.MaxImageExtent.Width, (uint)width)),
                    Height = Math.Max(capabilities.MinImageExtent.Height, Math.Min(capabilities.MaxImageExtent.Height, (uint)height))
                };
            }
        }
        static PresentMode ChooseSwapPresentMode(PresentMode[] availablePresentModes)
        {
            return availablePresentModes.Contains(PresentMode.Mailbox)
                ? PresentMode.Mailbox
                : PresentMode.Fifo;
        }
        static Format ChooseDepthFormat(PhysicalDevice phyDevice)
        {
            Format format = Format.Undefined;
            Format[] depthFormats = new[]
            {
                Format.D32SFloat,
                Format.D32SFloatS8UInt,
                Format.D24UNormS8UInt,
            };
            foreach (var depthFormat in depthFormats)
            {
                var props = phyDevice.GetFormatProperties(depthFormat);
                if (props.OptimalTilingFeatures.HasFlag(FormatFeatureFlags.DepthStencilAttachment))
                {
                    format = depthFormat;
                    break;
                }
            }
            return format;
        }
        static uint FindMemoryType(uint typeFilter, MemoryPropertyFlags flags, PhysicalDevice phyDevice)
        {
            var memoryProperties = phyDevice.GetMemoryProperties();

            for (int i = 0; i < memoryProperties.MemoryTypes.Length; i++)
            {
                if ((typeFilter & (1u << i)) > 0
                    && memoryProperties.MemoryTypes[i].PropertyFlags.HasFlag(flags))
                {
                    return (uint)i;
                }
            }

            throw new Exception("No compatible memory type.");
        }
    }
    private static ShaderModule CreateShader(string path, Device device)
    {
        var fileBytes = File.ReadAllBytes(path);
        var shaderData = new uint[(int)Math.Ceiling(fileBytes.Length / 4f)];

        System.Buffer.BlockCopy(fileBytes, 0, shaderData, 0, fileBytes.Length);
        
        return device.CreateShaderModule(fileBytes.Length, shaderData);
    }
    private static (RenderPass, DescriptorSetLayout, PipelineLayout, Pipeline) CreatePipeline(int width, int height, ShaderModule vertShader, ShaderModule fragShader, Format colorFormat, Format depthFormat, Device device)
    {
        #region RenderPass
        var renderPass = device.CreateRenderPass(
            new AttachmentDescription[]
            {
                // Color
                new AttachmentDescription()
                {
                    Format = colorFormat,
                    Samples = SampleCountFlags.SampleCount1,
                    LoadOp = AttachmentLoadOp.Clear,
                    StoreOp = AttachmentStoreOp.Store,
                    StencilLoadOp = AttachmentLoadOp.DontCare,
                    StencilStoreOp = AttachmentStoreOp.DontCare,
                    InitialLayout = ImageLayout.Undefined,
                    FinalLayout = ImageLayout.PresentSource
                },
                // Depth
                new AttachmentDescription()
                {
                    Format = depthFormat,
                    Samples = SampleCountFlags.SampleCount1,
                    LoadOp = AttachmentLoadOp.Clear,
                    StoreOp = AttachmentStoreOp.DontCare,
                    StencilLoadOp = AttachmentLoadOp.DontCare,
                    StencilStoreOp = AttachmentStoreOp.DontCare,
                    InitialLayout = ImageLayout.Undefined,
                    FinalLayout = ImageLayout.DepthStencilAttachmentOptimal,
                },
            },
            new SubpassDescription()
            {
                PipelineBindPoint = PipelineBindPoint.Graphics,
                ColorAttachments = new[]
                {
                    new AttachmentReference
                    {
                        Attachment = 0,
                        Layout = ImageLayout.ColorAttachmentOptimal
                    }
                },
                DepthStencilAttachment = new AttachmentReference
                {
                    Attachment = 1,
                    Layout = ImageLayout.DepthStencilAttachmentOptimal,
                },
            },
            new[]
            {
                new SubpassDependency()
                {
                    SourceSubpass = Constants.SubpassExternal,
                    DestinationSubpass = 0,
                    SourceAccessMask = AccessFlags.MemoryRead,
                    DestinationAccessMask = AccessFlags.ColorAttachmentWrite | AccessFlags.DepthStencilAttachmentWrite,
                    SourceStageMask = PipelineStageFlags.BottomOfPipe,
                    DestinationStageMask = PipelineStageFlags.ColorAttachmentOutput | PipelineStageFlags.EarlyFragmentTests,
                },
                new SubpassDependency()
                {
                    SourceSubpass = 0,
                    DestinationSubpass = Constants.SubpassExternal,
                    SourceAccessMask = AccessFlags.ColorAttachmentWrite | AccessFlags.DepthStencilAttachmentWrite,
                    DestinationAccessMask = AccessFlags.MemoryRead,
                    SourceStageMask = PipelineStageFlags.ColorAttachmentOutput | PipelineStageFlags.EarlyFragmentTests,
                    DestinationStageMask = PipelineStageFlags.BottomOfPipe,
                }
            });
        #endregion

        #region PipelineLayout
        // Uniform
        var bindings = new DescriptorSetLayoutBinding[]
        {
            Uniform.GetBinding(), // TODO Uniform
        };
        // PushConstant
        var ranges = new PushConstantRange[]
        {
            PushConstant.GetRange(), // 和Uniform不同, 一个Shader可能只有一个PushConstant, 类似VertexInput, 这样写没问题
        };

        var setLayout = device.CreateDescriptorSetLayout(bindings);
        var pipelineLayout = device.CreatePipelineLayout(setLayout, ranges);
        #endregion
        
        #region Pipeline
        var pipeline = device.CreateGraphicsPipeline(null,
            new[]
            {
                new PipelineShaderStageCreateInfo
                {
                    Stage = ShaderStageFlags.Vertex,
                    Module = vertShader,
                    Name = "main"
                },
                new PipelineShaderStageCreateInfo
                {
                    Stage = ShaderStageFlags.Fragment,
                    Module = fragShader,
                    Name = "main"
                }
            },
            new PipelineRasterizationStateCreateInfo
            {
                RasterizerDiscardEnable = false,
                PolygonMode = PolygonMode.Fill,
                LineWidth = 1,
                CullMode = CullModeFlags.Back,
                FrontFace = FrontFace.Clockwise,
                DepthBiasEnable = false,
            },
            pipelineLayout,
            renderPass,
            0,
            null,
            -1,
            vertexInputState: new PipelineVertexInputStateCreateInfo()
            {
                VertexBindingDescriptions = VertexInput.GetBinding(),
                VertexAttributeDescriptions = VertexInput.GetAttributes(),
            },
            inputAssemblyState: new PipelineInputAssemblyStateCreateInfo
            {
                PrimitiveRestartEnable = false,
                Topology = PrimitiveTopology.TriangleList
            },
            viewportState: new PipelineViewportStateCreateInfo
            {
                Viewports = new[]
                {
                    new Viewport(0f, 0f, width, height, 0, 1)
                },
                Scissors = new[]
                {
                    new Rect2D(new Extent2D((uint)width, (uint)height))
                }
            },
            colorBlendState: new PipelineColorBlendStateCreateInfo
            {
                Attachments = new[]
                {
                    new PipelineColorBlendAttachmentState
                    {
                        BlendEnable = false,
                        ColorWriteMask = ColorComponentFlags.R
                                         | ColorComponentFlags.G
                                         | ColorComponentFlags.B
                                         | ColorComponentFlags.A,
                        
                        SourceColorBlendFactor = BlendFactor.One,
                        DestinationColorBlendFactor = BlendFactor.OneMinusSourceAlpha,
                        ColorBlendOp = BlendOp.Add,
                        
                        SourceAlphaBlendFactor = BlendFactor.One,
                        DestinationAlphaBlendFactor = BlendFactor.Zero,
                        AlphaBlendOp = BlendOp.Add
                    }
                },
                LogicOpEnable = false,
            },
            multisampleState: new PipelineMultisampleStateCreateInfo
            {
                SampleShadingEnable = false,
                RasterizationSamples = SampleCountFlags.SampleCount1,
            },
            depthStencilState: new PipelineDepthStencilStateCreateInfo()
            {
                DepthTestEnable = true,
                DepthWriteEnable = true,
                DepthCompareOp = CompareOp.Less,
                DepthBoundsTestEnable = false,
                StencilTestEnable = false,
            });
        #endregion

        return (renderPass, setLayout, pipelineLayout, pipeline);
    }
    private static Framebuffer[] CreateFrameBuffers(int width, int height, ImageView[] colorImageViews, ImageView[] depthImageView, RenderPass renderPass, Device device)
    {
        Framebuffer[] frameBuffers = new Framebuffer[colorImageViews.Length];
        for (int i = 0; i < frameBuffers.Length; i++)
        {
            frameBuffers[i] = device.CreateFramebuffer(renderPass, new ImageView[] { colorImageViews[i], depthImageView[i] }, (uint)width, (uint)height, 1);
        }
        return frameBuffers;
    }
    private static (Semaphore[], Semaphore[], Fence[]) CreateSync(uint maxFlightCount, Device device)
    {
        var imageAvailableSemaphores = new Semaphore[maxFlightCount];
        var renderFinishedSemaphores = new Semaphore[maxFlightCount];
        var fences = new Fence[maxFlightCount];
        for (int i = 0; i < maxFlightCount; i++)
        {
            imageAvailableSemaphores[i] = device.CreateSemaphore();
            renderFinishedSemaphores[i] = device.CreateSemaphore();
            fences[i] = device.CreateFence(FenceCreateFlags.Signaled);
        }
        return (imageAvailableSemaphores, renderFinishedSemaphores, fences);
    }
    private static Buffer CreateBuffer<T>(T[] array, BufferUsageFlags flag, PhysicalDevice phyDevice, Device device, CommandPool commandPool, Queue graphicsQueue)
    {
        int size = Unsafe.SizeOf<T>();
        ulong bufferSize = (ulong)(size * array.Length);
        
        var (buffer, _) = CreateBufferInner(bufferSize, BufferUsageFlags.TransferDestination | flag, MemoryPropertyFlags.DeviceLocal, phyDevice, device);
        buffer = CreateBuffer(array, buffer, phyDevice, device, commandPool, graphicsQueue);

        return buffer;
        
        static (Buffer, DeviceMemory) CreateBufferInner(ulong bufferSize, BufferUsageFlags usage, MemoryPropertyFlags properties, PhysicalDevice phyDevice, Device device)
        {
            Buffer buffer = device.CreateBuffer(bufferSize, usage, SharingMode.Exclusive, null);
            var memRequirements = buffer.GetMemoryRequirements();
            DeviceMemory bufferMemory = device.AllocateMemory(memRequirements.Size, FindMemoryType(memRequirements.MemoryTypeBits, properties, phyDevice));
            buffer.BindMemory(bufferMemory, 0);
            return (buffer, bufferMemory);
        }
        
        static uint FindMemoryType(uint typeFilter, MemoryPropertyFlags flags, PhysicalDevice phyDevice)
        {
            var memoryProperties = phyDevice.GetMemoryProperties();

            for (int i = 0; i < memoryProperties.MemoryTypes.Length; i++)
            {
                if ((typeFilter & (1u << i)) > 0
                    && memoryProperties.MemoryTypes[i].PropertyFlags.HasFlag(flags))
                {
                    return (uint)i;
                }
            }

            throw new Exception("No compatible memory type.");
        }
    }
    private static Buffer CreateBuffer<T>(T[] src, Buffer dst, PhysicalDevice phyDevice, Device device, CommandPool commandPool, Queue graphicsQueue)
    {
        int size = Unsafe.SizeOf<T>();
        ulong bufferSize = (ulong)(size * src.Length);
        
        var (stagingBuffer, stagingBufferMemory) = CreateBufferInner(bufferSize, BufferUsageFlags.TransferSource, MemoryPropertyFlags.HostVisible | MemoryPropertyFlags.HostCoherent, phyDevice, device);
        
        IntPtr memoryBuffer = stagingBufferMemory.Map(0, bufferSize, MemoryMapFlags.None);
        for (int index = 0; index < src.Length; index++)
        {
            Marshal.StructureToPtr(src[index], memoryBuffer + (size * index), false);
        }
        stagingBufferMemory.Unmap();
        
        CopyBuffer(bufferSize, stagingBuffer, dst, device, commandPool, graphicsQueue);
        
        stagingBuffer.Dispose();
        stagingBufferMemory.Free();

        return dst;
        
        static (Buffer, DeviceMemory) CreateBufferInner(ulong bufferSize, BufferUsageFlags usage, MemoryPropertyFlags properties, PhysicalDevice phyDevice, Device device)
        {
            Buffer buffer = device.CreateBuffer(bufferSize, usage, SharingMode.Exclusive, null);
            var memRequirements = buffer.GetMemoryRequirements();
            DeviceMemory bufferMemory = device.AllocateMemory(memRequirements.Size, FindMemoryType(memRequirements.MemoryTypeBits, properties, phyDevice));
            buffer.BindMemory(bufferMemory, 0);
            return (buffer, bufferMemory);
        }
        static uint FindMemoryType(uint typeFilter, MemoryPropertyFlags flags, PhysicalDevice phyDevice)
        {
            var memoryProperties = phyDevice.GetMemoryProperties();

            for (int i = 0; i < memoryProperties.MemoryTypes.Length; i++)
            {
                if ((typeFilter & (1u << i)) > 0
                    && memoryProperties.MemoryTypes[i].PropertyFlags.HasFlag(flags))
                {
                    return (uint)i;
                }
            }

            throw new Exception("No compatible memory type.");
        }
        static void CopyBuffer(ulong bufferSize, Buffer src, Buffer dst, Device device, CommandPool transientCommandPool, Queue transferQueue)
        {
            var transferBuffers = device.AllocateCommandBuffers(transientCommandPool, CommandBufferLevel.Primary, 1);

            transferBuffers[0].Begin(CommandBufferUsageFlags.OneTimeSubmit);

            transferBuffers[0].CopyBuffer(src, dst, new[] { new BufferCopy { Size = bufferSize } });

            transferBuffers[0].End();

            transferQueue.Submit(new[] { new SubmitInfo { CommandBuffers = transferBuffers } }, null);
            transferQueue.WaitIdle();

            transientCommandPool.FreeCommandBuffers(transferBuffers);
        }
    }
    private static Buffer CreateBuffer<T>(T data, BufferUsageFlags flag, PhysicalDevice phyDevice, Device device, CommandPool commandPool, Queue graphicsQueue)
    {
        ulong bufferSize = (ulong)Unsafe.SizeOf<T>();

        var (buffer, _) = CreateBufferInner(bufferSize, BufferUsageFlags.TransferDestination | flag, MemoryPropertyFlags.DeviceLocal, phyDevice, device);
        buffer = CreateBuffer(data, buffer, phyDevice, device, commandPool, graphicsQueue);

        return buffer;
        
        static (Buffer, DeviceMemory) CreateBufferInner(ulong bufferSize, BufferUsageFlags usage, MemoryPropertyFlags properties, PhysicalDevice phyDevice, Device device)
        {
            Buffer buffer = device.CreateBuffer(bufferSize, usage, SharingMode.Exclusive, null);
            var memRequirements = buffer.GetMemoryRequirements();
            DeviceMemory bufferMemory = device.AllocateMemory(memRequirements.Size, FindMemoryType(memRequirements.MemoryTypeBits, properties, phyDevice));
            buffer.BindMemory(bufferMemory, 0);
            return (buffer, bufferMemory);
        }
        static uint FindMemoryType(uint typeFilter, MemoryPropertyFlags flags, PhysicalDevice phyDevice)
        {
            var memoryProperties = phyDevice.GetMemoryProperties();

            for (int i = 0; i < memoryProperties.MemoryTypes.Length; i++)
            {
                if ((typeFilter & (1u << i)) > 0
                    && memoryProperties.MemoryTypes[i].PropertyFlags.HasFlag(flags))
                {
                    return (uint)i;
                }
            }

            throw new Exception("No compatible memory type.");
        }
    }
    private static Buffer CreateBuffer<T>(T src, Buffer dst, PhysicalDevice phyDevice, Device device, CommandPool commandPool, Queue graphicsQueue)
    {
        ulong bufferSize = (ulong)Unsafe.SizeOf<T>();
        
        var (stagingBuffer, stagingBufferMemory) = CreateBufferInner(bufferSize, BufferUsageFlags.TransferSource, MemoryPropertyFlags.HostVisible | MemoryPropertyFlags.HostCoherent, phyDevice, device);
        
        IntPtr memoryBuffer = stagingBufferMemory.Map(0, bufferSize, MemoryMapFlags.None);
        Marshal.StructureToPtr(src, memoryBuffer, false);
        stagingBufferMemory.Unmap();
        
        CopyBuffer(bufferSize, stagingBuffer, dst, device, commandPool, graphicsQueue);
        
        stagingBuffer.Dispose();
        stagingBufferMemory.Free();

        return dst;
        
        static (Buffer, DeviceMemory) CreateBufferInner(ulong bufferSize, BufferUsageFlags usage, MemoryPropertyFlags properties, PhysicalDevice phyDevice, Device device)
        {
            Buffer buffer = device.CreateBuffer(bufferSize, usage, SharingMode.Exclusive, null);
            var memRequirements = buffer.GetMemoryRequirements();
            DeviceMemory bufferMemory = device.AllocateMemory(memRequirements.Size, FindMemoryType(memRequirements.MemoryTypeBits, properties, phyDevice));
            buffer.BindMemory(bufferMemory, 0);
            return (buffer, bufferMemory);
        }
        static uint FindMemoryType(uint typeFilter, MemoryPropertyFlags flags, PhysicalDevice phyDevice)
        {
            var memoryProperties = phyDevice.GetMemoryProperties();

            for (int i = 0; i < memoryProperties.MemoryTypes.Length; i++)
            {
                if ((typeFilter & (1u << i)) > 0
                    && memoryProperties.MemoryTypes[i].PropertyFlags.HasFlag(flags))
                {
                    return (uint)i;
                }
            }

            throw new Exception("No compatible memory type.");
        }
        static void CopyBuffer(ulong bufferSize, Buffer src, Buffer dst, Device device, CommandPool transientCommandPool, Queue transferQueue)
        {
            var transferBuffers = device.AllocateCommandBuffers(transientCommandPool, CommandBufferLevel.Primary, 1);

            transferBuffers[0].Begin(CommandBufferUsageFlags.OneTimeSubmit);

            transferBuffers[0].CopyBuffer(src, dst, new[] { new BufferCopy { Size = bufferSize } });

            transferBuffers[0].End();

            transferQueue.Submit(new[] { new SubmitInfo { CommandBuffers = transferBuffers } }, null);
            transferQueue.WaitIdle();

            transientCommandPool.FreeCommandBuffers(transferBuffers);
        }
    }
    private static (DescriptorPool, DescriptorSet) CreateDescriptor(uint uniformCount, (Buffer, int, int)[]  array, Device device, DescriptorSetLayout setLayout)
    {
        // Pool
        DescriptorPoolSize[] descriptorPoolSizes = new[]
        {
            new DescriptorPoolSize()
            {
                DescriptorCount = uniformCount,
                Type = DescriptorType.UniformBuffer,
            }
        };
        uint count = 0;
        foreach (var descriptorPoolSize in descriptorPoolSizes)
        {
            count += descriptorPoolSize.DescriptorCount;
        }
        var pool = device.CreateDescriptorPool(count, descriptorPoolSizes);

        // Sets
        DescriptorSet set = device.AllocateDescriptorSets(pool, setLayout)[0];

        // Update
        for (uint i = 0; i < array.Length; i++)
        {
            device.UpdateDescriptorSets(new WriteDescriptorSet()
            {
                DescriptorType = DescriptorType.UniformBuffer,
                DestinationBinding = (uint)array[i].Item3,
                DestinationSet = set,
                DestinationArrayElement = 0,
                DescriptorCount = 1,
                BufferInfo = new[]
                {
                    new DescriptorBufferInfo
                    {
                        Buffer = array[i].Item1,
                        Offset = 0,
                        Range = (uint)array[i].Item2,
                    }
                }
            }, null);
        }
        
        return (pool, set);
    }
    
    public void Tick()
    {
        device.WaitForFences(fences[curFrame], true, ulong.MaxValue);
        device.ResetFences(fences[curFrame]);
        
        uint nextImage = this.swapChain.AcquireNextImage(uint.MaxValue, this.imageAvailableSemaphores[curFrame], null);
        
        var commandBuffer = commandBuffers[curFrame];
        commandBuffer.Reset();
        
        commandBuffer.Begin(CommandBufferUsageFlags.OneTimeSubmit);
        commandBuffer.BeginRenderPass(this.renderPass,
            this.frameBuffers[nextImage],
            new Rect2D(new Extent2D((uint)window.Width, (uint)window.Height)),
            new ClearValue[]
            {
                new ClearColorValue(0.1f, 0.1f, 0.1f, 0.1f),
                new ClearDepthStencilValue(1.0f, 0)
            },
            SubpassContents.Inline);
        
        commandBuffer.BindPipeline(PipelineBindPoint.Graphics, this.pipeline);
        commandBuffer.BindVertexBuffers(0, new[] { vertexBuffer }, new ulong[] { 0 });
        commandBuffer.BindIndexBuffer(indexBuffer, 0, IndexType.Uint16);
        commandBuffer.BindDescriptorSets(PipelineBindPoint.Graphics, pipelineLayout, 0, descriptorSet, null);
        commandBuffer.PushConstants(pipelineLayout, ShaderStageFlags.Vertex | ShaderStageFlags.Fragment, 0, Helper.VkToBytes(pushConstant));
        commandBuffer.DrawIndexed((uint)indices.Length, 1,  0,0, 0);

        commandBuffer.EndRenderPass();
        commandBuffer.End();

        this.graphicsQueue.Submit(
            new SubmitInfo
            {
                CommandBuffers = new[] { commandBuffer },
                SignalSemaphores = new[] { this.renderFinishedSemaphores[curFrame] },
                WaitDestinationStageMask = new [] { PipelineStageFlags.ColorAttachmentOutput },
                WaitSemaphores = new [] { this.imageAvailableSemaphores[curFrame] }
            }, fences[curFrame]);
        this.presentQueue.Present(this.renderFinishedSemaphores[curFrame], this.swapChain, nextImage, new Result[1]);

        curFrame = (curFrame + 1) % maxFlightCount;
    }

    private readonly Window window;
    private readonly Instance instance;
    private readonly Surface surface;
    private readonly PhysicalDevice phyDevice;
    private readonly Device device;
    private readonly Queue graphicsQueue;
    private readonly Queue presentQueue;
    private readonly CommandPool commandPool;
    private readonly CommandBuffer[] commandBuffers;
    private readonly Swapchain swapChain;
    private readonly Format colorFormat;
    private readonly ImageView[] colorImageViews;
    private readonly Format depthFormat;
    private readonly ImageView[] depthImageViews;
    private readonly RenderPass renderPass;
    private readonly DescriptorSetLayout setLayout;
    private readonly PipelineLayout pipelineLayout;
    private readonly Pipeline pipeline;
    private readonly Framebuffer[] frameBuffers;
    private readonly Semaphore[] imageAvailableSemaphores;
    private readonly Semaphore[] renderFinishedSemaphores;
    private readonly Fence[] fences;
    private readonly Buffer vertexBuffer;
    private readonly Buffer indexBuffer;
    private readonly Buffer uniformBuffer;
    private readonly DescriptorPool descriptorPool;
    private readonly DescriptorSet descriptorSet;

    private uint curFrame = 0;

    private readonly uint maxFlightCount;

    private readonly VertexInput[] input = new VertexInput[]
    {
        new VertexInput()
        {
            position = new Vector3(-0.5f, -0.5f, 0),
            color = Color.Green,
            uv = new Vector2(0, 0),
        },
        new VertexInput()
        {
            position = new Vector3(0.5f, -0.5f, 0),
            color = Color.Red,
            uv = new Vector2(1, 0),
        },
        new VertexInput()
        {
            position = new Vector3(0.5f, 0.5f, 0),
            color = Color.Blue,
            uv = new Vector2(1, 1),
        },
        new VertexInput()
        {
            position  = new Vector3(-0.5f, 0.5f, 0),
            color = Color.White,
            uv = new Vector2(0, 1),
        },
    };
    private readonly ushort[] indices = new ushort[]
    {
        0, 1, 3, 1, 2, 3
    };
    private readonly Uniform uniform = new Uniform()
    {
        view = Matrix4x4.Identity,
        projection = Matrix4x4.Identity,
    };
    private readonly PushConstant pushConstant = new PushConstant()
    {
        model = Matrix4x4.Identity,
    };
    
    private struct QueueFamilyIndices
    {
        public uint? graphicsQueue;
        public uint? presentQueue;

        public static implicit operator bool(QueueFamilyIndices queueFamilyIndices)
        {
            return queueFamilyIndices.graphicsQueue.HasValue
                   && queueFamilyIndices.presentQueue.HasValue;
        }

        public IEnumerable<uint> Indices
        {
            get
            {
                if (this.graphicsQueue.HasValue)
                {
                    yield return this.graphicsQueue.Value;
                }

                if (this.presentQueue.HasValue && this.presentQueue != this.graphicsQueue)
                {
                    yield return this.presentQueue.Value;
                }
            }
        }
    }
    
    private struct SwapChainSupportDetails
    {
        public SurfaceCapabilities Capabilities;
        public SurfaceFormat[] Formats;
        public PresentMode[] PresentModes;
    }
}