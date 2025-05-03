using System.Numerics;
using Veldrid;

namespace UnityEngine;

public struct Vertex
{
    public Vector3 position;
    public Vector3 color;
    public Vector2 uv;

    public static VertexLayoutDescription GetLayout()
    {
        return new VertexLayoutDescription
        (
            new VertexElementDescription("position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("uv", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
        );
    }
}