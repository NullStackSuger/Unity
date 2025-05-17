using System.Numerics;
using ImGuiNET;

namespace UnityEngine;

public class BackGroundEditorWindow : AEditorWindow
{
    public BackGroundEditorWindow()
    {
        name = "Back Ground";
        flags = ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.NoCollapse |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoBringToFrontOnFocus |
                ImGuiWindowFlags.NoNavFocus | 
                ImGuiWindowFlags.NoBackground;
    }
    
    public override void Draw()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, Vector4.Zero); 
        ImGuiViewportPtr viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(viewport.Pos);
        ImGui.SetNextWindowSize(viewport.Size);
        ImGui.SetNextWindowViewport(viewport.ID);
        
        ImGui.DockSpace(ImGui.GetID("Back Ground"), Vector2.Zero, ImGuiDockNodeFlags.None);
        
        ImGui.PopStyleColor();
        ImGui.PopStyleVar(2);
    }
}