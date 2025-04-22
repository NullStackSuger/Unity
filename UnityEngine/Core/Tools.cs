using System.Text;

namespace UnityEngine
{
    public static partial class Tools
    {
        public static byte[] ReadFileBytes(string path)
        {
            return File.ReadAllBytes(path);
        }

        public static uint[] ReadFileUints(string path)
        {
            byte[] fileBytes = ReadFileBytes(path);

            // 将字节数组转换为 uint 数组，每 4 个字节构成一个 uint
            uint[] uintArray = new uint[fileBytes.Length / 4];
            for (int i = 0; i < uintArray.Length; i++)
            {
                uintArray[i] = BitConverter.ToUInt32(fileBytes, i * 4);
            }

            return uintArray;
        }

        public static string ReadFileString(string path)
        {
            byte[] fileBytes = ReadFileBytes(path);
            return Encoding.UTF8.GetString(fileBytes);
        }
    }
}