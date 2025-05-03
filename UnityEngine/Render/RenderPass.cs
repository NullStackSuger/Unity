using Veldrid;

namespace UnityEngine;

public abstract class RenderPass
{
    public abstract void Tick(CommandList commandList);
}