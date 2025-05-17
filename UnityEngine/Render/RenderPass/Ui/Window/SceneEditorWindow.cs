using System.Numerics;
using ImGuiNET;
using Veldrid;

namespace UnityEngine;

public class SceneEditorWindow : AEditorWindow
{
    public SceneEditorWindow(GraphicsDevice device, ImGuiController uiController, Texture render)
    {
        name = "Scene";
        flags = ImGuiWindowFlags.MenuBar;
        
        renderView = uiController.GetOrCreateImGuiBinding(device.ResourceFactory, device.ResourceFactory.CreateTextureView(render));
        width = render.Width;
        height = render.Height;
    }
    
    public override void Draw()
    {
        // TODO Scene视图下还需要加工
        ImGui.Image(renderView, new Vector2(width, height));
    }
    
    private readonly IntPtr renderView;
    private readonly float width, height;
}