namespace UnityEngine;

public class TreeNode<T>
{
    private readonly T value;
    public T Value => value;
    
    private readonly List<TreeNode<T>> children = new();
    public IReadOnlyList<TreeNode<T>> Children => children;
    public TreeNode<T> this[int index] => children[index];
    
    private TreeNode<T> parent;
    public TreeNode<T> Parent => parent;
    
    private TreeNode(T t)
    {
        this.value = t;
    }

    public static TreeNode<T> Root(T t)
    {
        return new TreeNode<T>(t);
    }

    public TreeNode<T> AddChild(T t)
    {
        return AddChild(new TreeNode<T>(t));
    }

    public TreeNode<T> AddChild(TreeNode<T> child)
    {
        child.parent = this;
        this.children.Add(child);
        return child;
    }

    public void RemoveChild(TreeNode<T> child)
    {
        child.parent = null;
        this.children.Remove(child);
    }

    public void Clear()
    {
        for (int i = children.Count - 1; i >= 0; i--)
        {
            var child = children[i];
            RemoveChild(child);
        }
        
        if (children.Count > 0) Debug.Error("Tree is not empty");
    }

    public static implicit operator T(TreeNode<T> node)
    {
        return node.value;
    }
}