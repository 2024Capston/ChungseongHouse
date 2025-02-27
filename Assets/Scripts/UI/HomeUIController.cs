using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class HomeUIController : MonoBehaviour
{
    private VisualElement _homeUI;
    private VisualElement _homeUIContainer;

    private VisualElement _settingPanel;
    private VisualElement _playDataSelectPanel;
    
    private Button _createButton;
    private Button _joinButton;
    private Button _settingButton;
    private Button _exitButton;
    
    private static readonly string SettingsUI_PATH = "Prefabs/UI/SettingsUI";
    private static readonly string PlayDataSelectUI_PATH = "Prefabs/UI/PlayDataSelectUI";
    
    /// <summary>
    /// 방 만들기 버튼을 누르면 호출된다.
    /// </summary>
    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        _homeUI = root.Q<VisualElement>("HomeUI");
        _homeUIContainer = _homeUI.Q<VisualElement>("HomeUIContainer");
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
        var playDataSelect = Resources.Load<VisualTreeAsset>(PlayDataSelectUI_PATH);
        _playDataSelectPanel = playDataSelect.CloneTree();
        _playDataSelectPanel.style.position = Position.Absolute;
        _playDataSelectPanel.AddToClassList("right");
        new PlayDataSelectUI(_playDataSelectPanel, () =>
        {
            ClosePanel(_playDataSelectPanel);
        });
        
        _homeUI.Add(_playDataSelectPanel);
        _homeUIContainer.AddToClassList("HomeUIContainer--out");
        UIManager.Instance.StartPopupIn(_playDataSelectPanel);
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
        var setting = Resources.Load<VisualTreeAsset>(SettingsUI_PATH);

        _settingPanel = setting.CloneTree();

        _settingPanel.style.position = Position.Absolute;
        
        // 스크립트, Close Action 연결
        new SettingsUI(_settingPanel, () =>
        {
            ClosePanel(_settingPanel);
        });
        
        // HomeUIContainer 퇴장 애니메이션
        _homeUIContainer.AddToClassList("HomeUIContainer--out");
        
        // UI 화면에 SettingPanel 추가
        _homeUI.Add(_settingPanel);
        
        // settingPanel이 오른쪽에서 중앙으로 이동하기위해 class 추가
        _settingPanel.AddToClassList("right");
        
        // settingPanel을 중앙으로 이동
        UIManager.Instance.StartPopupIn(_settingPanel);
    }
    
    /// <summary>
    /// 종료 버튼을 누르면 호출된다.
    /// </summary>
    public void OnClickExitButton(ClickEvent evt)
    {
        Application.Quit();
    }
    
    public void HomeUIConatainerIn()
    {
        _homeUIContainer.RemoveFromClassList("HomeUIContainer--out");
    }

    public void ClosePanel(VisualElement panel)
    {
        HomeUIConatainerIn();

        UIManager.Instance.StartPopupOut(panel);
    }
}
