using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using RaB.Connection;

public class PlayDataSelectUI 
{
    private VisualElement _root;

    private Button _saveSlot1Button;
    private Button _saveSlot2Button;
    private Button _saveSlot3Button;
    private Button _backButton;

    private List<Button> _buttonContainer;
    // Start is called before the first frame update
    public PlayDataSelectUI(VisualElement root)
    {
        _root = root;
        _saveSlot1Button = _root.Q<Button>("SaveSlot1_Button");
        _saveSlot2Button = _root.Q<Button>("SaveSlot2_Button");
        _saveSlot3Button = _root.Q<Button>("SaveSlot3_Button");
        _backButton = _root.Q<Button>("Back_Button");
        
        _buttonContainer = new List<Button>(){_saveSlot1Button, _saveSlot2Button, _saveSlot3Button};

        for (int i = 0; i < 3; i++)
        {
            if (!HomeManager.Instance.UserGameData.PlayDatas[i].HasData)
            {
                _buttonContainer[i].text = "새 게임";
            }
            else
            {
                _buttonContainer[i].text =  $"챕터 진행도 : {HomeManager.Instance.UserGameData.PlayDatas[i].StageClearCount} / {HomeManager.Instance.UserGameData.PlayDatas[i].StageCount}";
            }
        }

        for (int i = 0; i < 3; i++)
        {
            int index = i;
            _buttonContainer[i].clicked += () =>
            {
                OnClickSaveSlot(index);
            };
        }
        
        _backButton.RegisterCallback<ClickEvent>(OnClickBackButton);
    }

    private void OnClickSaveSlot(int index)
    {
        if (!HomeManager.Instance.UserGameData.PlayDatas[index].HasData)
        {
            HomeManager.Instance.UserGameData.SetNewData(index);
            HomeManager.Instance.UserGameData.SaveData();
        }
                
        // 선택한 인덱스로 세션을 생성하고 Server를 실행
        SessionManager.Instance.CreateSession(index);
        ConnectionManager.Instance.StartServer();
    }

    private void OnClickBackButton(ClickEvent evt)
    {
        _root.RemoveFromHierarchy();
    }
}
