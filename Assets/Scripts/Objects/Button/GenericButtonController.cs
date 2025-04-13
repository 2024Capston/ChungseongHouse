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
    private ButtonType _buttonType;

    /// <summary>
    /// 작동 방식이 Temporary일 때 비활성화까지 걸리는 시간
    /// </summary>
    private float _temporaryCooldown;

    /// <summary>
    /// 두 플레이어가 모두 근처에 위치해야 하는지 여부
    /// </summary>
    private bool _requiresBoth;

    /// <summary>
    /// Requires Both가 true일 때 인식 반경
    /// </summary>
    private float _detectionRadius;

    /// <summary>
    /// 버튼을 눌렀을 때 호출할 이벤트
    /// </summary>
    private EventType[] _publishOnPress;

    /// <summary>
    /// 버튼을 뗐을 때 호출할 이벤트
    /// </summary>
    private EventType[] _publishOnUnpress;

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

                foreach (EventType eventType in _publishOnUnpress)
                {
                    EventBus.Instance.InvokeEvent(eventType);
                }
            }
        }
    }

    private new void OnDestroy()
    {
        base.OnDestroy();
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
        player.GetComponent<PlayerRenderer>().PlayTouchAnimation(transform.position + Vector3.up * 10f);
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

            foreach (EventType eventType in _publishOnPress)
            {
                EventBus.Instance.InvokeEvent(eventType);
            }
        }
        // 버튼 타입: Toggle
        else if (_buttonType == ButtonType.Toggle)
        {
            if (_isPressed)
            {
                UnpressButton();
                DeactivateObjects();

                foreach (EventType eventType in _publishOnUnpress)
                {
                    EventBus.Instance.InvokeEvent(eventType);
                }
            }
            else
            {
                PressButton();
                ActivateObjects();

                foreach (EventType eventType in _publishOnPress)
                {
                    EventBus.Instance.InvokeEvent(eventType);
                }
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

                foreach (EventType eventType in _publishOnPress)
                {
                    EventBus.Instance.InvokeEvent(eventType);
                }

                _temporaryTime = _temporaryCooldown;
            }
        }
    }

    /// <summary>
    /// 서버와 클라이언트의 초기 상태를 동기화한다. 이 함수는 서버와 클라이언트 모두에서 호출된다.
    /// </summary>
    /// <param name="color">색깔</param>
    /// <param name="buttonType">작동 방식</param>
    /// <param name="requiresBoth">두 명 필요 여부</param>
    /// <param name="detectionRadius">인식 범위</param>
    [ClientRpc]
    private void InitializeClientRpc(ColorType color, ButtonType buttonType, bool requiresBoth, float detectionRadius)
    {
        _color = color;

        _buttonType = buttonType;

        _requiresBoth = requiresBoth;
        _detectionRadius = detectionRadius;
    }

    /// <summary>
    /// 버튼 상태를 초기화하고 클라이언트와 동기화한다. 이 함수는 서버에서만 호출한다.
    /// </summary>
    /// <param name="color">색깔</param>
    /// <param name="buttonType">작동 방식</param>
    /// <param name="temporaryCooldown">지속 시간</param>
    /// <param name="requiresBoth">두 명 필요 여부</param>
    /// <param name="detectionRadius">인식 범위</param>
    public void Initialize(ColorType color, ButtonType buttonType, float temporaryCooldown, bool requiresBoth, float detectionRadius, EventType[] publishOnPress, EventType[] publishOnUnpress, EventType[] subscribeForEnable, EventType[] subscribeForDisable)
    {
        InitializeClientRpc(color, buttonType, requiresBoth, detectionRadius);

        _temporaryCooldown = temporaryCooldown;

        _publishOnPress = publishOnPress;
        _publishOnUnpress = publishOnUnpress;

        foreach (EventType eventType in subscribeForEnable)
        {
            EventBus.Instance.SubscribeEvent<UnityAction>(eventType, EnableButton);
        }

        foreach (EventType eventType in subscribeForDisable)
        {
            EventBus.Instance.SubscribeEvent<UnityAction>(eventType, DisableButton);
        }
    }

    /// <summary>
    /// 버튼에 연결된 Activatable을 추가한다.
    /// </summary>
    /// <param name="activatable">Activatable 오브젝트</param>
    public void AddActivatable(GameObject activatable)
    {
        // Array 뒤에 하나만 Append하기 위해 List로 잠시 변환
        List<GameObject> activatables = new List<GameObject>(_activatables);
        activatables.Add(activatable);
        _activatables = activatables.ToArray();
    }
}
