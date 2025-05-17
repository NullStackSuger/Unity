using System.Numerics;
using ImGuiNET;
using Veldrid;

namespace UnityEngine;

public class GameEditorWindow : AEditorWindow
{
    public GameEditorWindow(GraphicsDevice device, ImGuiController uiController, Texture render)
    {
        name = "Game";
        flags = ImGuiWindowFlags.MenuBar;
        
        renderView = uiController.GetOrCreateImGuiBinding(device.ResourceFactory, device.ResourceFactory.CreateTextureView(render));
        width = render.Width;
        height = render.Height;
    }
    
    public override void Draw()
    {
        ImGui.Image(renderView, new Vector2(width, height));
    }

    private readonly IntPtr renderView;
    private readonly float width, height;
}