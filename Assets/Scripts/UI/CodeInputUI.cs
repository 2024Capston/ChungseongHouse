using System;
using System.Runtime.CompilerServices;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class CodeInputUI
{
    private VisualElement _root;

    private TextField _codeTextField;
    private Label _descText;
    private Button _okButton;
    private Button _backButton;

    private Action OnClosePanel;
    public CodeInputUI(VisualElement root, Action OnCloseCodeInputUI)
    {
        _root = root;
        _codeTextField = _root.Q<TextField>("CodeTextField");
        _descText = _root.Q<Label>("DescText");
        _okButton = _root.Q<Button>("OK_Button");
        _backButton = _root.Q<Button>("Back_Button");
        
        _descText.text = "";
        OnClosePanel = OnCloseCodeInputUI;
        _okButton.clicked += OnClickOkButton;
        _backButton.clicked += OnClickBackButton;
    }
    
    /// <summary>
    ///  버튼을 눌렀을 때 일치하는 Lobby가 있는지 확인하고, 해당하는 Lobby가 있으면 입장한다.
    /// </summary>
    public async void OnClickOkButton()
    {
        // 코드를 입력받고 그에 따른 처리를 합니다.
        Result result = await RaB.Connection.ConnectionManager.Instance.StartClient(_codeTextField.text);

        switch (result)
        {
            // 유효한 코드의 경우 바로 해당 Lobby로 입장
            case Result.OK:
            {
                _descText.text = "Find the lobby!";
                _descText.style.color = UnityEngine.Color.white;
                OnClosePanel?.Invoke();
                break;
            }
            // 코드 형식이 올바르지 않은 경우
            case Result.InvalidParam:
            {
                _descText.text = "The format is incorrect.";
                _descText.style.color = UnityEngine.Color.red;
                break;
            }
            // 로비가 꽉 찾을 때
            case Result.Busy:
            {
                _descText.text = "Lobby is full";
                _descText.style.color = UnityEngine.Color.red;
                break;
            }
            // 유효하지 않은 코드
            case Result.InvalidName:
            {
                _descText.text = "Couldn't find the lobby";
                _descText.style.color = UnityEngine.Color.red;
                break;
            }
        }
    }

    public void OnClickBackButton()
    {
        OnClosePanel?.Invoke();
    }
}
