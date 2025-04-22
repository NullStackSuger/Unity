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
            WindowSystem.Create(Width, Height, Title);
            while (WindowSystem.Tick())
            {
                
            }
        }
    }
}