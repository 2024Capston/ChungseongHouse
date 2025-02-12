using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class HomeUIController : MonoBehaviour
{
    private VisualElement _homeUIContainer;

    private VisualElement _settingPanel;

    private Button _createButton;
    private Button _joinButton;
    private Button _settingButton;
    private Button _exitButton;
    private string SettingsUI = "Prefabs/UI/SettingsUI";
    /// <summary>
    /// 방 만들기 버튼을 누르면 호출된다.
    /// </summary>
    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        _homeUIContainer = root.Q<VisualElement>("HomeUIContainer");
        _createButton = root.Q<Button>("Create_Button");
        _joinButton = root.Q<Button>("Join_Button");
        _settingButton = root.Q<Button>("Setting_Button");
        _exitButton = root.Q<Button>("Exit_Button");

        _createButton.RegisterCallback<ClickEvent>(OnClickCreateButton);
        _joinButton.RegisterCallback<ClickEvent>(OnClickEnterButton);
        _settingButton.RegisterCallback<ClickEvent>(OnClickSettingButton);
        _exitButton.RegisterCallback<ClickEvent>(OnClickExitButton);
    }
    public void OnClickCreateButton(ClickEvent evt)
    {
        PlayDataSelectUIData playDataSelectUIData = new PlayDataSelectUIData()
        {
            UserGameData = HomeManager.Instance.UserGameData
        };
        UIManager.Instance.OpenUI<PlayDataSelectUI>(playDataSelectUIData);

    }

    /// <summary>
    /// 방 참가 버튼을 누르면 호출된다.
    /// </summary>
    public void OnClickEnterButton(ClickEvent evt)
    {
        BaseUIData uidata = new BaseUIData();
        UIManager.Instance.OpenUI<CodeInputUI>(uidata);
    }

    /// <summary>
    /// 설정 버튼을 누르면 호출된다.
    /// </summary>
    public void OnClickSettingButton(ClickEvent evt)
    {
        var setting = Resources.Load<VisualTreeAsset>(SettingsUI);

        _settingPanel = setting.CloneTree();

        _settingPanel.style.position = Position.Absolute;

        new SettingsUIController(_settingPanel);

        _homeUIContainer.Add(_settingPanel);
    }
    // TODO : 설정 팝업을 만들면 해당 팝업을 띄운다.

    /// <summary>
    /// 종료 버튼을 누르면 호출된다.
    /// </summary>
    public void OnClickExitButton(ClickEvent evt)
    {
        Application.Quit();
    }
}
