/*using Vulkan;

namespace UnityEngine;

public class SwapChain
{
    public struct SwapChainInfo
    {
        public Extent2D imageExtent;
        public uint imageCount;
        public SurfaceFormatKhr format;
        public SurfaceTransformFlagsKhr transform;
        public PresentModeKhr present;
        public Format depthFormat;
    };
    
    public SwapChain(int width, int height, QueueFamilyIndices indices, SurfaceKhr surface, PhysicalDevice phyDevice, Device device, CommandPool commandPool, Queue graphicsQueue)
    {
        info = QueryInfo(width, height, surface, phyDevice);

        SwapchainCreateInfoKhr createInfo = new()
        {
            Clipped = true,
            ImageArrayLayers = 1,
            ImageUsage = ImageUsageFlags.ColorAttachment,
            CompositeAlpha = CompositeAlphaFlagsKhr.Opaque,
            Surface = surface,
            ImageColorSpace = info.format.ColorSpace,
            ImageFormat = info.format.Format,
            ImageExtent = info.imageExtent,
            MinImageCount = info.imageCount,
            PresentMode = info.present,
        };

        if (indices.graphicsQueue!.Value == indices.presentQueue!.Value)
        {
            createInfo.QueueFamilyIndices = new [] { indices.graphicsQueue!.Value };
            createInfo.ImageSharingMode = SharingMode.Exclusive;
        }
        else
        {
            createInfo.QueueFamilyIndices = new [] { indices.graphicsQueue!.Value, indices.presentQueue!.Value };
            createInfo.ImageSharingMode = SharingMode.Concurrent;
        }
        
        swapChain = device.CreateSwapchainKHR(createInfo);

        CreateColorImage(info, swapChain, device, out colorImages, out colorImageViews);
        CreateDepthImage(info, phyDevice, device, commandPool, graphicsQueue, out depthImages, out depthImageViews, out depthMemories);
    }

    public void CreateFrameBuffers(int width, int height, Device device, RenderPass renderPass)
    {
        frameBuffers = new Framebuffer[colorImages.Length];
        for (int i = 0; i < colorImages.Length; i++)
        {
            var attachments = new ImageView[] { colorImageViews[i], depthImageViews[i] };
            frameBuffers[i] = device.CreateFramebuffer(new FramebufferCreateInfo()
            {
                Attachments = attachments,
                Width = (uint)width,
                Height = (uint)height,
                RenderPass = renderPass,
                Layers = 1,
            });
        }
    }

    public static explicit operator SwapchainKhr(SwapChain swapChain)
    {
        return swapChain.swapChain;
    }

    private static SwapChainInfo QueryInfo(int width, int height, SurfaceKhr surface, PhysicalDevice phyDevice)
    {
        SwapChainInfo info;

        var formats = phyDevice.GetSurfaceFormatsKHR(surface);
        info.format = formats[0];
        foreach (SurfaceFormatKhr format in formats)
        {
            if (format.Format == Format.R8G8B8A8Srgb && format.ColorSpace == ColorSpaceKhr.SrgbNonlinear)
            {
                info.format = format;
                break;
            }
        }
        
        var capabilities = phyDevice.GetSurfaceCapabilitiesKHR(surface);
        info.imageCount = Math.Clamp(2, capabilities.MinImageCount, capabilities.MaxImageCount);
        info.imageExtent.Width = (uint)Math.Clamp(width, capabilities.MinImageExtent.Width, capabilities.MaxImageExtent.Width);
        info.imageExtent.Height = (uint)Math.Clamp(height, capabilities.MinImageExtent.Height, capabilities.MaxImageExtent.Height);
        info.transform = capabilities.CurrentTransform;
        
        var presents = phyDevice.GetSurfacePresentModesKHR(surface);
        info.present = PresentModeKhr.Fifo;
        foreach (PresentModeKhr present in presents)
        {
            if (present == PresentModeKhr.Mailbox)
            {
                info.present = PresentModeKhr.Mailbox;
                break;
            }
        }

        info.depthFormat = Format.Undefined;
        var depthFormats = new Format[] { Format.D32Sfloat, Format.D32SfloatS8Uint, Format.D24UnormS8Uint };
        foreach (var depthFormat in depthFormats)
        {
            FormatProperties props = phyDevice.GetFormatProperties(depthFormat);
            if ((props.OptimalTilingFeatures & FormatFeatureFlags.DepthStencilAttachment) == FormatFeatureFlags.DepthStencilAttachment)
            {
                info.depthFormat = depthFormat;
                break;
            }
        }
        
        return info;
    }

    private static void CreateColorImage(SwapChainInfo info, SwapchainKhr swapChain, Device device, out Image[] colorImages, out ImageView[] colorImageViews)
    {
        colorImages = device.GetSwapchainImagesKHR(swapChain);
        
        colorImageViews = new ImageView[colorImages.Length];
        for (int i = 0; i < colorImages.Length; i++)
        {
            colorImageViews[i] = device.CreateImageView(new ImageViewCreateInfo()
            {
                Image = colorImages[i],
                ViewType = ImageViewType.View2D,
                Components = new ComponentMapping(),
                Format = info.format.Format,
                SubresourceRange = new ImageSubresourceRange()
                {
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                    AspectMask = ImageAspectFlags.Color,
                },
            });
        }
    }

    private static void CreateDepthImage(SwapChainInfo info, PhysicalDevice phyDevice, Device device, CommandPool commandPool, Queue graphicsQueue, 
        out Image[] depthImages, out ImageView[] depthImageViews, out DeviceMemory[] depthMemories)
    {
        depthImages = new Image[info.imageCount];
        depthImageViews = new ImageView[info.imageCount];
        depthMemories = new DeviceMemory[info.imageCount];

        for (int i = 0; i < info.imageCount; i++)
        {
            // Image
            Image depthImage = device.CreateImage(new ImageCreateInfo()
            {
                ImageType = ImageType.Image2D,
                Extent = new Extent3D()
                {
                    Width = info.imageExtent.Width,
                    Height = info.imageExtent.Height,
                    Depth = 1,
                },
                MipLevels = 1,
                ArrayLayers = 1,
                Format = info.depthFormat,
                Tiling = ImageTiling.Optimal,
                InitialLayout = ImageLayout.Undefined,
                Usage = ImageUsageFlags.DepthStencilAttachment,
                Samples = SampleCountFlags.Count1,
                SharingMode = SharingMode.Exclusive,
            });
            depthImages[i] = depthImage;
            
            // Memory
            MemoryRequirements memReq = device.GetImageMemoryRequirements(depthImages[i]);
            uint memoryIndex = Tools.QueryMemoryIndex(MemoryPropertyFlags.DeviceLocal, memReq, phyDevice);
            depthMemories[i] = device.AllocateMemory(new MemoryAllocateInfo()
            {
                AllocationSize = memReq.Size,
                MemoryTypeIndex = memoryIndex,
            });
            device.BindImageMemory(depthImages[i], depthMemories[i], 0);
            
            // ImageView
            depthImageViews[i] = device.CreateImageView(new ImageViewCreateInfo()
            {
                Image = depthImages[i],
                ViewType = ImageViewType.View2D,
                Format = info.depthFormat,
                SubresourceRange = new ImageSubresourceRange()
                {
                    AspectMask = ImageAspectFlags.Depth,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                }
            });
            
            // 转换图像布局
            commandPool.Execute(graphicsQueue, device, (CommandBuffer commandBuffer) =>
            {
                PipelineStageFlags srcStage = PipelineStageFlags.TopOfPipe;
                PipelineStageFlags dstStage = PipelineStageFlags.EarlyFragmentTests;
                
                commandBuffer.CmdPipelineBarrier(srcStage, dstStage, new DependencyFlags(), null, null, new ImageMemoryBarrier()
                {
                    Image = depthImage,
                    OldLayout = ImageLayout.Undefined,
                    NewLayout = ImageLayout.DepthStencilAttachmentOptimal,
                    SrcQueueFamilyIndex = 0xFFFFFFFF,
                    DstQueueFamilyIndex = 0xFFFFFFFF,
                    SrcAccessMask = new(),
                    DstAccessMask = AccessFlags.DepthStencilAttachmentWrite,
                    SubresourceRange = new ImageSubresourceRange()
                    {
                        LayerCount = 1,
                        BaseArrayLayer = 0,
                        LevelCount = 1,
                        BaseMipLevel = 0,
                        AspectMask = ImageAspectFlags.Depth,
                    },
                });
            });
        }
    }
    
    public SwapChainInfo info;
    public Framebuffer[] frameBuffers;

    private readonly SwapchainKhr swapChain;

    private readonly Image[] colorImages;
    private readonly ImageView[] colorImageViews;
    
    private readonly Image[] depthImages;
    private readonly ImageView[] depthImageViews;
    private readonly DeviceMemory[] depthMemories;
}*/