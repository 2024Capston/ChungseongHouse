using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 트리거 영역을 조작하는 Class
/// </summary>
public class TriggerController : MonoBehaviour
{
    [SerializeField] private EventType[] _publishOnEnter;   // 물체가 영역에 들어오면 호출할 이벤트
    [SerializeField] private EventType[] _publishOnExit;    // 물체가 영역에서 벗어나면 호출할 이벤트

    private void OnTriggerEnter(Collider other)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            foreach(EventType eventType in _publishOnEnter)
            {
                EventBus.Instance.InvokeEvent(eventType, other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            foreach (EventType eventType in _publishOnExit)
            {
                EventBus.Instance.InvokeEvent(eventType, other.gameObject);
            }
        }
    }
}
