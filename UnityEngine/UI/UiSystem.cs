using System.Numerics;
using ImGuiNET;

namespace UnityEngine;

public class UiSystem
{
    public UiSystem(Window window)
    {
        ImGui.CreateContext();
        var io = ImGui.GetIO();
        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard | ImGuiConfigFlags.DockingEnable;
        io.Fonts.Flags |= ImFontAtlasFlags.NoBakedLines;
        io.Fonts.AddFontDefault();
        io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int bpp);
        io.Fonts.SetTexID(new IntPtr(1));
        io.Fonts.ClearTexData();
        io.DisplaySize = new Vector2(window.Width, window.Height);
        io.DisplayFramebufferScale = Vector2.One;
    }

    public void Tick()
    {
        ImGui.NewFrame();
        ImGui.Render();
        ImGui.EndFrame();
    }

    ~UiSystem()
    {
        ImGui.DestroyContext();
    }
}