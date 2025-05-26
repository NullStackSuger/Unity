namespace ET;

/// <summary>
/// 标记需要跟踪的类, 类似UnityECS中获取某类Component
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class BaseAttribute: Attribute
{
}