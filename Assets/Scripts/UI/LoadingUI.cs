using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;

public class LoadingUI : BaseUI
{
    private readonly string TEXT = "Loading";

    private Label _loadingText;
    private UniTaskCompletionSource _taskCompletionSource;
        
    public override void Init(VisualTreeAsset visualTree)
    {
        base.Init(visualTree);

        _loadingText = _root.Q<Label>("LoadingText");
    }

    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);
        _taskCompletionSource = new UniTaskCompletionSource();
        ChangeTextAsync().Forget();
    }

    public override async void CloseUI(bool isCloseAll = false)
    {
        if (_taskCompletionSource != null)
        {
            _taskCompletionSource.TrySetResult(); // 종료 신호 전달
            await _taskCompletionSource.Task; // 루프 종료까지 대기
        }

        base.CloseUI(isCloseAll); // 루프가 완전히 끝난 후 실행
    }

    private async UniTaskVoid ChangeTextAsync()
    {
        int count = 0;
        while (_taskCompletionSource.Task.Status != UniTaskStatus.Succeeded) // 무한 루프
        {
            _loadingText.text = TEXT;
            for (int i = 0; i < count % 3; i++)
            {
                _loadingText.text += '.';
            }

            await UniTask.Delay(500);
        }
    }
}
