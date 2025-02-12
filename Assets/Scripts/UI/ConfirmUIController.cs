using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UIElements;

/*public enum ConfirmType
{
    OK,
    Ok_Cancel,
}
*/
public class ConfirmData : BaseUIData
{
    public ConfirmType ConfirmType;
    public string TitleText;
    public string DescText;

    public string OKButtonText;

    public string CancelButtonText;
}
public class ConfirmUIController : BaseUI
{
    private Label _title;
    private Label _paragraph;
    private Button _okButton;
    private Button _cancelButton;

    private ConfirmData _confirmData;

    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);

        var root = GetComponent<UIDocument>().rootVisualElement;

        _title = root.Q<Label>("Popup_Title");
        _paragraph = root.Q<Label>("Popup_Paragraph");
        _okButton = root.Q<Button>("Ok_Button");
        _cancelButton = root.Q<Button>("Cancel_Button");

        _title.text = _confirmData.TitleText;
        _paragraph.text =  _confirmData.DescText;
        _okButton.text = _confirmData.OKButtonText;
        _cancelButton.text = _confirmData.CancelButtonText;

        _okButton.RegisterCallback<ClickEvent>(OnClickOkButton);

    }

    private void OnClickOkButton(ClickEvent evt)
    {
        CloseUI();
    }
}
