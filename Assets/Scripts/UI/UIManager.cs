using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;

public class UIManager : SingletonBehavior<UIManager>
{
    [Tooltip("UIManager의 UIDocument")] [SerializeField]
    private UIDocument _uiDocument;

    private readonly string UI_PATH = "Prefabs/UI/";

    private Dictionary<Type, BaseUI> _uiPool = new Dictionary<Type, BaseUI>();
    private VisualElement _root;

    protected override void Init()
    {
        base.Init();
        _root = _uiDocument.rootVisualElement;
        _root.style.display = DisplayStyle.None;
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
        
        if (_root.childCount == 0)
        {
            _root.style.display = DisplayStyle.None;
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
}
