namespace ET;

public class EventSystem: Singleton<EventSystem>, ISingletonAwake
{
    private class EventInfo
    {
        public IEvent IEvent { get; }
            
        public EventInfo(IEvent iEvent)
        {
            this.IEvent = iEvent;
        }
    }
        
    private readonly Dictionary<Type, List<EventInfo>> allEvents = new();
        
    public void Awake()
    {
        foreach (Type type in CodeTypes.Instance.GetTypes(typeof(EventAttribute)))
        {
            this.Register(type);
        }
    }

    public void Register(Type type)
    {
        IEvent obj = Activator.CreateInstance(type) as IEvent;
        if (obj == null) Log.Error($"type not is AEvent: {type.Name}");
        
        Type eventType = obj!.Type;
        EventInfo eventInfo = new(obj);
        this.allEvents.TryAdd(eventType, []);
        this.allEvents[eventType].Add(eventInfo);
    }
        
    public async ETTask PublishAsync<A>(A a) where A : struct
    {
        if (!this.allEvents.TryGetValue(typeof(A), out var iEvents))
        {
            return;
        }

        List<ETTask> tasks = new();
        foreach (EventInfo eventInfo in iEvents)
        {
            if (!(eventInfo.IEvent is AEvent<A> aEvent))
            {
                Log.Error($"event error: {eventInfo.IEvent.GetType().FullName}");
                continue;
            }

            tasks.Add(aEvent.Handle(a));
        }

        try
        {
            await ETTaskHelper.WaitAll(tasks);
        }
        catch (Exception e)
        {
            Log.Error(e);
        }
    }

    public void Publish<A>(A a) where A : struct
    {
        if (!this.allEvents.TryGetValue(typeof (A), out var iEvents))
        {
            return;
        }
            
        foreach (EventInfo eventInfo in iEvents)
        {
            if (!(eventInfo.IEvent is AEvent<A> aEvent))
            {
                Log.Error($"event error: {eventInfo.IEvent.GetType().FullName}");
                continue;
            }
                
            aEvent.Handle(a).Coroutine();
        }
    }
}