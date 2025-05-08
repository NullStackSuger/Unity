using System.Numerics;
using System.Runtime.InteropServices;

namespace UnityEngine;

[StructLayout(LayoutKind.Sequential)]
public struct LightVPUniform
{
    public Matrix4x4 vp;
}