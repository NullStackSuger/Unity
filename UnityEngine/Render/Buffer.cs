/*using Vulkan;
namespace UnityEngine;

public class Buffer
{
    public Buffer(ulong size, BufferUsageFlags usage, MemoryPropertyFlags properties, PhysicalDevice phyDevice, Device device)
    {
        buffer = device.CreateBuffer(new BufferCreateInfo()
        {
            Size = size,
            Usage = usage,
            SharingMode = SharingMode.Exclusive,
        });

        MemoryRequirements req = device.GetBufferMemoryRequirements(buffer);
        uint index = Tools.QueryMemoryIndex(properties, req, phyDevice);

        memory = device.AllocateMemory(new MemoryAllocateInfo()
        {
            MemoryTypeIndex = index,
            AllocationSize = req.Size,
        });
        
        device.BindBufferMemory(buffer, memory, 0);
    }

    public static explicit operator Vulkan.Buffer(Buffer buffer)
    {
        return buffer.buffer;
    }

    private Vulkan.Buffer buffer;
    private DeviceMemory memory;
}*/