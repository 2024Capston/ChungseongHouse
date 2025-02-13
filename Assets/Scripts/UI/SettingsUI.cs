using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SettingsUI 
{
    private VisualElement _root;
    private Button _closeSettingButton;
    public SettingsUI(VisualElement root)
    {
        _root = root;
        _closeSettingButton = _root.Q<Button>("CloseSettingButton");
        _closeSettingButton.RegisterCallback<ClickEvent>(OnClickCloseSettingButton);
    }

    private void OnClickCloseSettingButton(ClickEvent evt)
    {
        _root.RemoveFromHierarchy();
    }

}
