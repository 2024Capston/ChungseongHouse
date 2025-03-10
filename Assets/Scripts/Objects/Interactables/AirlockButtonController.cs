using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AirlockButtonController : MonoBehaviour, IInteractable
{
    [SerializeField] private AirlockController _airlockController;
    [SerializeField] private bool _isInButton;
    [SerializeField] private ColorType _buttonColor;

    private Outline _outline;
    public Outline Outline
    {
        get => _outline;
        set => _outline = value;
    }

    private void Start()
    {
        _outline = GetComponent<Outline>();
        _outline.enabled = false;
    }

    public bool IsInteractable(PlayerController player)
    {
        return true;
        return player.Color == _buttonColor;
    }

    public bool StartInteraction(PlayerController player)
    {
        _airlockController.OnClickAirlockButtonServerRpc(_buttonColor, _isInButton);
        return false;
    }

    public bool StopInteraction(PlayerController playerController)
    {
        return true;
    }
}