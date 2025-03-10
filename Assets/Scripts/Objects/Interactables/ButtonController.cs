using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Rendering;
using System;
using UnityEngine.Events;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

/// <summary>
/// 버튼을 조작하는 Abstract Class
/// </summary>
abstract public class ButtonController : NetworkBehaviour, IInteractable
{
    /// <summary>
    /// 버튼 색깔
    /// </summary>
    [SerializeField] protected ColorType _color;
    public ColorType Color
    {
        get => _color;
    }

    /// <summary>
    /// 버튼이 활성화됐을 때 Activate될 IInteractable 오브젝트
    /// </summary>
    [SerializeField] protected GameObject[] _activatables;

    protected ButtonRenderer _buttonRenderer;

    /// <summary>
    /// 버튼이 눌려서 활성화됐는지 여부
    /// </summary>
    protected bool _isPressed = false;
    public bool isPressed
    {
        get => _isPressed;
    }

    /// <summary>
    /// 버튼을 사용 가능한지 여부
    /// </summary>
    protected bool _isEnabled = true;
    public bool isEnabled
    {
        get => _isEnabled;
    }

    private Outline _outline;
    public Outline Outline
    {
        get => _outline;
        set => _outline = value;
    }

    public override void OnNetworkSpawn()
    {
        _outline = GetComponent<Outline>();
        _buttonRenderer = GetComponent<ButtonRenderer>();
    }

    public bool IsInteractable(PlayerController player)
    {
        return OnInteractableCheck(player);
    }

    /// <summary>
    /// 현재 버튼과 플레이어가 상호작용할 수 있는지 확인한다.
    /// </summary>
    /// <param name="player">대상 플레이어</param>
    /// <returns>상호작용 가능 여부</returns>
    public abstract bool OnInteractableCheck(PlayerController player);

    public bool StartInteraction(PlayerController player)
    {
        return OnStartInteraction(player);
    }

    /// <summary>
    /// 플레이어가 버튼을 눌렀을 때 실행할 함수.
    /// </summary>
    /// <param name="player">대상 플레이어</param>
    /// <returns>일반적인 경우엔 false로 반환</returns>
    public abstract bool OnStartInteraction(PlayerController player);

    public bool StopInteraction(PlayerController player)
    {
        return false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void EnableButtonServerRpc(bool isEnabled)
    {
        EnableButtonClientRpc(isEnabled);
    }

    [ClientRpc]
    private void EnableButtonClientRpc(bool isEnabled)
    {
        _isEnabled = isEnabled;
    }

    [ServerRpc(RequireOwnership = false)]
    private void PressButtonServerRpc(bool isPressed)
    {
        PressButtonClientRpc(isPressed);
    }

    [ClientRpc]
    private void PressButtonClientRpc(bool isPressed)
    {
        _isPressed = isPressed;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ActivateObjectsServerRpc()
    {
        // IActivatable 활성화
        foreach (GameObject gameObject in _activatables)
        {
            if (gameObject.TryGetComponent<IActivatable>(out IActivatable activatable))
            {
                activatable.Activate(gameObject);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeactivateObjectsServerRpc()
    {
        // IActivatable 비활성화
        foreach (GameObject gameObject in _activatables)
        {
            if (gameObject.TryGetComponent<IActivatable>(out IActivatable activatable))
            {
                activatable.Deactivate(gameObject);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetButtonColorServerRpc(ColorType newColor)
    {
        SetButtonColorClientRpc(newColor);
    }

    [ClientRpc]
    private void SetButtonColorClientRpc(ColorType newColor)
    {
        _color = newColor;
        _buttonRenderer.SetButtonColor(newColor);
    }

    [ServerRpc]
    private void SetAnimatorPressStateServerRpc(bool isPressed)
    {
        SetAnimatorPressStateClientRpc(isPressed);
    }

    [ClientRpc]
    private void SetAnimatorPressStateClientRpc(bool isPressed)
    {
        _buttonRenderer.SetAnimatorPressState(isPressed);
    }

    [ServerRpc]
    private void SetAnimatorToggleTriggerServerRpc()
    {
        SetAnimatorToggleTriggerClientRpc();
    }

    [ClientRpc]
    private void SetAnimatorToggleTriggerClientRpc()
    {
        _buttonRenderer.SetAnimatorToggleTrigger();
    }

    /// <summary>
    /// 버튼에 새 색깔을 지정한다.
    /// </summary>
    /// <param name="newColor">새 색깔</param>
    public void SetButtonColor(ColorType newColor)
    {
        SetButtonColorServerRpc(newColor);
    }

    /// <summary>
    /// 버튼에 연결된 Activatable들을 서버 단에서 활성화한다.
    /// </summary>
    public void ActivateObjects()
    {
        ActivateObjectsServerRpc();
    }

    /// <summary>
    /// 버튼에 연결된 Activatable들을 서버 단에서 비활성화한다.
    /// </summary>
    public void DeactivateObjects()
    {
        DeactivateObjectsServerRpc();
    }

    /// <summary>
    /// 버튼을 활성화한다.
    /// </summary>
    public void EnableButton()
    {
        EnableButtonServerRpc(true);
    }

    /// <summary>
    /// 버튼을 비활성화한다.
    /// </summary>
    public void DisableButton()
    {
        EnableButtonServerRpc(false);
    }

    /// <summary>
    /// 버튼의 눌림 상태를 true로 바꾼다.
    /// </summary>
    public void PressButton()
    {
        PressButtonServerRpc(true);
    }

    /// <summary>
    /// 버튼의 눌림 상태를 false로 바꾼다.
    /// </summary>
    public void UnpressButton()
    {
        PressButtonServerRpc(false);
    }

    /// <summary>
    /// 버튼 누르기 애니메이션을 재생한다.
    /// </summary>
    /// <param name="isPressed">true: 누르기 / false: 떼기</param>
    public void PlayPressAnimation(bool isPressed)
    {
        SetAnimatorPressStateServerRpc(isPressed);
    }

    /// <summary>
    /// 버튼을 눌렀다 떼는 애니메이션을 재생한다.
    /// </summary>
    public void PlayToggleAnimation()
    {
        SetAnimatorToggleTriggerServerRpc();
    }
}
