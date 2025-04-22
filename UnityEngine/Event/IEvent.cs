namespace UnityEngine
{
    public interface IEvent
    {
        Type Type { get; }
    }

    public abstract class AEvent<A> : IEvent where A : struct
    {
        public Type Type => typeof(A);

        public async Task Handle(A a)
        {
            try
            {
                await Run(a);
            }
            catch (Exception e)
            {
                Debug.Error(e);
            }
        }

        protected abstract Task Run(A a);
    }
}