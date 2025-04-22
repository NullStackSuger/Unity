using SharpVk.Glfw;
using UnityEngine.Events;

namespace UnityEngine
{
    public class Window
    {
        public string Title { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        private readonly WindowHandle window;

        public Window(int width, int height, string title = "")
        {
            Width = width;
            Height = height;
            Title = title;
            window = Glfw3.CreateWindow(width, height, title, default, default);

            ////////////////////////////////////////////////////////////////////////////////
            // Events
            ////////////////////////////////////////////////////////////////////////////////
            
            // 用户输入文本
            //Glfw3.SetCharCallback();

            Glfw3.SetKeyCallback(window, (handle, key, code, action, modifiers) =>
            {
                switch (action)
                {
                    case InputAction.Press:
                        EventSystem.PublishAsync(new Window_KeyPress_Event(){ window = handle, key = key });
                        break;
                    case InputAction.Release:
                        EventSystem.PublishAsync(new Window_KeyRelease_Event(){ window = handle, key = key });
                        break;
                    case InputAction.Repeat:
                        EventSystem.PublishAsync(new Window_KeyRepeat_Event(){ window = handle, key = key });
                        break;
                }
            });

            Glfw3.SetWindowSizeCallback(window, (handle, curWidth, curHeight) =>
            {
                int preWidth = Width;
                int preHeight = Height;
                Width = curWidth;
                Height = curHeight;
                EventSystem.PublishAsync(new Window_Resize_Event() { window = handle, preWidth = preWidth, preHeight = preHeight, curWidth = curWidth, curHeight = curHeight });
            });

            Glfw3.SetMouseButtonPosCallback(window, (handle, button, action, mods) =>
            {
                switch (action)
                {
                    case InputAction.Press:
                        EventSystem.PublishAsync(new Window_MousePress_Event() { window = handle, button = button });
                        break;
                    case InputAction.Release:
                        EventSystem.PublishAsync(new Window_MouseRelease_Event() { window = handle, button = button });
                        break;
                    case InputAction.Repeat:
                        EventSystem.PublishAsync(new Window_MouseRepeat_Event() { window = handle, button = button });
                        break;
                }
            });
        }

        public bool Tick()
        {
            bool result = !Glfw3.WindowShouldClose(window);
            if (result)
            {
                Glfw3.PollEvents();
            }
            return result;
        }
    }
}