using System.Numerics;

namespace UnityEngine
{
    public struct Color
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public Color(float value = 1.0f)
        {
            r = g = b = a = 1.0f;
        }

        public Color(float r, float g, float b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = 1.0f;
        }

        public Color(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public static implicit operator Vector4(Color color)
        {
            return new Vector4(color.r, color.g, color.b, color.a);
        }

        public static implicit operator Vector3(Color color)
        {
            return new Vector3(color.r, color.g, color.b);
        }

        public static Color Black => new Color(0.0f, 0.0f, 0.0f);
        public static Color White => new Color(1.0f, 1.0f, 1.0f);
        public static Color Gray => new Color(0.5f, 0.5f, 0.5f);
        public static Color Red => new Color(1.0f, 0.0f, 0.0f);
        public static Color Green => new Color(0.0f, 1.0f, 0.0f);
        public static Color Blue => new Color(0.0f, 0.0f, 1.0f);
    }
}