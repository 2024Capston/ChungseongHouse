using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyUIController : MonoBehaviour
{
    private Label _playerColorData;
    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        _playerColorData = root.Q<Label>("PlayerColorData");
    }

    public void SetPlayerColorData(bool isHost)
    {
        if (isHost)
        {
            _playerColorData.text = "Your Color: <color=\"blue\">Blue</color>";
        }
        else
        {
            _playerColorData.text = "Your Color: <color=\"red\">Red</color>";
        }
    }
}
