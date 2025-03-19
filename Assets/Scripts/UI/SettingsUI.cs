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
    private Button _language;
    private Action _onCloseSetting;

    private VisualElement _audioPanel;
    private VisualElement _videoPanel;
    private VisualElement _languagePanel;

    private VisualElement newPanel;

    private UIDocumentLocalization _localization; // UIDocumentLocalization 참조

    private static readonly string AudioUI_PATH = "Prefabs/UI/Setting/AudioUI";
    private static readonly string VideoUI_PATH = "Prefabs/UI/Setting/VideoUI";
    private static readonly string LanguageUI_PATH = "Prefabs/UI/Setting/LanguageUI";

    public SettingsUI(VisualElement root, Action onCloseSettingButtonClick, UIDocumentLocalization localization, bool fromUIManager = false)
    {
        _root = root;
        _localization = localization; // UIDocumentLocalization 참조 저장

        _root.RegisterButtonClickSound();

        _onCloseSetting = onCloseSettingButtonClick;

        _settingUI = _root.Q<VisualElement>("SettingUI");
        _settingPanel = _root.Q<VisualElement>("SettingPanel");

        _audio = _root.Q<Button>("AudioSettingButton");
        _video = _root.Q<Button>("VideoSettingButton");
        _language = _root.Q<Button>("LanguageSettingButton");
        _closeSettingButton = _root.Q<Button>("CloseSettingButton");

        _audio.RegisterCallback<ClickEvent>(OnClickAudio);
        _video.RegisterCallback<ClickEvent>(OnClickVideo);
        _language.RegisterCallback<ClickEvent>(OnClickLanguage);
        _closeSettingButton.RegisterCallback<ClickEvent>(OnClickCloseSettingButton);

        if (fromUIManager)
        {
            _settingUI.style.backgroundColor = new Color(0, 0, 0, 0);
        }

        // 초기 번역 적용
        ApplyLocalization(_root);
    }

    private void OnClickAudio(ClickEvent evt)
    {
        NewSettingUI<AudioUIController>(AudioUI_PATH);
    }

    private void OnClickVideo(ClickEvent evt)
    {
        NewSettingUI<VideoUIController>(VideoUI_PATH);
    }
    
    private void OnClickLanguage(ClickEvent evt)
    {
        NewSettingUI<LanguageUIController>(LanguageUI_PATH);
    }

    private void OnClickCloseSettingButton(ClickEvent evt)
    {
        _onCloseSetting?.Invoke();
    }

    private void ClosePanel(VisualElement panel)
    {
        _settingPanel.RemoveFromClassList("left");
        UIManager.Instance.StartPopupOut(panel);
    }

    private void NewSettingUI<T>(string PATH) where T : class
    {
        var newUI = Resources.Load<VisualTreeAsset>(PATH);
        newPanel = newUI.CloneTree();
        newPanel.style.position = Position.Absolute;

        // T가 특정 생성자를 가지도록 강제
        var controller = Activator.CreateInstance(typeof(T), newPanel, (Action)(() => ClosePanel(newPanel))) as T;

        // SettingMenu 퇴장 애니메이션
        _settingPanel.AddToClassList("left");

        // UI 화면에 SettingPanel 추가
        _settingUI.Add(newPanel);
        
        // 번역 적용
        ApplyLocalization(newPanel);

        // settingPanel이 오른쪽에서 중앙으로 이동하기 위해 class 추가
        newPanel.AddToClassList("right");

        // settingPanel을 중앙으로 이동
        UIManager.Instance.StartPopupIn(newPanel);
    }

    private void ApplyLocalization(VisualElement panel)
    {
        if (_localization != null)
        {
            var table = _localization.GetTable(); 
            if (table != null)
            {
                _localization.LocalizeChildrenRecursively(panel, table);
            }
        }
    }
}