using Veldrid;

namespace UnityEngine;

/// <summary>
/// 这个和Shader中的Pass概念不同
/// 这是控制整个RenderSystem的渲染流程, 而不应该针对某一个(类)Mesh
/// </summary>
public abstract class RenderPass
{
    public abstract void Tick(CommandList commandList);
}