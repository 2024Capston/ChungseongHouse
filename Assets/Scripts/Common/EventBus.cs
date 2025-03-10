using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 이벤트 버스를 관리하는 Class
/// </summary>
public class EventBus : SingletonBehavior<EventBus>
{
    private Dictionary<EventType, UnityEventBase> _events;
    private Dictionary<EventType, int> _counts;             // 각 이벤트에 등록된 Listener 수를 저장

    // TEST
    [SerializeField] private EventType[] T_events;
    [SerializeField] private int[] T_counts;

    protected override void Init()
    {
        base.Init();

        _events = new Dictionary<EventType, UnityEventBase>();
        _counts = new Dictionary<EventType, int>();
    }

    private void Update()
    {
        // TEST
        List<EventType> t_events = new List<EventType>();
        List<int> t_counts = new List<int>();

        foreach(var i in _counts)
        {
            t_events.Add(i.Key);
            t_counts.Add(i.Value);
        }

        T_events = t_events.ToArray();
        T_counts = t_counts.ToArray();
    }

    /// <summary>
    /// UnityAction 자료형을 UnityEvent 자료형으로 변환한다.
    /// </summary>
    /// <param name="unityAction">UnityAction</param>
    /// <returns>UnityEvent</returns>
    private Type GetUnityEventType(Delegate unityAction)
    {
        Type[] parameters = unityAction.GetMethodInfo().GetParameters().Select(p => p.ParameterType).ToArray();

        switch (parameters.Length)
        {
            case 0:
                return typeof(UnityEvent);
            case 1:
                return typeof(UnityEvent<>).MakeGenericType(parameters);
            case 2:
                return typeof(UnityEvent<,>).MakeGenericType(parameters);
            case 3:
                return typeof(UnityEvent<,,>).MakeGenericType(parameters);
            case 4:
                return typeof(UnityEvent<,,,>).MakeGenericType(parameters);
            default:
                Logger.Log($"There are two many parameters for {unityAction}");
                return null;
        }
    }

    /// <summary>
    /// 함수를 이벤트에 등록한다.
    /// </summary>
    /// <typeparam name="T">함수의 자료형 (UnityAction 형식)</typeparam>
    /// <param name="eventType">이벤트 유형</param>
    /// <param name="unityAction">등록할 함수</param>
    public void SubscribeEvent<T>(EventType eventType, T unityAction) where T : Delegate
    {
        Type unityEventType = GetUnityEventType(unityAction);

        if (!_events.ContainsKey(eventType))
        {
            _events[eventType] = Activator.CreateInstance(unityEventType) as UnityEventBase;
            _counts[eventType] = 0;
        }
        else if (_events[eventType].GetType() != unityEventType)
        {
            Logger.Log($"Trying to subscribe {unityEventType} into {_events[eventType].GetType()}");
            return;
        }

        _counts[eventType]++;

        MethodInfo addListenerMethod = unityEventType.GetMethod("AddListener");
        addListenerMethod.Invoke(_events[eventType], new object[] { unityAction });
    }

    /// <summary>
    /// 함수를 이벤트에서 등록 해제한다.
    /// </summary>
    /// <typeparam name="T">함수의 자료형 (UnityAction 형식)</typeparam>
    /// <param name="eventType">이벤트 유형</param>
    /// <param name="unityAction">등록 해제할 함수</param>
    public void UnsubscribeEvent<T>(EventType eventType, T unityAction) where T : Delegate
    {
        Type unityEventType = GetUnityEventType(unityAction);

        if (!_events.ContainsKey(eventType))
        {
            Logger.Log($"There is no event of {eventType}");
            return;
        }

        if (_events[eventType].GetType() != unityEventType)
        {
            Logger.Log($"Trying to unsubscribe {unityEventType} from {_events[eventType].GetType()}");
            return;
        }

        MethodInfo removeListenerMethod = unityEventType.GetMethod("RemoveListener");
        removeListenerMethod.Invoke(_events[eventType], new object[] { unityAction });

        _counts[eventType]--;

        if (_counts[eventType] == 0)
        {
            _events.Remove(eventType);
            _counts.Remove(eventType);
        }
    }

    /// <summary>
    /// 이벤트를 호출한다.
    /// </summary>
    /// <param name="eventType">이벤트 유형</param>
    /// <param name="parameters">매개 변수</param>
    public void InvokeEvent(EventType eventType, params object[] parameters)
    {
        if (_events.ContainsKey(eventType))
        {
            MethodInfo invokeMethod = _events[eventType].GetType().GetMethod("Invoke");
            invokeMethod.Invoke(_events[eventType], parameters);
        }
    }
}
