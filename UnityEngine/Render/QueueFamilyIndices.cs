namespace UnityEngine
{
    public class QueueFamilyIndices
    {
        public uint? graphicsQueue;
        public uint? presentQueue;

        public bool HasValue()
        {
            return graphicsQueue.HasValue && presentQueue.HasValue;
        }
    }
}