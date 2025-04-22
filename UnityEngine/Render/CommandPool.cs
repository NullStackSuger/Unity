/*using Evergine.Bindings.Vulkan;
using Vulkan;

namespace UnityEngine;

public unsafe class CommandPool
{
    public CommandPool(VkDevice device)
    {
        VkCommandPoolCreateInfo poolInfo = new VkCommandPoolCreateInfo()
        {
            sType = VkStructureType.VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO,
            flags = VkCommandPoolCreateFlags.VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT,
        };
        
        fixed (VkCommandPool* commandPoolPtr = &this.commandPool)
        {
            RenderHelper.CheckErrors(VulkanNative.vkCreateCommandPool(device, &poolInfo, null, commandPoolPtr));
        }
    }

    public void CreateCommandBuffers(VkDevice device, int size, out VkCommandBuffer[] commandBuffers)
    {
        commandBuffers = new VkCommandBuffer[size];
        VkCommandBufferAllocateInfo allocInfo = new VkCommandBufferAllocateInfo()
        {
            sType = VkStructureType.VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO,
            commandPool = commandPool,
            commandBufferCount = (uint)commandBuffers.Length,
            level = VkCommandBufferLevel.VK_COMMAND_BUFFER_LEVEL_PRIMARY,
        };
        fixed (VkCommandBuffer* commandBuffersPtr = &commandBuffers[0])
        {
            RenderHelper.CheckErrors(VulkanNative.vkAllocateCommandBuffers(device, &allocInfo, commandBuffersPtr));
        }
    }

    public void CreateCommandBuffer(VkDevice device, out VkCommandBuffer commandBuffer)
    {
        VkCommandBufferAllocateInfo allocInfo = new VkCommandBufferAllocateInfo()
        {
            sType = VkStructureType.VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO,
            commandPool = commandPool,
            commandBufferCount = 1,
            level = VkCommandBufferLevel.VK_COMMAND_BUFFER_LEVEL_PRIMARY,
        };
        fixed (VkCommandBuffer* commandBuffersPtr = &commandBuffer)
        {
            RenderHelper.CheckErrors(VulkanNative.vkAllocateCommandBuffers(device, &allocInfo, commandBuffersPtr));
        }
    }

    public void Execute(VkQueue queue, VkDevice device, Action<VkCommandBuffer> callback)
    {
        CreateCommandBuffer(device, out var commandBuffer);
        
        // Begin
        VkCommandBufferBeginInfo beginInfo = new VkCommandBufferBeginInfo()
        {
            sType = VkStructureType.VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO,
            flags = VkCommandBufferUsageFlags.VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT,
        };
        RenderHelper.CheckErrors(VulkanNative.vkBeginCommandBuffer(commandBuffer, &beginInfo));
        
        callback(commandBuffer);
        
        // End
        RenderHelper.CheckErrors(VulkanNative.vkEndCommandBuffer(commandBuffer));

        // Submit
        VkSubmitInfo submitInfo = new VkSubmitInfo()
        {
            sType = VkStructureType.VK_STRUCTURE_TYPE_SUBMIT_INFO,
            commandBufferCount = 1,
            pCommandBuffers = &commandBuffer,
        };
        RenderHelper.CheckErrors(VulkanNative.vkQueueSubmit(queue, 1, &submitInfo));
        
        commandBuffer.Begin(new CommandBufferBeginInfo() { Flags = CommandBufferUsageFlags.OneTimeSubmit });
        callback(commandBuffer);
        commandBuffer.End();
        
        queue.Submit(new SubmitInfo() { CommandBuffers = new [] { commandBuffer } });
        queue.WaitIdle();
        device.WaitIdle();
    }

    public static explicit operator VkCommandPool(CommandPool commandPool)
    {
        return commandPool.commandPool;
    }

    private readonly VkCommandPool commandPool;
}*/