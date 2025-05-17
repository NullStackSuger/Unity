using System.Numerics;
using ImGuiNET;
using UnityEngine.Events;

namespace UnityEngine;

public class ConsoleEditorWindow : AEditorWindow
{
    public ConsoleEditorWindow()
    {
        name = "Console";
        flags = ImGuiWindowFlags.MenuBar;
    }
    
    public override void Draw()
    {
        Helper.DrawScroll(ImGui.GetContentRegionAvail().Y, Debug.Get(), DrawDebugScroll);
        Helper.DrawMenuBar(DrawMenuBar);
    }

    private void DrawDebugScroll(DebugEvent item)
    {
        Helper.DrawLeftRight(DrawLeft, DrawRight, 0, ImGui.CalcTextSize($"{item.time}").X + 10);

        void DrawLeft()
        {
            ImGui.TextColored(item.color, $"[{item.level}]: {item.message}");
            
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip($"{item.path}, {item.line}");
            }
        }
        void DrawRight()
        {
            ImGui.Text($"{item.time}");
        }
    }
    private void DrawMenuBar()
    {
        if (ImGui.Button("Clear"))
        {
            Debug.Clear();
        }

        Vector4 onColor = ImGui.GetStyle().Colors[(int)ImGuiCol.Button];
        Vector4 offColor = ImGui.GetStyle().Colors[(int)ImGuiCol.ButtonActive];
        
        ImGui.PushStyleColor(ImGuiCol.Button, showInfo ? onColor : offColor);
        if (ImGui.Button($"Info:{Debug.InfoCount}###Info")) // 这里###之后的是按钮的id, 因为Debug.InfoCount每帧都会变化, 不这样写点击之后关闭的和显示的id不同, 导致显示的无法关闭
        {
            showInfo = !showInfo;
            
            if (showInfo)
            {
                Debug.Level |= DebugLevel.Info;
            }
            else
            {
                Debug.Level &= ~DebugLevel.Info;
            }
        }
        ImGui.PopStyleColor(1);
        
        ImGui.PushStyleColor(ImGuiCol.Button, showWarning ? onColor : offColor);
        if (ImGui.Button($"Warning:{Debug.WarningCount}###Warning"))
        {
            showWarning = !showWarning;
            
            if (showWarning)
            {
                Debug.Level |= DebugLevel.Warning;
            }
            else
            {
                Debug.Level &= ~DebugLevel.Warning;
            }
        }
        ImGui.PopStyleColor(1);
        
        ImGui.PushStyleColor(ImGuiCol.Button, showError ? onColor : offColor);
        if (ImGui.Button($"Error:{Debug.ErrorCount}###Error"))
        {
            showError = !showError;
            
            if (showError)
            {
                Debug.Level |= DebugLevel.Error;
            }
            else
            {
                Debug.Level &= ~DebugLevel.Error;
            }
        }
        ImGui.PopStyleColor(1);
    }
    
    private bool showInfo = true, showWarning = true, showError = true;
}