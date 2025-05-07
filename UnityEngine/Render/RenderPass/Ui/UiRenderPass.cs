using System.Diagnostics;
using System.Numerics;
using System.Text;
using ImGuiNET;
using UnityEngine.Events;
using Veldrid;

namespace UnityEngine;

public sealed class UiRenderPass : RenderPass
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
        style.Colors[(int)ImGuiCol.FrameBg] = new Color(0.4f); // 输入背景色
        style.Colors[(int)ImGuiCol.FrameBgHovered] = new Color(0.5f); // 输入悬停颜色
        style.Colors[(int)ImGuiCol.FrameBgActive] = new Color(0.4f); // 输入点击颜色
        style.Colors[(int)ImGuiCol.Header] = new Color(0.3f); // 鼠标选中颜色
        style.Colors[(int)ImGuiCol.HeaderHovered] = new Color(0.4f); // 鼠标悬停颜色
        style.Colors[(int)ImGuiCol.HeaderActive] = new Color(0.5f); // 鼠标点击颜色
        style.Colors[(int)ImGuiCol.MenuBarBg] = Color.Gray; // 工具栏背景色
        style.Colors[(int)ImGuiCol.CheckMark] = Color.Gray; // 工具栏背景色
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
        #region Debug
        ImGui.Begin("Debug", ImGuiWindowFlags.MenuBar);
        
        float messagePosX = ImGui.GetCursorPos().X;
        float y = ImGui.GetCursorPos().Y;
        foreach (var info in GetDebug(level))
        {
            ImGui.SetCursorPos(new Vector2(messagePosX, y));
            ImGui.TextColored(info.color, $"[{info.level}]: {info.message}");
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip($"({info.path}, {info.line})  {info.time}");
            }
            y += 30;
        }
        
        if (ImGui.BeginMenuBar())
        {
            ImGui.Checkbox("Info", ref showInfo);
            if (showInfo)
            {
                level |= DebugLevel.Info;
            }
            else
            {
                level &= ~DebugLevel.Info;
            }
            
            ImGui.Checkbox("Warning", ref showWarning);
            if (showWarning)
            {
                level |= DebugLevel.Warning;
            }
            else
            {
                level &= ~DebugLevel.Warning;
            }
            
            ImGui.Checkbox("Error", ref showError);
            if (showError)
            {
                level |= DebugLevel.Error;
            }
            else
            {
                level &= ~DebugLevel.Error;
            }
            
            if (ImGui.Button("Clear"))
            {
                ClearDebug();
            }

            ImGui.Text($"Count: {debugInfos.Count}");
            
            ImGui.EndMenuBar();
        }
        
        ImGui.End();
        #endregion
        #region Asset
        ImGui.Begin("Asset", ImGuiWindowFlags.MenuBar);
        // 点击空白清空选中文件
        if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !ImGui.IsAnyItemHovered())
        {
            selectedFile = null;
        }
        // Build
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.Button("Rebuild"))
            {
                FileSystem.Build();
            }
            
            ImGui.EndMenuBar();
        }
        DrawAsset(FileSystem.root, ref selectedFile);
        ImGui.End();

        static void DrawAsset(TreeNode<FileSystem.FileInfo> node, ref FileSystem.FileInfo selectedFile)
        {
            FileSystem.FileInfo info = node;
            bool isDirectory = info.IsDirectory;
            bool isFile = info.IsFile;
            if (!isDirectory && !isFile) return;
            
            ImGui.PushID(info.Path);
            
            var flags = ImGuiTreeNodeFlags.DefaultOpen;
            if (selectedFile != null && selectedFile.Path == info.Path)
            {
                flags |= ImGuiTreeNodeFlags.Selected;
            }
            if (node.Children.Count <= 0)
            {
                flags |= ImGuiTreeNodeFlags.Leaf;
            }

            if (isDirectory)
            {
                bool needOpen = ImGui.TreeNodeEx(info.Name, flags);
                
                // 左键点击选择
                if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                {
                    selectedFile = node;
                }
                
                // 右键菜单
                if (ImGui.BeginPopupContextItem("context"))
                {
                    // 在资源管理器打开文件夹
                    if (ImGui.MenuItem("Open"))
                    {
                        Process.Start("explorer.exe", $"/select,\"{info.Path}\"");
                    }
                    
                    ImGui.EndPopup();
                }

                if (needOpen)
                {
                    for(int i = node.Children.Count - 1; i >= 0; --i)
                    {
                        var child = node.Children[i];
                        DrawAsset(child, ref selectedFile);
                    }   
                    ImGui.TreePop();
                }
            }
            else // isFile
            {
                bool needOpen = ImGui.TreeNodeEx(info.Name, flags);
                
                // 左键点击选择
                if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                {
                    selectedFile = node;
                }
                
                // 右键菜单
                if (ImGui.BeginPopupContextItem("context"))
                {
                    // 使用默认方式打开文件
                    if (ImGui.MenuItem("Open"))
                    {
                        Process.Start(new ProcessStartInfo()
                        {
                            FileName = info.Path,
                            UseShellExecute = true,
                        });
                    }
                    
                    ImGui.EndPopup();
                }

                if (needOpen)
                {
                    ImGui.TreePop();
                }
            }
            
            ImGui.PopID();
        }
        #endregion
        #region Scene
        ImGui.Begin("Scene", ImGuiWindowFlags.MenuBar);
        // 点击空白清空选中Object
        if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !ImGui.IsAnyItemHovered())
        {
            selectedObject = null;
        }
        // Build
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.Button("Rebuild"))
            {
                scene.Build();
            }
            
            ImGui.EndMenuBar();
        }
        DrawScene(scene, ref renameInfo, ref renameBuffer, ref selectedObject);
        ImGui.End();

        static void DrawScene(TreeNode<Scene.SceneObjectInfo> node, ref Scene.SceneObjectInfo renameInfo, ref byte[] renameBuffer, ref Scene.SceneObjectInfo selectedObject)
        {
            ImGui.PushID(node.GetHashCode());

            Scene.SceneObjectInfo info = node;

            // 重命名
            if (renameInfo != null && renameInfo == info)
            {
                if (ImGui.InputText("##rename", renameBuffer, (uint)renameBuffer.Length, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    int len = Array.IndexOf(renameBuffer, (byte)0);
                    if (len < 0) len = renameBuffer.Length;
                    info.gameObject.name = Encoding.UTF8.GetString(renameBuffer, 0, len);

                    renameInfo = null;
                    renameBuffer = null;
                }
                
                if (ImGui.IsItemDeactivated())
                {
                    renameInfo = null;
                    renameBuffer = null;
                }
            }
            else
            {
                var flags = ImGuiTreeNodeFlags.DefaultOpen;
                if (selectedObject != null && selectedObject.gameObject.GetHashCode() == info.gameObject.GetHashCode())
                {
                    flags |= ImGuiTreeNodeFlags.Selected;
                }
                if (node.Children.Count <= 0)
                {
                    flags |= ImGuiTreeNodeFlags.Leaf;
                }
                
                bool nodeOpen = ImGui.TreeNodeEx($"{info.gameObject.name}##{node.GetHashCode()}", flags);
                
                // 左键点击选择
                if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                {
                    selectedObject = node;
                }

                // 右键菜单
                if (ImGui.BeginPopupContextItem("context"))
                {
                    if (ImGui.MenuItem("ReName"))
                    {
                        renameBuffer = new byte[256];
                        Encoding.UTF8.GetBytes(info.gameObject.name, renameBuffer);
                        renameInfo = info;
                    }
                
                    if (ImGui.MenuItem("Add"))
                    {
                        GameObject gameObject = new GameObject($"Game Object {node.Children.Count + 1}")
                        {
                            Parent = node.Value
                        };
                    }

                    if (ImGui.MenuItem("Remove"))
                    {
                        GameObject obj = node.Value;
                        obj.Dispose();
                    }
                
                    ImGui.EndPopup();
                }
            
                if (nodeOpen)
                {
                    for (int i = node.Children.Count - 1; i >= 0; i--)
                    {
                        var child = node.Children[i];
                        DrawScene(child, ref renameInfo, ref renameBuffer, ref selectedObject);
                    }
                    ImGui.TreePop();
                }
            }
            
            ImGui.PopID();
        }
        #endregion
        #region Editor
        ImGui.Begin("Editor");
        ImGui.Image(objectsRenderView, new Vector2(objectsRender.Width, objectsRender.Height));
        ImGui.End();
        #endregion
        #region Run
        ImGui.Begin("Run");
        ImGui.Image(objectsRenderView, new Vector2(objectsRender.Width, objectsRender.Height));
        ImGui.End();
        #endregion
        #region Setting
        ImGui.Begin("Setting");
        if (selectedObject != null)
        {
            DrawSettingObject(selectedObject);
        }
        ImGui.End();
        
        static void DrawSettingObject(Scene.SceneObjectInfo selectedObject)
        {
            GameObject gameObject = selectedObject;
            ImGui.Text(gameObject.name);
            foreach (MonoBehaviour component in gameObject.Components.Values)
            {
                if (ImGui.CollapsingHeader(component.ToString(), ImGuiTreeNodeFlags.DefaultOpen))
                {
                    component.DrawSetting();
                    ImGui.NewLine();
                }   
            }
        }
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
    public IEnumerable<DebugEvent> GetDebug(DebugLevel level)
    {
        foreach (DebugEvent info in debugInfos)
        {
            if ((info.level & level) != 0)
            {
                yield return info;
            }
        }
    }

    private bool showInfo = true, showWarning = true, showError = true;
    public DebugLevel level = DebugLevel.All;
    private readonly List<DebugEvent> debugInfos;
    #endregion
    #region Asset
    private FileSystem.FileInfo selectedFile;
    #endregion
    #region Scene
    private Scene.SceneObjectInfo selectedObject;
    private Scene.SceneObjectInfo renameInfo = null;
    private byte[] renameBuffer = null;
    private readonly Scene scene = Scene.ActiveScene;
    #endregion
    #region Editor
    private readonly Texture objectsRender;
    private readonly IntPtr objectsRenderView;
    #endregion
    
    private readonly GraphicsDevice device;
    private readonly ImGuiController uiRenderer;
    
    private class WindowResizeEventHandler : AEvent<WindowResizeEvent>
    {
        private static ImGuiController uiRenderer; // TODO 不应该有状态, 目前用static保证合理

        public WindowResizeEventHandler(ImGuiController uiRenderer)
        {
            WindowResizeEventHandler.uiRenderer = uiRenderer;
        }
        
        protected override async Task Run(WindowResizeEvent a)
        {
            uiRenderer.WindowResized(a.width, a.height);
            await Task.CompletedTask;
        }
    }
    
    private class DebugEventHandler : AEvent<DebugEvent>
    {
        private static List<DebugEvent> debugInfos; // TODO 不应该有状态, 目前用static保证合理

        public DebugEventHandler(List<DebugEvent> debugInfos)
        {
            DebugEventHandler.debugInfos = debugInfos;
        }
        
        protected override async Task Run(DebugEvent a)
        {
            debugInfos.Add(a);
            await Task.CompletedTask;
        }
    }
}