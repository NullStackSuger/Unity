using System.Numerics;
using ImGuiNET;
using UnityEngine.Events;
using Veldrid;
using Veldrid.Sdl2;

namespace UnityEngine;

public class UiRenderPass : RenderPass
{
    public UiRenderPass(GraphicsDevice device, Texture objectsRender)
    {
        var window = Window.window;
        this.debugInfos = new List<DebugEvent>(10);
        this.device = device;
        uiRenderer = new ImGuiController(device, device.MainSwapchain.Framebuffer.OutputDescription, window.Width, window.Height);

        // 场景渲染结果
        this.objectsRender = objectsRender;
        var resultView = device.ResourceFactory.CreateTextureView(this.objectsRender);
        objectsRenderView = uiRenderer.GetOrCreateImGuiBinding(device.ResourceFactory, resultView);

        // 事件
        EventSystem.Add(new WindowResizeEventHandler(uiRenderer));
        EventSystem.Add(new DebugEventHandler(debugInfos));
        
        // 设置Ui格式
        var style = ImGui.GetStyle();
        style.Colors[(int)ImGuiCol.WindowBg] = Color.Gray; // 窗口背景色
        style.Colors[(int)ImGuiCol.TitleBg] = Color.Gray; // 页标颜色
        style.Colors[(int)ImGuiCol.TitleBgActive] = Color.Gray; // 活动页表颜色
        style.Colors[(int)ImGuiCol.DockingPreview] = new Color(0.3f); // 吸附到窗口颜色
        style.Colors[(int)ImGuiCol.DockingEmptyBg] = Color.Gray; // 吸附到空地颜色
        style.Colors[(int)ImGuiCol.TabHovered] = new Color(0.5f); // 页签悬停颜色
        style.Colors[(int)ImGuiCol.Tab] = Color.Gray; // 页签默认颜色
        style.Colors[(int)ImGuiCol.TabSelected] = new Color(0.15f); // 选中页签颜色
        style.Colors[(int)ImGuiCol.TabSelectedOverline] = new Color(0.5f); // 选中页签的线的颜色
        style.Colors[(int)ImGuiCol.TabDimmed] = Color.Gray; // 没选中的页签颜色
        style.Colors[(int)ImGuiCol.TabDimmedSelected] = Color.Gray; // 没选中的页签颜色(仅之前选过的)
        style.Colors[(int)ImGuiCol.ResizeGrip] = Color.Gray; // 窗口右下角调整大小图标的颜色
        style.Colors[(int)ImGuiCol.ResizeGripHovered] = new Color(0.5f); // 窗口横向调整大小的颜色
        style.Colors[(int)ImGuiCol.ResizeGripActive] = new Color(0.5f); // 窗口竖向调整大小的颜色
        style.Colors[(int)ImGuiCol.Button] = new Color(0.5f); // 按钮颜色
        style.Colors[(int)ImGuiCol.ButtonHovered] = new Color(0.4f); // 按钮悬停
        style.Colors[(int)ImGuiCol.ButtonActive] = new Color(0.3f); // 按钮点击
    }
    
    public override void Tick(CommandList commandList)
    {
        InputSnapshot snapshot = Input.Snapshot;
        float deltaTime = Time.DeltaTime;
        
        commandList.SetFramebuffer(device.MainSwapchain.Framebuffer);
        commandList.ClearColorTarget(0, new RgbaFloat(0.1f, 0.1f, 0.1f, 1.0f));
        
        uiRenderer.Update(deltaTime, snapshot);
        #region DockSpace BG
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, Vector4.Zero); 
        ImGui.SetNextWindowPos(Vector2.Zero);
        ImGui.SetNextWindowSize(ImGui.GetIO().DisplaySize);
        ImGui.SetNextWindowViewport(ImGui.GetMainViewport().ID);
        ImGui.Begin("DockSpace Window",
            ImGuiWindowFlags.NoTitleBar |
            ImGuiWindowFlags.NoCollapse |
            ImGuiWindowFlags.NoResize |
            ImGuiWindowFlags.NoMove |
            ImGuiWindowFlags.NoBringToFrontOnFocus |
            ImGuiWindowFlags.NoNavFocus |
            ImGuiWindowFlags.NoDocking |
            ImGuiWindowFlags.NoBackground);
        
        ImGui.DockSpace(ImGui.GetID("DockSpace"), Vector2.Zero, ImGuiDockNodeFlags.None);
        
        ImGui.End();
        ImGui.PopStyleColor();
        ImGui.PopStyleVar(2);
        #endregion
        #region Editor
        ImGui.Begin("Editor");
        ImGui.Image(objectsRenderView, new System.Numerics.Vector2(objectsRender.Width, objectsRender.Height));
        ImGui.End();
        #endregion
        #region Run
        ImGui.Begin("Run");
        ImGui.End();
        #endregion
        #region Debug
        ImGui.Begin("Debug");
            
        Vector2 clearButtonPos = ImGui.GetCursorPos() + new Vector2(ImGui.GetWindowSize().X - 50, -10);
        ImGui.SetCursorPos(clearButtonPos);
        if (ImGui.Button("Clear"))
        {
            ClearDebug();
        }
        
        float messagePosX = ImGui.GetCursorPos().X;
        float extentMessagePosX = messagePosX + ImGui.GetWindowSize().X - 300;
        float y = ImGui.GetCursorPos().Y;
        foreach (var info in GetDebug(level))
        {
            ImGui.SetCursorPos(new Vector2(messagePosX, y));
            ImGui.TextColored(info.color, $"[{info.level}]: {info.message}");
            ImGui.SetCursorPos(new Vector2(extentMessagePosX, y));
            ImGui.Text($"({info.path}, {info.line})  {info.time}");
            y += 30;
        }
        
        ImGui.End();
        #endregion
        #region Asset
        ImGui.Begin("Asset");
        ImGui.End();
        #endregion
        #region Scene
        ImGui.Begin("Scene");
        ImGui.End();
        #endregion
        #region Setting
        ImGui.Begin("Setting");
        ImGui.End();
        #endregion
        
        uiRenderer.Render(device, commandList);
    }

    #region Debug
    /// <summary>
    /// 清除输出信息
    /// </summary>
    public void ClearDebug()
    {
        debugInfos.Clear();
    }

    /// <summary>
    /// 获取输出信息
    /// </summary>
    public IEnumerable<DebugEvent> GetDebug(DebugLevel level = DebugLevel.All)
    {
        foreach (DebugEvent info in debugInfos)
        {
            if ((info.level & level) != 0)
            {
                yield return info;
            }
        }
    }

    public DebugLevel level = DebugLevel.All;
    private readonly List<DebugEvent> debugInfos;
    #endregion

    #region Scene
    private readonly Texture objectsRender;
    private readonly IntPtr objectsRenderView;
    #endregion
    
    private readonly GraphicsDevice device;
    private readonly ImGuiController uiRenderer;
    
    private class WindowResizeEventHandler : AEvent<WindowResizeEvent>
    {
        private readonly ImGuiController uiRenderer;

        public WindowResizeEventHandler(ImGuiController uiRenderer)
        {
            this.uiRenderer = uiRenderer;
        }
        
        protected override async Task Run(WindowResizeEvent a)
        {
            uiRenderer.WindowResized(a.width, a.height);
            await Task.CompletedTask;
        }
    }
    
    private class DebugEventHandler : AEvent<DebugEvent>
    {
        private readonly List<DebugEvent> debugInfos;

        public DebugEventHandler(List<DebugEvent> debugInfos)
        {
            this.debugInfos = debugInfos;
        }
        
        protected override async Task Run(DebugEvent a)
        {
            debugInfos.Add(a);
            await Task.CompletedTask;
        }
    }
}