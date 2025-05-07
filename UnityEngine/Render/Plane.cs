using System.Numerics;

namespace UnityEngine;

public struct Panel
{
    public Vector3 Normal;
    public float Distance;

    public Panel()
    {
        
    }
    
    public Plane(Vector3 normal, float distance)
    {
        Normal = normal;
        Distance = distance;
    }
    
    public Plane(float a, float b, float c, float d)
    {
        Normal = new Vector3(a, b, c);
        Distance = d;
    }
}