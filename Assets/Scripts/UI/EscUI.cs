using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using ConnectionManager = RaB.Connection.ConnectionManager;
using UnityEngine.UIElements;

public class EscUIData : BaseUIData
{
    public UIDocumentLocalization Localization;
}

public class EscUI : BaseUI
{
    private EscUIData _escUIData;
    
    private VisualElement _escUI;
    private VisualElement _escPanel;

    private Button _roomCodeButton;
    private Button _mainButton;
    private Button _lobbyButton;
    private Button _respawnButton;
    private Button _settingButton;
    private Button _backButton;
    
    private Label _roomCodeLabel;
    
    private VisualElement _settingPanel;

    private string sceneName;

    
    private static readonly string SettingsUI_PATH = "Prefabs/UI/SettingsUI";

    private UIDocumentLocalization _localization;

    public override void Init(VisualTreeAsset visualTree)
    {
        base.Init(visualTree);
        _root.RegisterButtonClickSound();
        
        _escUI = _root.Q<VisualElement>("EscUI");
        _escPanel = _root.Q<VisualElement>("EscPanel");
        
        _roomCodeLabel = _root.Q<Label>("RoomCodeLabel");
        
        _mainButton = _root.Q<Button>("MainButton");
        _roomCodeButton = _root.Q<Button>("RoomCodeButton");
        _lobbyButton = _root.Q<Button>("LobbyButton");
        _respawnButton = _root.Q<Button>("RespawnButton");
        _settingButton = _root.Q<Button>("SettingButton");
        _backButton = _root.Q<Button>("BackButton");
        
        _mainButton.RegisterCallback<ClickEvent>(OnClickMain);
        _roomCodeButton.RegisterCallback<ClickEvent>(OnClickRoomCode);
        _roomCodeButton.RegisterCallback<MouseEnterEvent>(OnMouseEnterRoomCode);
        _roomCodeButton.RegisterCallback<MouseLeaveEvent>(OnMouseLeaveRoomCode);

        _lobbyButton.RegisterCallback<ClickEvent>(OnClickLobby);
        _respawnButton.RegisterCallback<ClickEvent>(OnClickRespawn);
        _settingButton.RegisterCallback<ClickEvent>(OnClickSettingButton);
        _backButton.RegisterCallback<ClickEvent>(OnClickCloseUI);
        
        sceneName = SceneManager.GetActiveScene().name;

        if (sceneName != SceneType.InGame.ToString())
        {
            _lobbyButton.style.display = DisplayStyle.None;
            _respawnButton.style.display = DisplayStyle.None;
        }
    }

    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);
        _escUIData = uiData as EscUIData;

        _localization = _escUIData.Localization;
        _roomCodeLabel.text = ConnectionManager.Instance.CurrentLobby?.Id.Value.ToString();
        ApplyLocalization(_root);
    }

    public override void CloseUI(bool isCloseAll = false)
    {
        _escPanel.RemoveFromClassList("left");
        if (_settingPanel is not null)
        {
            _settingPanel.RemoveFromClassList("center");
            _settingPanel.RemoveFromHierarchy();
            _settingPanel = null;
        }

        PlayerController.IsInputEnabled = true;
        CameraController.IsInputEnabled = true;

        base.CloseUI(isCloseAll);
    }

    private void OnClickMain(ClickEvent evt)
    {
        // Main 가는 기능
        ConnectionManager.Instance.RequestShutdown();
    }

    private void OnMouseEnterRoomCode(MouseEnterEvent evt)
    {
        string localizedCopy = LocalizationSettings.StringDatabase.GetLocalizedString("UI Table", "CLICK TO COPY");
        _roomCodeLabel.text =  localizedCopy;
    }

    private void OnClickRoomCode(ClickEvent evt)
    {
        GUIUtility.systemCopyBuffer =  ConnectionManager.Instance.CurrentLobby?.Id.Value.ToString();
    }

    private void OnMouseLeaveRoomCode(MouseLeaveEvent evt)
    {
        _roomCodeLabel.text =  ConnectionManager.Instance.CurrentLobby?.Id.Value.ToString();
    }
    
    // 로비
    private void OnClickLobby(ClickEvent evt)
    {
        InGameManager.Instance.EndGameServerRpc();
    }

    // Respawn시 소환될 위치 설정
    private void OnClickRespawn(ClickEvent evt)
    {
        StageManager.Instance.RestartGame();
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
        CloseUI();
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
