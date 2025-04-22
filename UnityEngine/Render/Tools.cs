/*using Vulkan;

namespace UnityEngine;

public static partial class Tools
{
    public static uint QueryMemoryIndex(MemoryPropertyFlags propFlag, MemoryRequirements req, PhysicalDevice phyDevice)
    {
        var props = phyDevice.GetMemoryProperties();
        for (int i = 0; i < props.MemoryTypeCount; i++)
        {
            if ((1 << i) == req.MemoryTypeBits && props.MemoryTypes[i].PropertyFlags == propFlag)
            {
                return (uint)i;
            }
        }
        Debug.Error("Buffer does not have a valid memory type!");
        return 0;
    }
}*/