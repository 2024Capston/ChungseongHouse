using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputHandler : SingletonBehavior<InputHandler>
{
    public UnityAction<InputValue> OnMove;
    public UnityAction<InputValue> OnLookAround;
    public UnityAction OnJump;
    public UnityAction OnInteraction;
    public UnityAction OnEscape;

    private void OnDestroy()
    {
        OnMove = null;
        OnLookAround = null;
        OnJump = null;
        OnInteraction = null;
        OnEscape = null;

        base.OnDestory();
    }

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
