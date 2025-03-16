using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using ConnectionManager = RaB.Connection.ConnectionManager;
using UnityEngine.UIElements;

public class EscUI 
{ 
    private VisualElement _root;
    private VisualElement _escUI;
    private VisualElement _escPanel;

    private Button _roomCodeButton;
    private Button _lobbyButton;
    private Button _respawnButton;
    private Button _settingButton;
    private Button _backButton;
    
    private Action OnCloseUI;
    
    private Label _roomCodeLabel;
    
    private VisualElement _settingPanel;

    
    private static readonly string SettingsUI_PATH = "Prefabs/UI/SettingsUI";

    private UIDocumentLocalization _localization;
    
    public EscUI(VisualElement root, Action OnCloseSettingButtonClick, UIDocumentLocalization localization)
    {
        _root = root;
        _localization = localization; // UIDocumentLocalization 참조 저장

        _root.RegisterButtonClickSound();

        OnCloseUI = OnCloseSettingButtonClick;

        _escUI = _root.Q<VisualElement>("EscUI");
        _escPanel = _root.Q<VisualElement>("EscPanel");
        
        _roomCodeLabel = root.Q<Label>("RoomCodeLabel");
        
        _roomCodeButton = _root.Q<Button>("RoomCodeButton");
        _lobbyButton = _root.Q<Button>("LobbyButton");
        _respawnButton = _root.Q<Button>("RespawnButton");
        _settingButton = _root.Q<Button>("SettingButton");
        _backButton = _root.Q<Button>("BackButton");
        
        _roomCodeLabel.text = ConnectionManager.Instance.CurrentLobby?.Id.Value.ToString();

        _roomCodeButton.RegisterCallback<ClickEvent>(OnClickRoomCode);
        _lobbyButton.RegisterCallback<ClickEvent>(OnClickLobby);
        _respawnButton.RegisterCallback<ClickEvent>(OnClickRespawn);
        _settingButton.RegisterCallback<ClickEvent>(OnClickSettingButton);
        _backButton.RegisterCallback<ClickEvent>(OnClickCloseUI);

        // 초기 번역 적용
        ApplyLocalization(_root);
    }

    private void OnClickRoomCode(ClickEvent evt)
    {
        GUIUtility.systemCopyBuffer =  ConnectionManager.Instance.CurrentLobby?.Id.Value.ToString();
    }

    // 로비
    private void OnClickLobby(ClickEvent evt)
    {
        throw new NotImplementedException();
    }

    // Respawn시 소환될 위치 설정
    private void OnClickRespawn(ClickEvent evt)
    {
        throw new NotImplementedException();
    }
    
    private void OnClickSettingButton(ClickEvent evt)
    {
        Debug.Log("OnClickSettingButton");
        
        var setting = Resources.Load<VisualTreeAsset>(SettingsUI_PATH);
        _settingPanel = setting.CloneTree();

        _settingPanel.style.position = Position.Absolute;



        // SettingUI 생성 및 번역 적용
        new SettingsUI(_settingPanel, () =>
        {
            ClosePanel(_settingPanel);
        }, _localization, true);

        _escPanel.AddToClassList("left");

        // UI 화면에 SettingPanel 추가
        _escUI.Add(_settingPanel);

        // settingPanel이 오른쪽에서 중앙으로 이동하기 위해 class 추가
        _settingPanel.AddToClassList("right");

        // settingPanel을 중앙으로 이동
        UIManager.Instance.StartPopupIn(_settingPanel);
    }
    
    private void OnClickCloseUI(ClickEvent evt)
    {
        OnCloseUI?.Invoke();
    }

    private void ClosePanel(VisualElement panel)
    {
        _escPanel.RemoveFromClassList("left");
        UIManager.Instance.StartPopupOut(panel);
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
