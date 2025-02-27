using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class SettingsUI 
{
    private VisualElement _root;
    private VisualElement _settingUI;
    private VisualElement _settingPanel;
    
    private Button _closeSettingButton;
    private Button _audio;
    private Button _video;
    private Button _control;
    private Button _language;
    private Action OnCloseSetting;
    
    private VisualElement _audioPanel;
    private VisualElement _videoPanel;
    private VisualElement _controlPanel;
    private VisualElement _languagePanel;

    private VisualElement _newPanel;

    
   // private static readonly string AudioUI_PATH = "Prefabs/UI/Setting/AudioUI";
    private static readonly string VideoUI_PATH = "Prefabs/UI/Setting/VideoUI";
   // private static readonly string ControlsUI_PATH = "Prefabs/UI/Setting/ControlsUI";
   // private static readonly string LanguageUI_PATH = "Prefabs/UI/Setting/LanguageUI";

    public SettingsUI(VisualElement root, Action OnCloseSettingButtonClick)
    {
        _root = root;
        OnCloseSetting = OnCloseSettingButtonClick;
        
        _settingUI = _root.Q<VisualElement>("SettingUI");
        _settingPanel = _root.Q<VisualElement>("SettingPanel");
        
        _audio = _root.Q<Button>("AudioSettingButton");
        _video = _root.Q<Button>("VideoSettingButton");
        _control = _root.Q<Button>("ControlSettingButton");
        _language = _root.Q<Button>("LanguageSettingButton");
        _closeSettingButton = _root.Q<Button>("CloseSettingButton");
        
        _audio.RegisterCallback<ClickEvent>(OnClickAudio);
        _video.RegisterCallback<ClickEvent>(OnClickVideo);
        _control.RegisterCallback<ClickEvent>(OnClickControl);
        _language.RegisterCallback<ClickEvent>(OnClickLanguage);
        _closeSettingButton.RegisterCallback<ClickEvent>(OnClickCloseSettingButton);
    }
    
    private void OnClickAudio(ClickEvent evt)
    {

    }
    private void OnClickVideo(ClickEvent evt)
    {
        newSettingUI<VideoUIController>(VideoUI_PATH);

    }
    
    private void OnClickControl(ClickEvent evt)
    {
    }
    private void OnClickLanguage(ClickEvent evt)
    {
    }

    private void OnClickCloseSettingButton(ClickEvent evt)
    {
        OnCloseSetting?.Invoke();
    }
    
    public void ClosePanel(VisualElement panel)
    {
        _settingPanel.RemoveFromClassList("left");

        UIManager.Instance.StartPopupOut(panel);
    }
    
    private void newSettingUI<T>(string PATH) where T : class
    {
        var _newUI = Resources.Load<VisualTreeAsset>(PATH);
        _newPanel = _newUI.CloneTree();
        _newPanel.style.position = Position.Absolute;

        // T가 특정 생성자를 가지도록 강제
        var controller = Activator.CreateInstance(typeof(T), _newPanel, (Action)(() => ClosePanel(_newPanel))) as T;

        // SettingMenu 퇴장 애니메이션
        _settingPanel.AddToClassList("left");

        // UI 화면에 SettingPanel 추가
        _settingUI.Add(_newPanel);

        // settingPanel이 오른쪽에서 중앙으로 이동하기 위해 class 추가
        _newPanel.AddToClassList("right");

        // settingPanel을 중앙으로 이동
        UIManager.Instance.StartPopupIn(_newPanel);
    }
}
