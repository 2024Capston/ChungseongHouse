using System;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// BaseUI를 사용할 때 필요한 Data Class
/// </summary>
public class BaseUIData
{
    public Action OnShow;
    public Action OnClose;
}

public class BaseUI
{
    protected VisualElement _root;

    public VisualElement Root
    {
        get => _root;
        set => _root = value;
    }

    private Action _onShow;
    private Action _onClose;

    public virtual void Init(VisualTreeAsset visualTree)
    {
        _root = visualTree.CloneTree();

        _root.style.position = Position.Absolute;
        _root.style.left = 0;
        _root.style.right = 0;
        _root.style.top = 0;
        _root.style.bottom = 0;
        _root.style.display = DisplayStyle.None;
        _root.name = GetType().Name;
    }

    public virtual void SetInfo(BaseUIData uiData)
    {
        _onShow = uiData.OnShow;
        _onClose = uiData.OnClose;
    }

    public virtual void ShowUI()
    {
        _onShow?.Invoke();
        _onClose = null;
        _root.style.display = DisplayStyle.Flex;
    }

    public virtual void CloseUI(bool isCloseAll = false)
    {
        if (!isCloseAll)
        {
            _onClose?.Invoke();
        }
        _onClose = null;
        
        _root.style.display = DisplayStyle.None;
        UIManager.Instance.CloseUI(this);
    }
}

