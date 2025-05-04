namespace UnityEngine;

public class TreeNode<T>
{
    public T value;
    public List<TreeNode<T>> children = new();
    public TreeNode<T> parent;
    
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

    public static implicit operator T(TreeNode<T> node)
    {
        return node.value;
    }
}