using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class SettingsUI : MonoBehaviour
{
    private VisualElement _root;
    private Button _closeSettingButton;
    private HomeUIController _homeUIController;
    private Action OnCloseSetting;
    public SettingsUI(VisualElement root, Action OnCloseSettingButtonClick)
    {
        _root = root;
        OnCloseSetting = OnCloseSettingButtonClick;
        
        _closeSettingButton = _root.Q<Button>("CloseSettingButton");
        _closeSettingButton.RegisterCallback<ClickEvent>(OnClickCloseSettingButton);
    }

    private void OnClickCloseSettingButton(ClickEvent evt)
    {
        OnCloseSetting?.Invoke();
    }

}
