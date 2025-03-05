using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SphereCollider))]
public class DoorController : NetworkBehaviour, IActivatable
{
    [SerializeField] private bool _isTriggerable = true;    // 근처에 가면 열릴지 여부

    private Animator _animator;
    
    /// <summary>
    /// 문이 열리기 위해선 Host에서 IsOpened가 true 상태이어야 함.
    /// </summary>
    [field: SerializeField]
    public bool IsOpened { get; set; } = false;
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    /// <summary>
    /// IsOpened = true라면 문을 엽니다.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void OpenDoorServerRpc()
    {
        OpenDoorClientRpc();
    }

    [ClientRpc]
    private void OpenDoorClientRpc()
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
        CloseDoorClientRpc();
    }

    [ClientRpc]
    public void CloseDoorClientRpc()
    {
        _animator.SetBool("Open", false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isTriggerable)
        {
            OpenDoorServerRpc();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_isTriggerable)
        {
            CloseDoorServerRpc();
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
    public void Initialize(bool isTriggerable, bool isOpen)
    {
        InitializeClientRpc(isTriggerable, isOpen);

        if (isOpen)
        {
            OpenDoorServerRpc();
        }
    }
}
