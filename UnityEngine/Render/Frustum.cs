using System.Numerics;

namespace UnityEngine;

public readonly struct Frustum
{
    public readonly Plane Left;
    public readonly Plane Right;
    public readonly Plane Top;
    public readonly Plane Bottom;
    public readonly Plane Near;
    public readonly Plane Far;
    
    public IEnumerable<Plane> Planes => new[] { Left, Right , Top, Bottom, Near, Far };

    public Frustum(Matrix4x4 vp)
    {
        // 提取六个平面， plane: Ax + By + Cz + D = 0
        this.Left   = new Plane(vp.M14 + vp.M11, vp.M24 + vp.M21, vp.M34 + vp.M31, vp.M44 + vp.M41);
        this.Right  = new Plane(vp.M14 - vp.M11, vp.M24 - vp.M21, vp.M34 - vp.M31, vp.M44 - vp.M41);
        this.Top    = new Plane(vp.M14 - vp.M12, vp.M24 - vp.M22, vp.M34 - vp.M32, vp.M44 - vp.M42);
        this.Bottom = new Plane(vp.M14 + vp.M12, vp.M24 + vp.M22, vp.M34 + vp.M32, vp.M44 + vp.M42);
        this.Near   = new Plane(vp.M13, vp.M23, vp.M33, vp.M43);
        this.Far    = new Plane(vp.M14 - vp.M13, vp.M24 - vp.M23, vp.M34 - vp.M33, vp.M44 - vp.M43);
        
        // 归一化每个平面（单位法线）
        this.Left.Normalize();
        this.Right.Normalize();
        this.Top.Normalize();
        this.Bottom.Normalize();
        this.Near.Normalize();
        this.Far.Normalize();
    }
    
    // 判断包围盒和视锥体相交
    public bool Intersects(AABB bounds)
    {
        foreach (var plane in Planes)
        {
            if (!bounds.Intersects(plane))
            {
                return false;
            }
        }
        return true;
    }
    // 判断某个点是否在包围盒内
    public bool Contains(Vector3 point)
    {
        foreach (var plane in Planes)
        {
            if (plane.GetDistance(point) < 0)
            {
                // 点在任意一个平面的背面 => 点在视锥外
                return false;
            }
        }
        return true; // 所有平面前侧 => 点在视锥内
    }

    public override string ToString()
    {
        return $"{Left}, {Right}, {Top}, {Bottom}, {Near}, {Far}";
    }
}