using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Rendering;
using System;
using UnityEngine.Events;
using System.Runtime.CompilerServices;

/// <summary>
/// 일반적인 버튼을 조작하는 Class
/// </summary>
public class GenericButtonController : ButtonController
{
    /// <summary>
    /// 버튼 작동 방식
    /// </summary>
    [SerializeField] private ButtonType _buttonType;

    /// <summary>
    /// 작동 방식이 Temporary일 때 비활성화까지 걸리는 시간
    /// </summary>
    [SerializeField] private float _temporaryCooldown;

    /// <summary>
    /// 두 플레이어가 모두 근처에 위치해야 하는지 여부
    /// </summary>
    [SerializeField] private bool _requiresBoth;

    /// <summary>
    /// Requires Both가 true일 때 인식 반경
    /// </summary>
    [SerializeField] private float _detectionRadius;

    /// <summary>
    /// 버튼이 활성화됐을 때 실행될 함수
    /// </summary>
    [SerializeField] private UnityEvent _events;

    private float _temporaryTime = 0f;  // Temporary용 타이머

    private void Update()
    {
        if (!IsServer || !_isEnabled || _buttonType != ButtonType.Temporary)
        {
            return;
        }

        // Temporary 처리
        if (_isPressed)
        {
            _temporaryTime -= Time.deltaTime;

            if (_temporaryTime < 0f)
            {
                UnpressButton();
                DeactivateObjects();

                PlayPressAnimation(false);
            }
        }
    }

    public override bool OnInteractableCheck(PlayerController player)
    {
        // 버튼이 비활성화 상태인 경우
        if (!_isEnabled)
        {
            return false;
        }
        // 플레이어 둘이 주변에 있어야 하는 경우
        else if (_requiresBoth)
        {
            PlayerController[] playerControllers = FindObjectsOfType<PlayerController>();

            foreach (PlayerController playerController in playerControllers)
            {
                if (Vector3.Distance(transform.position, playerController.transform.position) > _detectionRadius)
                {
                    return false;
                }
            }
        }

        return (_color == ColorType.Purple || _color == player.Color) && (_buttonType == ButtonType.Toggle || !_isPressed);
    }

    public override bool OnStartInteraction(PlayerController player)
    {
        PressButtonServerRpc();

        return false;
    }

    /// <summary>
    /// 서버 측에서 버튼 입력을 처리한다.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void PressButtonServerRpc()
    {
        // 버튼 타입: Persistent
        if (_buttonType == ButtonType.Persistent && !_isPressed)
        {
            PressButton();
            ActivateObjects();

            PlayPressAnimation(true);

            _events?.Invoke();
        }
        // 버튼 타입: Toggle
        else if (_buttonType == ButtonType.Toggle)
        {
            if (_isPressed)
            {
                UnpressButton();
                DeactivateObjects();
            }
            else
            {
                PressButton();
                ActivateObjects();

                _events.Invoke();
            }

            PlayToggleAnimation();
        }
        // 버튼 타입: Temporary
        else if (_buttonType == ButtonType.Temporary)
        {
            if (!_isPressed)
            {
                PressButton();
                ActivateObjects();

                PlayPressAnimation(true);

                _events.Invoke();

                _temporaryTime = _temporaryCooldown;
            }
        }
    }

    [ClientRpc]
    private void InitializeClientRpc(ColorType color, ButtonType buttonType, bool requiresBoth, float detectionRadius)
    {
        _color = color;

        _buttonType = buttonType;

        _requiresBoth = requiresBoth;
        _detectionRadius = detectionRadius;
    }

    public void Initialize(ColorType color, GameObject[] activatables, ButtonType buttonType, float temporaryCooldown, bool requiresBoth, float detectionRadius, UnityEvent events)
    {
        InitializeClientRpc(color, buttonType, requiresBoth, detectionRadius);

        _activatables = activatables;
        _temporaryCooldown = temporaryCooldown;

        _events = events;
    }
}
