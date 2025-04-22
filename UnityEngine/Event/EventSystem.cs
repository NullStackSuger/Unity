namespace UnityEngine
{
    public static class EventSystem
    {
        private static readonly Dictionary<Type, List<IEvent>> allEvents = new Dictionary<Type, List<IEvent>>();

        // TODO 之后需要添加特性去自动注册
        public static void Add(IEvent e)
        {
            allEvents.TryAdd(e.Type, new List<IEvent>());
            allEvents[e.Type].Add(e);
        }

        public static async void PublishAsync<A>(A a) where A : struct
        {
            if (!allEvents.TryGetValue(typeof(A), out List<IEvent> iEvents))
            {
                return;
            }

            foreach (IEvent iEvent in iEvents)
            {
                if (!(iEvent is AEvent<A> aEvent))
                {
                    Debug.Error($"event error: {iEvent.GetType().FullName}");
                    continue;
                }

                await aEvent.Handle(a);
            }
        }
    }
}