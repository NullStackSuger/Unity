namespace UnityEngine;

public class TreeNode<T>
{
    public T value;
    public List<TreeNode<T>> children;
    
    public TreeNode(T t)
    {
        this.value = t;
    }
}