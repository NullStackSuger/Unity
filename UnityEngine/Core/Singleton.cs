namespace UnityEngine
{
    public abstract class ASingleton : IDisposable
    {
        public abstract void Dispose();
    }

    public abstract class Singleton<T> : IDisposable where T : Singleton<T>, new()
    {
        private static T instance;

        public static T Instance => instance ??= new T();

        public virtual void Dispose()
        {
            instance = null;
        }
    }
}