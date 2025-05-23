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

        var prepareRenderPass = new PrepareRenderPass();
        var shadowRenderPass = new ShadowRenderPass(device, 800, 600, prepareRenderPass.Objects);
        var objectsRenderPass = new ObjectRenderPass(device, 800, 600, prepareRenderPass.Objects, shadowRenderPass.shadowMap);
        var uiRenderPass = new UiRenderPass(device, objectsRenderPass.result);
        renderPasses = [prepareRenderPass, shadowRenderPass, objectsRenderPass, uiRenderPass];
        
        EventSystem.Add(new WindowResizeEventHandler());
    }

    public static void Tick()
    {
        commandList.Begin();

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
    public static readonly RenderPass[] renderPasses;
    
    private class WindowResizeEventHandler : AEvent<WindowResizeEvent>
    {
        protected override async Task Run(WindowResizeEvent a)
        {
            device.MainSwapchain.Resize((uint)window.Width, (uint)window.Height);
            await Task.CompletedTask;
        }
    }
}