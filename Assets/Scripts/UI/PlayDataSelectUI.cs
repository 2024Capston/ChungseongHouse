using RaB.Connection;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayDataSelectUIData : BaseUIData
{
    public UserGameData UserGameData;
}

/// <summary>
/// 저장된 데이터를 선택하는 UI
/// </summary>
public class PlayDataSelectUI : BaseUI
{
    [FormerlySerializedAs("_saveDatasText")] [SerializeField]
    private TMP_Text[] _playDatasText = new TMP_Text[3];

    private PlayDataSelectUIData _playDataSelectUIData;

    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);

        _playDataSelectUIData = uiData as PlayDataSelectUIData;

        for (int i = 0; i < 3; i++)
        {
            if (!_playDataSelectUIData.UserGameData.PlayDatas[i].HasData)
            {
                _playDatasText[i].text = "새 게임";
            }
            else
            {
                _playDatasText[i].text = $"챕터 진행도 : {_playDataSelectUIData.UserGameData.PlayDatas[i].StageClearCount} / {_playDataSelectUIData.UserGameData.PlayDatas[i].StageCount}";
            }
        }
    }

    /// <summary>
    /// 저장된 게임 슬롯을 선택하면 호출된다.
    /// </summary>
    /// <param name="index"></param>
    public void OnClickSelectButton(int index)
    {
        string descText;
        if (!_playDataSelectUIData.UserGameData.PlayDatas[index].HasData)
        {
            descText = "새 게임";
        }
        else
        {
            descText = $"챕터 진행도 : {_playDataSelectUIData.UserGameData.PlayDatas[index].StageClearCount} / {_playDataSelectUIData.UserGameData.PlayDatas[index].StageCount}";
        }


        ConfirmUIData confirmUIData = new ConfirmUIData()
        {
            ConfirmType = ConfirmType.OK_Cancel,
            TitleText = "확인",
            ParagraphText = $"{descText}을\n시작하겠습니까?",
            OKButtonText = "확인",
            CancelButtonText = "취소",
            OnClickOKButton = () =>
            {
                // OK 버튼을 눌렀을 때 새 게임은 새로 데이터를 생성
                if (!_playDataSelectUIData.UserGameData.PlayDatas[index].HasData)
                {
                    _playDataSelectUIData.UserGameData.SetNewData(index);
                    _playDataSelectUIData.UserGameData.SaveData();
                }
                
                // 선택한 인덱스로 세션을 생성하고 Server를 실행
                SessionManager.Instance.CreateSession(index);
                ConnectionManager.Instance.StartServer();
            },
        };
        UIManager.Instance.OpenUI<ConfirmUI>(confirmUIData);

    }

    public void OnClickUndoButton()
    {
        CloseUI();
    }
}
