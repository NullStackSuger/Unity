using System.Numerics;
using ImGuiNET;

namespace UnityEngine;

public static partial class Helper
{
    public static void DrawScroll<T>(float height, IEnumerable<T> items, Action<T> drawItem)
    {
        ImGui.BeginChild("Scroll", new Vector2(0, height), ImGuiChildFlags.None, ImGuiWindowFlags.HorizontalScrollbar);
        foreach (T item in items)
        {
            drawItem.Invoke(item);
        }
        if (ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
        {
            ImGui.SetScrollHereY(1.0f);
        }
        ImGui.EndChild();
    }

    public static void DrawLeftRight(Action left, Action right, float leftOffset = 0, float rightOffset = 0)
    {
        ImGui.SetCursorPosX(0 + leftOffset);
        left.Invoke();
        
        float contentWidth = ImGui.GetContentRegionAvail().X;
        float lineHeight = ImGui.GetTextLineHeight();
        float y = ImGui.GetCursorPosY();
        ImGui.SetCursorPos(new Vector2(contentWidth - rightOffset, y - lineHeight));
        right.Invoke();
    }

    public static void DrawMenuBar(Action callback)
    {
        if (ImGui.BeginMenuBar())
        {
            callback.Invoke();
            
            ImGui.EndMenuBar();
        }
    }
}