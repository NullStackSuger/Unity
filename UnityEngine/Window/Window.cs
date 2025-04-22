using SharpVk.Glfw;
using UnityEngine.Events;

namespace UnityEngine
{
    public class Window
    {
        public string Title { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        private float mouseX, mouseY;
        
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

            Glfw3.SetKeyCallback(window, (_, key, _, action, _) =>
            {
                switch (action)
                {
                    case InputAction.Press:
                        EventSystem.PublishAsync(new Window_KeyPress_Event(){ window = this, key = key });
                        break;
                    case InputAction.Release:
                        EventSystem.PublishAsync(new Window_KeyRelease_Event(){ window = this, key = key });
                        break;
                    case InputAction.Repeat:
                        EventSystem.PublishAsync(new Window_KeyRepeat_Event(){ window = this, key = key });
                        break;
                }
            });

            Glfw3.SetWindowSizeCallback(window, (_, curWidth, curHeight) =>
            {
                int preWidth = Width;
                int preHeight = Height;
                Width = curWidth;
                Height = curHeight;
                EventSystem.PublishAsync(new Window_Resize_Event() { window = this, preWidth = preWidth, preHeight = preHeight, curWidth = curWidth, curHeight = curHeight });
            });

            Glfw3.SetMouseButtonPosCallback(window, (_, button, action, _) =>
            {
                switch (action)
                {
                    case InputAction.Press:
                        EventSystem.PublishAsync(new Window_MousePress_Event() { window = this, button = button });
                        break;
                    case InputAction.Release:
                        EventSystem.PublishAsync(new Window_MouseRelease_Event() { window = this, button = button });
                        break;
                    case InputAction.Repeat:
                        EventSystem.PublishAsync(new Window_MouseRepeat_Event() { window = this, button = button });
                        break;
                }
            });

            Glfw3.SetScrollCallback(window, (_, xOffset, yOffset) =>
            {
                EventSystem.PublishAsync(new Window_MouseScroll_Event() { window = this, xOffset = (float)xOffset, yOffset = (float)yOffset });
            });

            Glfw3.SetCursorPosCallback(window, (_, curMouseX, curMouseY) =>
            {
                float xOffset = (float)curMouseX - mouseX;
                float yOffset = (float)curMouseY - mouseY;
                mouseX = (float)curMouseX;
                mouseY = (float)curMouseY;
                EventSystem.PublishAsync(new Window_MouseMove_Event(){ window = this, xOffset = xOffset, yOffset = yOffset });
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