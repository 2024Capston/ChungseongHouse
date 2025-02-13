using System;
using UnityEngine.UIElements;
using TMPro;
using UnityEngine;

public enum ConfirmType
{
    OK,
    OK_Cancel,
}

public class ConfirmUIData : BaseUIData
{
    public ConfirmType ConfirmType;
    public string TitleText;
    public string ParagraphText;

    public string OKButtonText;
    public Action OnClickOKButton;

    public string CancelButtonText;
    public Action OnClickCancelButton;
}

public class ConfirmUI : BaseUI
{
    private Label _title;
    private Label _paragraph;
    private Button _okButton;
    private Button _cancelButton;

    private Action _onClickOKButton;
    private Action _onClickCancelButton;
    
    private ConfirmUIData _confirmUIData;

    public override void Init(VisualTreeAsset visualTree)
    {
        base.Init(visualTree);

        _title = _root.Q<Label>("Popup_Title");
        _paragraph = _root.Q<Label>("Popup_Paragraph");
        
        _okButton = _root.Q<Button>("Ok_Button");
        _onClickOKButton += OnClickOKButton;
        
        _cancelButton = _root.Q<Button>("Cancel_Button");
        _onClickCancelButton += OnClickCancelButton;
        _cancelButton.style.display = DisplayStyle.None;
    }
    
    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);
        
        _confirmUIData = uiData as ConfirmUIData;
        _title.text = _confirmUIData.TitleText;
        _paragraph.text = _confirmUIData.ParagraphText;
        
        _okButton.text = _confirmUIData.OKButtonText;
        _onClickOKButton = _confirmUIData.OnClickOKButton;
        
        _cancelButton.text = _confirmUIData.CancelButtonText;
        _onClickCancelButton = _confirmUIData.OnClickCancelButton;
        if (_confirmUIData.ConfirmType == ConfirmType.OK_Cancel)
        {
            _cancelButton.style.display = DisplayStyle.Flex;
        }
    }

    public void OnClickOKButton()
    {
        _onClickOKButton?.Invoke();
        _onClickOKButton = null;
        CloseUI();
    }

    public void OnClickCancelButton()
    {
        _onClickCancelButton?.Invoke();
        _onClickCancelButton = null;
        CloseUI();
    }
}
