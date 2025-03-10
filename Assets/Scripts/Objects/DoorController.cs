using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SphereCollider))]
public class DoorController : NetworkBehaviour, IActivatable
{
    /// <summary>
    /// 근처에 가면 열릴지 여부
    /// </summary>
    [SerializeField] private bool _isTriggerable = true;

    /// <summary>
    /// 호출되면 문을 열 이벤트
    /// </summary>
    [SerializeField] private EventType[] _subscribeForActivation;

    /// <summary>
    /// 호출되면 문을 닫을 이벤트
    /// </summary>
    [SerializeField] private EventType[] _subscribeForDeactivation;

    private Animator _animator;
    private float _playerCount = 0;

    /// <summary>
    /// 문이 열리기 위해선 Host에서 IsOpened가 true 상태이어야 함.
    /// </summary>
    [field: SerializeField]
    public bool IsOpened { get; set; } = false;
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private new void OnDestroy()
    {
        foreach (EventType eventType in _subscribeForActivation)
        {
            EventBus.Instance.UnsubscribeEvent<UnityAction>(eventType, OpenDoorServerRpc);
        }

        foreach (EventType eventType in _subscribeForDeactivation)
        {
            EventBus.Instance.UnsubscribeEvent<UnityAction>(eventType, CloseDoorServerRpc);
        }

        base.OnDestroy();
    }

    /// <summary>
    /// IsOpened = true라면 문을 엽니다.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void OpenDoorServerRpc()
    {
        if (IsOpened)
        {
            _animator.SetBool("Open", true);
        }
    }
    
    /// <summary>
    /// 문을 닫습니다.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void CloseDoorServerRpc()
    {
        _animator.SetBool("Open", false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            _playerCount++;

            if (_isTriggerable)
            {
                OpenDoorServerRpc();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            _playerCount--;

            if (_isTriggerable && _playerCount == 0)
            {
                CloseDoorServerRpc();
            }
        }
    }

    public bool IsActivatable(GameObject activator = null)
    {
        return true;
    }

    public bool Activate(GameObject activator = null)
    {
        OpenDoorServerRpc();

        return true;
    }

    public bool Deactivate(GameObject activator = null)
    {
        CloseDoorServerRpc();

        return true;
    }

    /// <summary>
    /// 서버와 클라이언트의 초기 상태를 동기화한다. 이 함수는 서버와 클라이언트 모두에서 호출된다.
    /// </summary>
    /// <param name="isTriggerable">주변에 가면 켜질지 여부</param>
    /// <param name="isOpen">열린 상태 여부</param>
    [ClientRpc]
    private void InitializeClientRpc(bool isTriggerable, bool isOpen)
    {
        _isTriggerable = isTriggerable;
        IsOpened = isOpen;
    }

    /// <summary>
    /// 문 상태를 초기화하고 클라이언트와 동기화한다. 이 함수는 서버에서만 호출한다.
    /// </summary>
    /// <param name="isTriggerable">주변에 가면 켜질지 여부</param>
    /// <param name="isOpen">열린 상태 여부</param>
    public void Initialize(bool isTriggerable, bool isOpen, EventType[] subscribeForActivation, EventType[] subscribeForDeactivation)
    {
        InitializeClientRpc(isTriggerable, isOpen);

        _subscribeForActivation = subscribeForActivation;
        _subscribeForDeactivation = subscribeForDeactivation;

        foreach (EventType eventType in _subscribeForActivation)
        {
            EventBus.Instance.SubscribeEvent<UnityAction>(eventType, OpenDoorServerRpc);
        }

        foreach (EventType eventType in _subscribeForDeactivation)
        {
            EventBus.Instance.SubscribeEvent<UnityAction>(eventType, CloseDoorServerRpc);
        }
    }
}
