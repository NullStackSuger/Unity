namespace ET.Event;

public struct OnCollisionTestBegin
{
    public GameObject obj;
}

public struct OnCollisionTestEnd
{
    public GameObject obj;
}

public struct OnCollisionEnter
{
    public GameObject a, b;
}

public struct OnCollisionStay
{
    public GameObject a, b;
}

public struct OnCollisionExit
{
    public GameObject a, b;
}