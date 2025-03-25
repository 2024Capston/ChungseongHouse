using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 네트워크 테스트용 Class
/// </summary>
public class NetworkTestUI : MonoBehaviour
{
    [SerializeField] Button _hostButton;
    [SerializeField] Button _clientButton;

    void Start()
    {
        _hostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();

            _hostButton.gameObject.SetActive(false);
            _clientButton.gameObject.SetActive(false);
        });

        _clientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();

            _hostButton.gameObject.SetActive(false);
            _clientButton.gameObject.SetActive(false);
        });
    }
}
