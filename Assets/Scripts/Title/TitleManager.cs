using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class TitleManager : MonoBehaviour
{
    [SerializeField]
    private FacepunchTransport _facepunch;

    private AsyncOperation _asyncOperation;

    private static readonly uint APP_ID = 480;

    private bool _isSteamClientInitialized;

    private ProgressBar _progressBar;

    private void Awake()
    {
        _isSteamClientInitialized = false;
        InitSteamClient();
    }

    private void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        _progressBar = root.Q<ProgressBar>("ProgressBar");
        
        if (_isSteamClientInitialized)
        {
            LoadUserData();

            StartCoroutine(CoLoadHome());
        }
        
        AudioManager.Instance.ApplyAudioMixerValues();
    }

    /// <summary>
    /// SteamClient를 Initialize한다. 성공하면 _isSteamClientInitialized가 true가 된다.
    /// </summary>
    private void InitSteamClient()
    {
        try
        {
            SteamClient.Init(APP_ID, false);
            _isSteamClientInitialized = true;
            _facepunch.InitSteamworks();
        }
        catch (Exception e)
        {
            Logger.LogError($"[{nameof(FacepunchTransport)}] - Caught an exeption during initialization of Steam client: {e}");

            // 팝업에 대한 정보를 넣는다.
            ConfirmUIData confirmUIData = new ConfirmUIData()
            {
                ConfirmType = ConfirmType.OK,
                TitleText = "네트워크 연결 실패",
                ParagraphText = $"[{nameof(FacepunchTransport)}] - Caught an exeption during initialization of Steam client: {e}",
                OKButtonText = "종료",
                OnClickOKButton = () =>
                {
                    Application.Quit();
                }
            };

            // Confirm 팝업을 연다.
            UIManager.Instance.OpenUI<ConfirmUI>(confirmUIData);
        }
    }

    /// <summary>
    /// UserData를 불러온다.
    /// </summary>
    private void LoadUserData()
    {
        UserDataManager.Instance.LoadUserData();

        // 저장된 UserData가 없으면 신규 유저이므로 기본값을 넣어준다.
        if (!UserDataManager.Instance.HasUserData)
        {
            UserDataManager.Instance.SetDefaultUserData();
            UserDataManager.Instance.SaveUserData();
        }
    }

    /// <summary>
    /// Home Scene을 Load하기 위한 Coroutine
    /// </summary>
    /// <returns></returns>
    private IEnumerator CoLoadHome()
    {
        Logger.Log($"{GetType()}::CoLoadHome");

        _progressBar.title = "0%";
        _progressBar.value = 0;

        // NetworkManager Instance가 생성될 때까지 대기한다.
        yield return new WaitUntil(() => NetworkManager.Singleton != null);

        _progressBar.title = "10%";
        _progressBar.value = 10;

        // Home Scene을 비동기적으로 불러오기 위해 시도한다.
        _asyncOperation = SceneLoadManager.Instance.LoadScene(SceneType.Home.ToString(), false);

        if (_asyncOperation == null)
        {
            Logger.LogError("Lobby async loading error");
            yield break;
        }

        // asyncOperation이 완료되었을 때 progress를 0.9에서 정지시킨다.
        _asyncOperation.allowSceneActivation = false;

        // asyncOperation이 완료될 때까지 반복한다.
        while (!_asyncOperation.isDone)
        {
            _progressBar.title = $"{(int)((_asyncOperation.progress < 0.5f ? 0.5f : _asyncOperation.progress) * 100)}%";
            _progressBar.value = (int)((_asyncOperation.progress < 0.5f ? 0.5f : _asyncOperation.progress) * 100);

            // asyncOperation이 완료되었다면 
            if (_asyncOperation.progress >= 0.9f)
            {

                // 약간 대기 한 뒤 Home Scene으로 넘어간다.
                _progressBar.title = "100%";
                _progressBar.value = 100;
                yield return new WaitForSecondsRealtime(0.5f);

                _asyncOperation.allowSceneActivation = true;

                Logger.Log($"UserName : {Steamworks.SteamClient.Name}\nUserID : {SteamClient.SteamId}");
                yield break;
            }

            yield return null;
        }
    }
}