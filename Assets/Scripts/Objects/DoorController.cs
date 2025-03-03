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
    [SerializeField]
    private bool _isTriggerable = true;

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

    [ClientRpc]
    private void InitializeClientRpc(bool isTriggerable)
    {
        _isTriggerable = isTriggerable;
    }

    public void Initialize(bool isTriggerable)
    {
        InitializeClientRpc(isTriggerable);
    }
}
