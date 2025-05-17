using ImGuiNET;

namespace UnityEngine;

public class InspectorEditorWindow : AEditorWindow
{
    public InspectorEditorWindow()
    {
        name = "Inspector";
        flags = ImGuiWindowFlags.MenuBar;
    }
    
    public override void Draw()
    {
        
    }
}