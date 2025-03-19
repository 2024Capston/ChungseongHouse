using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using UnityEngine.SceneManagement;

public class UIManager : SingletonBehavior<UIManager>
{
    [Tooltip("UIManager의 UIDocument")] [SerializeField]
    private UIDocument _uiDocument;

    private UIDocumentLocalization _localization;
    public UIDocumentLocalization Localization => _localization;

    private readonly string UI_PATH = "Prefabs/UI/";

    private Dictionary<Type, BaseUI> _uiPool = new Dictionary<Type, BaseUI>();
    private VisualElement _root;
    private BaseUI _frontUI;

    protected override void Init()
    {
        base.Init();
        _root = _uiDocument.rootVisualElement;
        
        _root.RegisterButtonClickSound();

        _root.style.display = DisplayStyle.None;
    
        _localization = GetComponent<UIDocumentLocalization>();
    }

    public void OpenUI<T>(BaseUIData uiData) where T : BaseUI, new()
    {
        Type uiType = typeof(T);

        bool isAlreadyOpen = false;
        BaseUI ui = GetUI<T>(out isAlreadyOpen);
        if (ui is null)
        {
            Logger.LogError($"{uiType} does not exist");
            return;
        }

        if (isAlreadyOpen)
        {
            Logger.Log($"{uiType} is already open.");
            return;
        }
        
        ui.SetInfo(uiData);
        _root.Add(ui.Root);
        _root.style.display = DisplayStyle.Flex;
        ui.ShowUI();

        _frontUI = ui;
    }

    public void CloseUI(BaseUI ui)
    {
        Type uiType = ui.GetType();
        
        VisualElement visualElement = _root.Q<VisualElement>(uiType.ToString());

        if (visualElement is null)
        {
            Logger.Log($"{uiType} is not opened");
            return;
        }
        
        visualElement.RemoveFromHierarchy();

        _frontUI = null;
        if (_root.childCount == 0)
        {
            _root.style.display = DisplayStyle.None;
            
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == SceneType.Lobby.ToString() || sceneName == SceneType.InGame.ToString())
            {
                if (_frontUI is null)
                {
                    UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                }
            }
            
        }
        else
        {
            var lastUI = _root.ElementAt(_root.childCount - 1);
            
            // 따로 찾을 방법이 없어서 ui pool 순회해서 Search
            // 어짜피 BaseUI가 많진 않아서 괜찮을듯?
            // 근데 맘에는 안드는데
            // 다른 방법이 없어서 더 화나는
            foreach (var baseUI in _uiPool.Values)
            {
                if (baseUI.Root == lastUI)
                {
                    _frontUI = baseUI;
                }
            }
        }
    }
    
    private BaseUI GetUI<T>(out bool isAlreadyOpen) where T : BaseUI, new()
    {
        Type uiType = typeof(T);

        BaseUI baseUI = null;
        isAlreadyOpen = false;

        // T가 이미 uipool에 존재할 경우
        if (_uiPool.TryGetValue(uiType, out baseUI))
        {
            // root VisualElement에 있을 땐 이미 열려있는 경우
            if (_root.Q<VisualElement>(uiType.ToString()) is not null)
            {
                isAlreadyOpen = true;
            }

            return baseUI;
        }
        
        // ui pool에 없으면 새롭게 생성
        VisualTreeAsset visualElement = Resources.Load<VisualTreeAsset>(UI_PATH + uiType);
        if (visualElement is null)
        {
            return null;
        }
        
        T ui = new T();
        ui.Init(visualElement);
        _uiPool.Add(uiType, ui);

        return ui;
    }

    
    public void CloseAllOpenUI()
    {
        while (_root.childCount != 0)
        {
            Type uiType = Type.GetType(_root.ElementAt(_root.childCount - 1).name);
            if (!_uiPool.TryGetValue(uiType, out BaseUI baseUI))
            {
                Logger.LogError($"{uiType} does not exist in uiPool");
                return;
            }
            Logger.Log($"{uiType} close");
            baseUI.CloseUI();
        }
    }
    
    public void StartPopupIn(VisualElement panel)
    {
        StartCoroutine(PopupUIManager.PopupIn(panel));
    }

    public void StartPopupOut(VisualElement panel)
    {
        StartCoroutine(PopupUIManager.PopupOut(panel));
    }
    
    public void OnEscapeInput()
    {
        Logger.Log("Escape button Inputed");
        
        if (_root.childCount == 0)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == SceneType.Lobby.ToString() || sceneName == SceneType.InGame.ToString())
            {
                UnityEngine.Cursor.lockState = CursorLockMode.None;

                EscUIData escUIData = new EscUIData()
                {
                    Localization = UIManager.Instance.Localization,
                    /*OnShow = () =>
                    {
                        // settingPanel이 오른쪽에서 중앙으로 이동하기위해 class 추가
                        _escPanel.AddToClassList("right");

                        // settingPanel을 중앙으로 이동
                        StartPopupIn(_escPanel);
                    },*/
                    //OnClose = () => ClosePanel(_escPanel)
                };
        
                Instance.OpenUI<EscUI>(escUIData);
            }
        }
        else
        {
            _frontUI.CloseUI();
        }
    }

    public void ResetUI()
    {
        CloseAllOpenUI();
        _uiPool.Clear();
    }
}
