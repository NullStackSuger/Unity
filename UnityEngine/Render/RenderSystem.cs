using UnityEngine.Events;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace UnityEngine;

public static class RenderSystem
{
    static RenderSystem()
    {
        window = Window.window;
        
        device = VeldridStartup.CreateGraphicsDevice
        (
            window, new GraphicsDeviceOptions(true, null, true, ResourceBindingModel.Improved, true, true)
            //,GraphicsBackend.Vulkan //Vulkan的似乎有Bug, 窗口关闭时大小异常
        );
        commandList = device.ResourceFactory.CreateCommandList();

        renderPasses = [new ObjectsRenderPass(device), new UiRenderPass(device)];
        
        EventSystem.Add(new WindowResizeEventHandler());
    }

    public static void Tick()
    {

        commandList.Begin();
        commandList.SetFramebuffer(device.MainSwapchain.Framebuffer);
        commandList.ClearColorTarget(0, new RgbaFloat(0.1f, 0.1f, 0.1f, 1.0f));

        foreach (RenderPass pass in renderPasses)
        {
            pass.Tick(commandList);
        }
        
        commandList.End();
        device.SubmitCommands(commandList);
        device.SwapBuffers();
    }

    private static readonly Sdl2Window window;
    private static readonly GraphicsDevice device;
    private static readonly CommandList commandList;
    private static readonly RenderPass[] renderPasses;
    
    private class WindowResizeEventHandler : AEvent<WindowResizeEvent>
    {
        protected override async Task Run(WindowResizeEvent a)
        {
            device.MainSwapchain.Resize((uint)window.Width, (uint)window.Height);
            await Task.CompletedTask;
        }
    }
}