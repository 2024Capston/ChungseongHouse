using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SettingsUIController 
{
    private VisualElement _root;
    private Button _closeSettingButton;
    public SettingsUIController(VisualElement root)
    {
        _root = root;
        var _closeSettingButton = _root.Q<Button>("CloseSettingButton");
        _closeSettingButton.RegisterCallback<ClickEvent>(OnClickCloseSettingButton);
    }

    public void OnClickCloseSettingButton(ClickEvent evt)
    {
        _root.RemoveFromHierarchy();
    }

}
