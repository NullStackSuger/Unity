using System.Runtime.InteropServices;
using Sandbox.Test_3;
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
            /*Window window = WindowSystem.Create(Width, Height, Title);
            RenderSystem renderSystem = new RenderSystem(window);
            while (WindowSystem.Tick(_ =>
                   {
                       renderSystem.Render(); 
                   })) { }*/
            Debug.Log($"Test_1: {Marshal.SizeOf<MyTest_1>()}");
            Debug.Log($"Test_2: {Marshal.SizeOf<MyTest_2>()}");
            
            Debug.Log($"Test_3: {Marshal.SizeOf<MyTest_3>()}");
            Debug.Log($"Test_4: {Marshal.SizeOf<MyTest_4>()}"); // 3和4说明同一文件中, 分布类字段是按声明顺序拼接的
            Debug.Log($"Test_5: {Marshal.SizeOf<MyTest_5>()}"); // 虽然b和b1在2个分布类中, 但还是会合并
            
            Debug.Log($"Test_6 {Marshal.SizeOf<MyStruct>()}");
            Debug.Log($"Test_7 {Marshal.SizeOf<MyStruct_1>()}"); // 说明和文件夹中排序有关?
            Debug.Log($"Test_8 {Marshal.SizeOf<MyStruct_2>()}"); // MyStruct_2是先创建的MyTest_8, 说明和文件创建顺序无关
        }

        private struct MyTest_1 // 12
        {
            public byte b; // 1 -> 4
            public int i; // 4
            public byte b1; // 1 -> 4
        }

        private struct MyTest_2 // 8 
        {
            public int i; // 4
            public byte b; // 1
            public byte b1; // 1
        }

        private partial struct MyTest_3
        {
            public int i;
        }
        private partial struct MyTest_3
        {
            public byte b;
        }
        private partial struct MyTest_3
        {
            public byte b1;
        }

        private partial struct MyTest_4
        {
            public byte b;
        }
        private partial struct MyTest_4
        {
            public int i;
        }
        private partial struct MyTest_4
        {
            public byte b1;
        }

        private partial struct MyTest_5
        {
            public int i;
        }
        private partial struct MyTest_5
        {
            public byte b;
        }
        private partial struct MyTest_5
        {
            public byte b1;
        }
    }
}