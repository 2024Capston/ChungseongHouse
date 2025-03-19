using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine;


public class HomeManager : SingletonBehavior<HomeManager>
{
    
    private UserGameData _userGameData;
    public UserGameData UserGameData => _userGameData;

    public HomeUIController HomeUIController { get; private set; }

    private void Start()
    {
        _userGameData = UserDataManager.Instance.GetUserData<UserGameData>();
        UIManager.Instance.CloseAllOpenUI();
    }

    protected override void Init()
    {
        _isDestroyOnLoad = true;
        base.Init();
    }
}

