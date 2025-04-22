/*using System.Diagnostics;
using System.Runtime.InteropServices;
using Evergine.Bindings.Vulkan;
using static GLFWDotNet.GLFW;

namespace UnityEngine;

public unsafe class RenderSystem
{
    public RenderSystem(Window window)
    {
        const int maxFlightCount = 2;
        const string vertPath = "";
        const string fragPath = "";
        
        CreateInstance(out instance);
        CreateSurface(window.window, instance, out surface);
        CreatePhysicalDevice(instance, out phyDevice);
        QueueFamilyIndices queueFamilyIndices = QueryFamilyIndices(phyDevice, surface);
        CreateDevice(phyDevice, queueFamilyIndices, out device);
        GetGraphicsQueue(device, queueFamilyIndices, out graphicsQueue);
        GetPresentQueue(device, queueFamilyIndices, out presentQueue);
        
        /*instance = CreateInstance();
        surface = CreateSurface(instance, window!.window);
        PhysicalDevice phyDevice = CreatePhysicalDevice(instance);
        QueueFamilyIndices queueFamilyIndices = QueryFamilyIndices(phyDevice, surface);
        device = CreateDevice(phyDevice, queueFamilyIndices);
        graphicsQueue = GetGraphicsQueue(device, queueFamilyIndices);
        presentQueue = GetPresentQueue(device, queueFamilyIndices);
        commandPool = new CommandPool(device);
        commandBuffers = commandPool.CreateCommandBuffers(device, maxFlightCount);
        swapChain = new SwapChain(window!.Width, window.Height, queueFamilyIndices, surface, phyDevice, device, commandPool, graphicsQueue);
        ShaderModule vertModule = CreateShaderModule(device, vertPath);
        ShaderModule fragModule = CreateShaderModule(device, fragPath);
        renderPipeline = new RenderPipeline(window!.Width, window!.Height, vertModule, fragModule, device, swapChain);
        swapChain.CreateFrameBuffers(window.Width, window.Height, device, renderPipeline.renderPass);
        imageAvailableSems = CreateSemaphores(device, maxFlightCount);
        imageDrawFinishSems= CreateSemaphores(device, maxFlightCount);
        fences = CreateFences(device, maxFlightCount);#1#
    }

    private static void CreateInstance(out VkInstance instance)
    {
        // Api Version
        VkApplicationInfo appInfo = new VkApplicationInfo()
        {
            sType = VkStructureType.VK_STRUCTURE_TYPE_APPLICATION_INFO,
            apiVersion = Version(1, 2, 0),
        };
        
        // Extensions
        string[] extensions = glfwGetRequiredInstanceExtensions();
        IntPtr* extensionsToBytesArray = stackalloc IntPtr[extensions.Length];
        for (int i = 0; i < extensions.Length; i++)
        {
            extensionsToBytesArray[i] = Marshal.StringToHGlobalAnsi(extensions[i]);
        }

        VkInstanceCreateInfo createInfo = new VkInstanceCreateInfo
        {
            sType = VkStructureType.VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO,
            pApplicationInfo = &appInfo,
            enabledExtensionCount = (uint)extensions.Length,
            ppEnabledExtensionNames = (byte**)extensionsToBytesArray,
        };

        // Validation layers
        if (Define.IsDebug)
        {
            string[] validationLayers = new[] { "VK_LAYER_KHRONOS_validation" };
            IntPtr* layersToBytesArray = stackalloc IntPtr[validationLayers.Length];
            for (int i = 0; i < validationLayers.Length; i++)
            {
                layersToBytesArray[i] = Marshal.StringToHGlobalAnsi(validationLayers[i]);
            }
            createInfo.enabledLayerCount = (uint)validationLayers.Length;
            createInfo.ppEnabledLayerNames = (byte**)layersToBytesArray;
        }
        else
        {
            createInfo.enabledLayerCount = 0;
            createInfo.pNext = null;
        }
        
        fixed (VkInstance* instancePtr = &instance)
        {
            RenderHelper.CheckErrors(VulkanNative.vkCreateInstance(&createInfo, null, instancePtr));
        }
        
        static uint Version(uint major, uint minor, uint patch)
        {
            return (major << 22) | (minor << 12) | patch;
        }
    }
    
    private readonly VkInstance instance;

    private static void CreateSurface(IntPtr window, VkInstance instance, out VkSurfaceKHR surface)
    {
        VkWin32SurfaceCreateInfoKHR createInfo = new VkWin32SurfaceCreateInfoKHR()
        {
            sType = VkStructureType.VK_STRUCTURE_TYPE_WIN32_SURFACE_CREATE_INFO_KHR,
            hwnd = window,
            hinstance = Process.GetCurrentProcess().Handle,
        };
        
        fixed (VkSurfaceKHR* surfacePtr = &surface)
        {
            RenderHelper.CheckErrors(VulkanNative.vkCreateWin32SurfaceKHR(instance, &createInfo, null, surfacePtr));
        }
    }
    
    private readonly VkSurfaceKHR surface;
    
    private static void CreatePhysicalDevice(VkInstance instance, out VkPhysicalDevice phyDevice)
    {
        uint deviceCount = 0;
        RenderHelper.CheckErrors(VulkanNative.vkEnumeratePhysicalDevices(instance, &deviceCount, null));
        if (deviceCount == 0)
        {
            Debug.Error("Failed to find GPUs with Vulkan support!");
        }

        VkPhysicalDevice* devices = stackalloc VkPhysicalDevice[(int)deviceCount];
        RenderHelper.CheckErrors(VulkanNative.vkEnumeratePhysicalDevices(instance, &deviceCount, devices));

        phyDevice = devices[0];
        if (phyDevice == default)
        {
            Debug.Error("failed to find a suitable GPU!");
        }
    }

    private readonly VkPhysicalDevice phyDevice;
    
    private static QueueFamilyIndices QueryFamilyIndices(VkPhysicalDevice phyDevice, VkSurfaceKHR surface)
    {
        QueueFamilyIndices indices = new QueueFamilyIndices();
        uint queueFamilyCount = 0;
        VulkanNative.vkGetPhysicalDeviceQueueFamilyProperties(phyDevice, &queueFamilyCount, null);
        VkQueueFamilyProperties* properties = stackalloc VkQueueFamilyProperties[(int)queueFamilyCount];
        VulkanNative.vkGetPhysicalDeviceQueueFamilyProperties(phyDevice, &queueFamilyCount, properties);

        for (uint i = 0; i < queueFamilyCount; i++)
        {
            var prop = properties[i];
            
            if ((prop.queueFlags & VkQueueFlags.VK_QUEUE_GRAPHICS_BIT) != 0)
            {
                indices.graphicsQueue = i;
            }
            
            VkBool32 presentSupport = false;
            RenderHelper.CheckErrors(VulkanNative.vkGetPhysicalDeviceSurfaceSupportKHR(phyDevice, i, surface, &presentSupport));
            if (presentSupport)
            {
                indices.presentQueue = i;
            }
            
            if (indices.HasValue()) break;
        }
        return indices;
    }

    private static void CreateDevice(VkPhysicalDevice phyDevice, QueueFamilyIndices indices, out VkDevice device)
    {
        List<VkDeviceQueueCreateInfo> queueInfos = new();
        float priority = 1.0f;
        // 用HashSet使得2个Queue相同时只添加一个
        foreach (uint queueFamily in new HashSet<uint>() { indices.graphicsQueue!.Value, indices.presentQueue!.Value })
        {
            queueInfos.Add(new VkDeviceQueueCreateInfo()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO,
                queueFamilyIndex = queueFamily,
                queueCount = 1,
                pQueuePriorities = &priority,
            });
        }

        VkPhysicalDeviceFeatures deviceFeatures = default;
        string[] deviceExtensions = new[] { "VK_KHR_swapchain" };
        IntPtr* deviceExtensionsArray = stackalloc IntPtr[deviceExtensions.Length];
        for (int i = 0; i < deviceExtensions.Length; i++)
        {
            string extension = deviceExtensions[i];
            deviceExtensionsArray[i] = Marshal.StringToHGlobalAnsi(extension);
        }
        VkDeviceCreateInfo createInfo = new VkDeviceCreateInfo()
        {
            sType = VkStructureType.VK_STRUCTURE_TYPE_DEVICE_CREATE_INFO,
            pEnabledFeatures = &deviceFeatures,
            enabledExtensionCount = (uint)deviceExtensions.Length,
            ppEnabledExtensionNames = (byte**)deviceExtensionsArray,
        };
        fixed (VkDeviceQueueCreateInfo* queueCreateInfosArrayPtr = &queueInfos.ToArray()[0])
        {
            createInfo.queueCreateInfoCount = (uint)queueInfos.Count;
            createInfo.pQueueCreateInfos = queueCreateInfosArrayPtr;
        }
        
        fixed (VkDevice* devicePtr = &device)
        {
            RenderHelper.CheckErrors(VulkanNative.vkCreateDevice(phyDevice, &createInfo, null, devicePtr));
        }
    }
    
    private readonly VkDevice device;
    
    private static void GetGraphicsQueue(VkDevice device, QueueFamilyIndices indices, out VkQueue graphicsQueue)
    {
        fixed (VkQueue* graphicsQueuePtr = &graphicsQueue)
        {
            VulkanNative.vkGetDeviceQueue(device, indices.graphicsQueue!.Value, 0, graphicsQueuePtr);
        }
    }
    
    private readonly VkQueue graphicsQueue;

    private static void GetPresentQueue(VkDevice device, QueueFamilyIndices indices, out VkQueue presentQueue)
    {
        fixed (VkQueue* presentQueuePtr = &presentQueue)
        {
            VulkanNative.vkGetDeviceQueue(device, indices.presentQueue!.Value, 0, presentQueuePtr);
        }
    }

    private readonly VkQueue presentQueue;

    /*#region Create Function

    private static Instance CreateInstance()
    {
        Instance instance = new Instance (new InstanceCreateInfo()
        {
            ApplicationInfo = new ApplicationInfo
            {
                ApiVersion = Vulkan.Version.Make (1, 3, 0)
            },
            EnabledLayerNames = new string[] { "VK_LAYER_KHRONOS_validation" },
            EnabledExtensionNames = Glfw.GetRequiredInstanceExtensions(),
        });

        return instance;
    }

    private static SurfaceKhr CreateSurface(Instance instance, IntPtr window)
    {
        int result = glfwCreateWindowSurface(((IMarshalling)instance).Handle, window, IntPtr.Zero, out var pSurface);
        if (result != (int)Result.Success)
        {
            //throw new ResultException((Result)result);
            Debug.Error(result);
        }

        return new SurfaceKhr() { Handle = (ulong)pSurface.ToInt64() };

        /*long surface = 0;
        VkResult result = Glfw.CreateWindowSurface(instance.Handle, window.__Instance, IntPtr.Zero, surface);
        if (result != VkResult.VK_SUCCESS)
        {
            Debug.Error("CreateSurface failed");
        }
        return new SurfaceKhr() { Handle = (ulong)surface };#2#
    }

    private static PhysicalDevice CreatePhysicalDevice(Instance instance)
    {
        return instance.EnumeratePhysicalDevices()[0];
    }

    private static QueueFamilyIndices QueryFamilyIndices(PhysicalDevice phyDevice, SurfaceKhr surface)
    {
        QueueFamilyIndices indices = new QueueFamilyIndices();
        var properties = phyDevice.GetQueueFamilyProperties();
        for (uint i = 0; i < properties.Length; i++)
        {
            var prop = properties[i];
            if (prop.QueueFlags == QueueFlags.Graphics)
            {
                indices.graphicsQueue = i;
            }
            if (phyDevice.GetSurfaceSupportKHR(i, surface))
            {
                indices.presentQueue = i;
            }
            if (indices.HasValue()) break;
        }
        return indices;
    }

    private static Device CreateDevice(PhysicalDevice phyDevice, QueueFamilyIndices queueFamilyIndices)
    {
        List<DeviceQueueCreateInfo> queueInfo = new();

        // 如果2个队列相同, 只需要创建一份
        if (queueFamilyIndices.presentQueue == queueFamilyIndices.graphicsQueue)
        {
            queueInfo.Add(new()
            {
                QueuePriorities = new float[] { 1 },
                QueueCount = 1,
                QueueFamilyIndex = queueFamilyIndices.graphicsQueue!.Value
            });
        }
        else
        {
            queueInfo.Add(new()
            {
                QueuePriorities = new float[] { 1 },
                QueueCount = 1,
                QueueFamilyIndex = queueFamilyIndices.graphicsQueue!.Value
            });
            queueInfo.Add(new()
            {
                QueuePriorities = new float[] { 1 },
                QueueCount = 1,
                QueueFamilyIndex = queueFamilyIndices.presentQueue!.Value
            });
        }

        return phyDevice.CreateDevice(new DeviceCreateInfo()
        {
            QueueCreateInfos = queueInfo.ToArray(),
            EnabledExtensionNames = new string[] { "VK_KHR_swapchain" }
        });
    }

    private static Queue GetGraphicsQueue(Device device, QueueFamilyIndices indices)
    {
        return device.GetQueue(indices.graphicsQueue!.Value, 0);
    }

    private static Queue GetPresentQueue(Device device, QueueFamilyIndices indices)
    {
        return device.GetQueue(indices.presentQueue!.Value, 0);
    }

    private static ShaderModule CreateShaderModule(Device device, string path)
    {
        return device.CreateShaderModule(new ShaderModuleCreateInfo()
        {
            Code = Tools.ReadFileUints(path)
        });
    }

    private static Semaphore[] CreateSemaphores(Device device, int maxFlightCount)
    {
        Semaphore[] sems = new Semaphore[maxFlightCount];
        for (int i = 0; i < maxFlightCount; ++i)
        {
            sems[i] = CreateSemaphore(device);
        }
        return sems;
    }
    private static Semaphore CreateSemaphore(Device device)
    {
        return device.CreateSemaphore(new SemaphoreCreateInfo());
    }

    private static Fence[] CreateFences(Device device, int maxFlightCount)
    {
        Fence[] fences = new Fence[maxFlightCount];
        for (int i = 0; i < maxFlightCount; ++i)
        {
            fences[i] = CreateFence(device);
        }
        return fences;
    }
    private static Fence CreateFence(Device device)
    {
        return device.CreateFence(new FenceCreateInfo()
        {
            Flags = FenceCreateFlags.Signaled
        });
    }

    private static List<(Buffer, Buffer)> CreateBuffers()
    {
        return null;
    }

    private static (Buffer, Buffer) CreateBuffer()
    {
        return (null, null);
    }

    private static void UpdateBuffers()
    {

    }

    private static void UpdateBuffer()
    {

    }

    private static Sampler CreateSampler(Device device)
    {
        return device.CreateSampler(new SamplerCreateInfo()
        {
            MagFilter = Filter.Linear,
            MinFilter = Filter.Linear,
            AddressModeU = SamplerAddressMode.Repeat,
            AddressModeV = SamplerAddressMode.Repeat,
            AddressModeW = SamplerAddressMode.Repeat,
            AnisotropyEnable = false,
            BorderColor = BorderColor.IntOpaqueBlack,
            UnnormalizedCoordinates = false,
            CompareEnable = false,
            MipmapMode = SamplerMipmapMode.Linear,
        });
    }

    private static DescriptorPool CreateDescriptorPool(Device device, int maxFlightCount, DescriptorType[] types)
    {
        DescriptorPoolSize[] poolSizes = new DescriptorPoolSize[types.Length];
        for (int i = 0; i < types.Length; ++i)
        {
            poolSizes[i].Type = types[i];
            poolSizes[i].DescriptorCount = (uint)maxFlightCount;
        }

        return device.CreateDescriptorPool(new DescriptorPoolCreateInfo()
        {
            Flags = DescriptorPoolCreateFlags.FreeDescriptorSet,
            MaxSets = (uint)maxFlightCount,
            PoolSizes = poolSizes,
        });
    }

    private static DescriptorSet[] CreateDescriptorSets(int maxFlightCount, int setIndex, Device device, RenderPipeline pipeline, DescriptorPool descriptorPool)
    {
        return null;
    }

    private static void UpdateDescriptorSets(int binding, int dataSize, Buffer[] deviceBuffer, DescriptorSet[] descriptorSets, Device device)
    {

    }

    private static void UpdateDescriptorSets()
    {

    }
    #endregion#1#

    /*#region Fields

    private static Instance instance;
    private static SurfaceKhr surface;
    private static Device device;
    private static Queue graphicsQueue;
    private static Queue presentQueue;
    private static CommandPool commandPool;
    private static CommandBuffer[] commandBuffers;
    private static SwapChain swapChain;
    private static RenderPipeline renderPipeline;
    private static Semaphore[] imageAvailableSems;
    private static Semaphore[] imageDrawFinishSems;
    private static Fence[] fences;

    private static Buffer deviceVertBuffer;
    private static Buffer hostVertBuffer;
    private static Buffer deviceIndexBuffer;
    private static Buffer hostIndexBuffer;

    private static Buffer[] deviceVpUniformBuffer;
    private static Buffer[] hostVpUniformBuffer;

    private static Texture texture;
    private static Sampler sampler;

    private static DescriptorPool descriptorPool;
    private static DescriptorSet[] descriptorSets;

    private static int curFrame = 0;

    #endregion#1#
}

public static class RenderHelper
{
    [Conditional("DEBUG")]
    public static void CheckErrors(VkResult result)
    {
        if (result != VkResult.VK_SUCCESS)
        {
            throw new InvalidOperationException(result.ToString());
        }
    }
}*/