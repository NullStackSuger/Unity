using System.Numerics;
using System.Runtime.InteropServices;

namespace UnityEngine;

[StructLayout(LayoutKind.Sequential)]
public struct MVPUniform
{
    public Matrix4x4 model;
    public Matrix4x4 view;
    public Matrix4x4 projection;
}