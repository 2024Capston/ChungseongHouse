using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어의 입력을 처리하는 Class
/// </summary>
public class InputHandler : SingletonBehavior<InputHandler>
{
    // 다음 Delegate에 등록해서 입력을 받을 수 있음
    public UnityAction<InputValue> OnMove;
    public UnityAction<InputValue> OnLookAround;
    public UnityAction OnJump;
    public UnityAction OnInteraction;
    public UnityAction OnEscape;

    void OnMoveInput(InputValue value)
    {
        OnMove?.Invoke(value);
    }

    void OnLookAroundInput(InputValue value)
    {
        OnLookAround?.Invoke(value);
    }

    void OnJumpInput()
    {
        OnJump?.Invoke();
    }

    void OnInteractionInput()
    {
        OnInteraction?.Invoke();
    }

    void OnEscapeInput()
    {
        OnEscape?.Invoke();
    }
}
