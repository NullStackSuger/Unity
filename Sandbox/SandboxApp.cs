using UnityEngine;

namespace Sandbox
{
    public static class SandboxApp
    {
        private const int Width = 1920;
        private const int Height = 1080;
        private const string Title = "Sandbox";
        
        public static void Main(string[] args)
        {
            Window window = WindowSystem.Create(Width, Height, Title);
            RenderSystem renderSystem = new RenderSystem(window);
            
            while (WindowSystem.Tick(Tick)) { }

            void Tick(Window _)
            {
                renderSystem.Tick();
            }
        }
    }
}