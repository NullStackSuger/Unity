using System.Numerics;
using Veldrid;

namespace UnityEngine;

public sealed class PrepareRenderPass : RenderPass
{
    public PrepareRenderPass()
    {
        Tick(null);
    }
    
    public override void Tick(CommandList commandList)
    {
        objects.Clear();
        
        Scene scene = Scene.ActiveScene;
        Camera camera = Camera.Main;
        
        // 剔除相机之外的Obj, 并按里相机远近排序
        GameObject sceneObj = scene;
        GameObject cameraObj = camera;
        
        // 视锥体剔除
        // TODO 这里暂时不剔除
        foreach (GameObject obj in sceneObj.Find())
        {
            if (!obj.TryGetComponent(out MeshComponent meshComponent)) continue;
            
            objects.Add((obj, meshComponent));
            
            // 世界坐标系的包围盒
            AABB aabb = meshComponent.AABB.Transform(obj.transform.Model);
        }
        
        // 按距离摄像机的远近排序
        Vector3 camPos = cameraObj.transform.position;
        objects.Sort((a, b) =>
        {
            float da = Vector3.DistanceSquared(camPos, a.Item1.transform.position);
            float db = Vector3.DistanceSquared(camPos, b.Item1.transform.position);
            return da.CompareTo(db); // 近 -> 远
        });
    }

    private readonly List<(GameObject, MeshComponent)> objects = new();
    public IReadOnlyList<(GameObject, MeshComponent)> Objects => objects;
}