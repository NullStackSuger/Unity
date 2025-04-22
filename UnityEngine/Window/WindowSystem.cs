using SharpVk.Glfw;

namespace UnityEngine
{

    public static class WindowSystem
    {
        private static readonly List<Window> windows = new List<Window>();

        static WindowSystem()
        {
            // TODO 注意要把glfw3.dll复制到bin去, 之后要写个自动复制的, 是复制到SanaBox下面
            Glfw3.Init();
            Glfw3.WindowHint(WindowAttribute.ClientApi, 0);
        }
        
        public static bool Tick()
        {
            if (windows.Count == 0) return false;
            for (int i = windows.Count - 1; i >= 0; i--)
            {
                var window = windows[i];
                if (!window.Tick())
                {
                    Destroy(window);
                }
            }

            return true;
        }

        public static void Create(Window window)
        {
            windows.Add(window);
        }

        public static Window Create(int width, int height, string title = "")
        {
            Window window = new Window(width, height, title);
            Create(window);
            return window;
        }

        public static void Destroy(Window window)
        {
            windows.Remove(window);
        }

        public static void Destroy()
        {
            windows.RemoveAt(windows.Count - 1);
        }

        public static Window Peek()
        {
            Debug.Assert(windows.Count <= 0, "Windows Count must more than 0");
            return windows.Last();
        }

        public static Window? Find(string title)
        {
            return windows.Find(window => window.Title == title);
        }
    }
}