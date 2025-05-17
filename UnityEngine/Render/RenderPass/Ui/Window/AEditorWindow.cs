using ImGuiNET;

namespace UnityEngine;

public abstract class AEditorWindow
{
    public abstract void Draw();

    public string name;
    public ImGuiWindowFlags flags;
}