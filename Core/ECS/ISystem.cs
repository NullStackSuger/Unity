namespace ET;

public interface ISystem
{
    Type Type();
    Type SystemType();
    int GetInstanceQueueIndex();
}