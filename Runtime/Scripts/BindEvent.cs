using UnityEngine;
using UnityEngine.Events;

public class BindEvent : MonoBehaviour
{
    public string EventName;
    public UnityEvent Callback;

    private void Start()
    {
        EventBus.Register(EventName, ()  => Callback.Invoke());
    }
}
