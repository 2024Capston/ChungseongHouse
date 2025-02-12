using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : SingletonBehavior<UIManager>
{
    [SerializeField]
    private Transform _uiCanvas;

    [SerializeField]
    private Transform _uiCloseCanvas;

    /// <summary>
    /// 가장 맨 위에 있는 UI
    /// </summary>
    private BaseUI _frontUI;

    /// <summary>
    /// 화면에 보이고 있는 UI Dictionary
    /// </summary>
    private Dictionary<System.Type, GameObject> _openUIPool = new Dictionary<System.Type, GameObject>();

    /// <summary>
    /// 화면에서 보이고 있지 않은 UI Dictionary
    /// </summary>
    private Dictionary<System.Type, GameObject> _closedUIPool = new Dictionary<System.Type, GameObject>();
    
    public Camera UICamara;
    
    protected override void Init()
    {
        base.Init();
    }

    /// <summary>
    /// BaseUI를 연다.
    /// </summary>
    /// <typeparam name="T">BaseUI를 상속받은 UI</typeparam>
    /// <param name="uiData">T에 필요한 UIData</param>
    public void OpenUI<T>(BaseUIData uiData)
    {
        System.Type uiType = typeof(T);

        Logger.Log($"{GetType()}::OpenUI({uiType})");

        bool isAlreadyOpen = false;
        BaseUI ui = GetUI<T>(out isAlreadyOpen);

        // BaseUI가 존재하지 않을 때
        if (!ui)
        {
            Logger.LogError($"{uiType} does not exist.");
            return;
        }

        // 열려는 UI가 이미 열려있을 때
        if (isAlreadyOpen)
        {
            Logger.LogError($"{uiType} is already open.");
            return;
        }

        // 열려있는 UI의 위치를 일관성있게 관리하기 위한 코드
        int siblingIndex = _uiCanvas.childCount;
        ui.Init(_uiCanvas);
        ui.transform.SetSiblingIndex(siblingIndex);

        // UI를 보이게 하고 초기화한다.
        ui.gameObject.SetActive(true);
        ui.SetInfo(uiData);
        ui.ShowUI();

        // 새롭게 열린 ui가 가장 앞에 있다.
        _frontUI = ui;
        _openUIPool[uiType] = ui.gameObject;
    }

    /// <summary>
    /// BaseUI를 닫는다.
    /// </summary>
    /// <param name="ui"></param>
    public void CloseUI(BaseUI ui)
    {
        System.Type uiType = ui.GetType();

        Logger.Log($"{GetType()}::CloseUI {uiType}");

        ui.gameObject.SetActive(false);

        // OpenUIPool에서 제거하고 CloseUIPool에 넣는다. (Object Pooling)
        _openUIPool.Remove(uiType);
        _closedUIPool[uiType] = ui.gameObject;
        ui.transform.SetParent(_uiCloseCanvas);

        // 열려있는 또다른 BaseUI가 있다면 해당 UI를 FrontUI로 설정한다.
        _frontUI = null;
        Transform lastChild = _uiCanvas.GetChild(_uiCanvas.childCount - 1);

        if (lastChild)
        {
            _frontUI = lastChild.gameObject.GetComponent<BaseUI>();
        }
    }

    /// <summary>
    /// BaseUI 객체를 가져온다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="isAlreadyOpen">열려있는지 여부</param>
    /// <returns></returns>
    private BaseUI GetUI<T>(out bool isAlreadyOpen)
    {
        System.Type uiType = typeof(T);

        BaseUI ui = null;
        isAlreadyOpen = false;

        // UI가 열려있는지 확인한다.
        if (_openUIPool.ContainsKey(uiType))
        {
            ui = _openUIPool[uiType].GetComponent<BaseUI>();
            isAlreadyOpen = true;
        }

        // UI가 닫혀있는지 확인한다.
        else if (_closedUIPool.ContainsKey(uiType))
        {
            ui = _closedUIPool[uiType].GetComponent<BaseUI>();
            _closedUIPool.Remove(uiType);
        }

        // UI 객체가 생성되지 않았으면 새로 생성한다.
        else
        {
            GameObject uiObj = Instantiate(Resources.Load<GameObject>($"Prefabs/UI/{uiType}"));
            ui = uiObj.GetComponent<BaseUI>();
        }

        return ui;
    }

    /// <summary>
    /// 열려 있는 모든 UI를 닫는다.
    /// </summary>
    public void CloseAllOpenUI()
    {
        while (_frontUI)
        {
            _frontUI.CloseUI();
        }
    }
}

namespace CustomUI
{
    public class UIManager : SingletonBehavior<UIManager>
    {
        private BaseUI _frontUI;

        private Dictionary<System.Type, BaseUI> _openUIPool = new Dictionary<System.Type, BaseUI>();

        private Dictionary<System.Type, BaseUI> _closedUIPool = new Dictionary<System.Type, BaseUI>();

        public void OpenUI<T>(BaseUIData uiData)
        {
            System.Type uiType = typeof(T);

            bool isAlreadyOpen = false;
            BaseUI ui = GetUI<T>(out isAlreadyOpen);
        }

        private BaseUI GetUI<T>(out bool isAlreadyOpen) 
        {
            System.Type uiType = typeof(T);
            BaseUI ui = null;

            if (_openUIPool.ContainsKey(uiType))
            {
                ui = _openUIPool[uiType];
                isAlreadyOpen = true;
            }
            else if (_closedUIPool.ContainsKey(uiType))
            {
                ui = _closedUIPool[uiType];
                _closedUIPool.Remove(uiType);
            }
            else
            {
                VisualTreeAsset visualTreeAsset = Resources.Load<VisualTreeAsset>($"Prefabs/UI/{uiType}");
                VisualElement visualElement = visualTreeAsset.CloneTree();
               
            }
        }
    }
}
