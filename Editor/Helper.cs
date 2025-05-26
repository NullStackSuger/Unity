using System.Reflection;

namespace ET;

internal static class Helper
{
    internal static void SetProperty(object obj, string propertyName, object value)
    {
        Type type = obj.GetType();
        while (type != null)
        {
            PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (property != null && property.CanWrite)
            {
                property.SetValue(obj, value);
                return;
            }
            type = type.BaseType;
        }
        Log.Error($"Property {propertyName} not found on {obj.GetType()}");
    }
}