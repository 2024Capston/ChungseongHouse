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
    private VisualElement _codeInputPanel;
    
    private Button _createButton;
    private Button _joinButton;
    private Button _settingButton;
    private Button _exitButton;
    
    private static readonly string SettingsUI_PATH = "Prefabs/UI/SettingsUI";
    private static readonly string PlayDataSelectUI_PATH = "Prefabs/UI/PlayDataSelectUI";
    private static readonly string CodeInputUI_PATH = "Prefabs/UI/CodeInputUI";
    
   
    /*private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        
        SoundManager.RegisterButtonClickSound(root);
        
        _homeUI = root.Q<VisualElement>("HomeUI");
        _homeUIContainer = _homeUI.Q<VisualElement>("HomeUIContainer");
        _createButton = root.Q<Button>("Create_Button");
        _joinButton = root.Q<Button>("Join_Button");
        _settingButton = root.Q<Button>("Setting_Button");
        _exitButton = root.Q<Button>("Exit_Button");

        RegisterButtonEvents();
    }*/
    
    private void OnEnable()
    {
        var localization = GetComponent<UIDocumentLocalization>();
        localization.onLocalizationCompleted += RegisterButtonEvents;
    }

    private void OnDisable()
    {
        var localization = GetComponent<UIDocumentLocalization>();
        localization.onLocalizationCompleted -= RegisterButtonEvents;
    }

    private void RegisterButtonEvents()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        
        root.RegisterButtonClickSound();
        
        _homeUI = root.Q<VisualElement>("HomeUI");
        _homeUIContainer = _homeUI.Q<VisualElement>("HomeUIContainer");

        _createButton = root.Q<Button>("Create_Button");
        _joinButton = root.Q<Button>("Join_Button");
        _settingButton = root.Q<Button>("Setting_Button");
        _exitButton = root.Q<Button>("Exit_Button");
        
        _createButton.UnregisterCallback<ClickEvent>(OnClickCreateButton);
        _joinButton.UnregisterCallback<ClickEvent>(OnClickEnterButton);
        _settingButton.UnregisterCallback<ClickEvent>(OnClickSettingButton);
        _exitButton.UnregisterCallback<ClickEvent>(OnClickExitButton);

        _createButton.RegisterCallback<ClickEvent>(OnClickCreateButton);
        _joinButton.RegisterCallback<ClickEvent>(OnClickEnterButton);
        _settingButton.RegisterCallback<ClickEvent>(OnClickSettingButton);
        _exitButton.RegisterCallback<ClickEvent>(OnClickExitButton);
    }
    
    /// <summary>
    /// 방 만들기 버튼을 누르면 호출된다.
    /// </summary>
    private void OnClickCreateButton(ClickEvent evt)
    {
        var playDataSelect = Resources.Load<VisualTreeAsset>(PlayDataSelectUI_PATH);
        _playDataSelectPanel = playDataSelect.CloneTree();
        _playDataSelectPanel.style.position = Position.Absolute;
        
        _playDataSelectPanel.AddToClassList("right");
        
        var localization = GetComponent<UIDocumentLocalization>();

        new PlayDataSelectUI(_playDataSelectPanel, () =>
        {
            ClosePanel(_playDataSelectPanel);
        });
        
        ApplyLocalization(_playDataSelectPanel);

        
        _homeUI.Add(_playDataSelectPanel);
        _homeUIContainer.AddToClassList("HomeUIContainer--out");
        UIManager.Instance.StartPopupIn(_playDataSelectPanel);
    }

    /// <summary>
    /// 방 참가 버튼을 누르면 호출된다.
    /// </summary>
    private void OnClickEnterButton(ClickEvent evt)
    {
        var codeInput = Resources.Load<VisualTreeAsset>(CodeInputUI_PATH);
        _codeInputPanel = codeInput.CloneTree();
        _codeInputPanel.style.position = Position.Absolute;
        _codeInputPanel.AddToClassList("right");
        
        new CodeInputUI(_codeInputPanel, () =>
        {
            ClosePanel(_codeInputPanel);
        });
        
        ApplyLocalization(_codeInputPanel);

        _homeUI.Add(_codeInputPanel);
        _homeUIContainer.AddToClassList("HomeUIContainer--out");
        UIManager.Instance.StartPopupIn(_codeInputPanel);
    }

    /// <summary>
    /// 설정 버튼을 누르면 호출된다.
    /// </summary>
    private void OnClickSettingButton(ClickEvent evt)
    {
        Debug.Log("OnClickSettingButton");
        
        var setting = Resources.Load<VisualTreeAsset>(SettingsUI_PATH);
        _settingPanel = setting.CloneTree();

        _settingPanel.style.position = Position.Absolute;

        // UIDocumentLocalization 참조 가져오기
        var localization = GetComponent<UIDocumentLocalization>();

        // SettingUI 생성 및 번역 적용
        new SettingsUI(_settingPanel, () =>
        {
            ClosePanel(_settingPanel);
        }, localization);

        // HomeUIContainer 퇴장 애니메이션
        _homeUIContainer.AddToClassList("HomeUIContainer--out");

        // UI 화면에 SettingPanel 추가
        _homeUI.Add(_settingPanel);

        // settingPanel이 오른쪽에서 중앙으로 이동하기 위해 class 추가
        _settingPanel.AddToClassList("right");

        // settingPanel을 중앙으로 이동
        UIManager.Instance.StartPopupIn(_settingPanel);
    }
    
    private void ApplyLocalization(VisualElement panel)
    {
        var localization = GetComponent<UIDocumentLocalization>();
        if (localization != null)
        {
            var table = localization.GetTable();
            if (table != null)
            {
                localization.LocalizeChildrenRecursively(panel, table);
            }
        }
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
