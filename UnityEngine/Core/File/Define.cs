namespace UnityEngine;

public static partial class Define
{
    public static string[] LikeFileTypes = [".obj", ".spv", ".cs"];
    public static string[] UnlikeFileTypes = [];

    /// <summary>
    /// 种类, 名字, 类型
    /// </summary>
    public static Dictionary<string, Dictionary<string, Type>> TypeMap = new()
    {
        ["Shader"] = new Dictionary<string, Type>()
        {
            [nameof(DefaultShadowShader)] = typeof(DefaultShadowShader),
            [nameof(DefaultObjectShader)] = typeof(DefaultObjectShader),
        }
    };
}