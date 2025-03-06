﻿using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : NetworkSingletonBehaviour<SceneLoader>
{
    
    private bool IsNetworkSceneManagementEnabled => NetworkManager != null && NetworkManager.SceneManager != null && NetworkManager.NetworkConfig.EnableSceneManagement;

    private bool m_IsInitialized;
    
    public virtual AsyncOperation LoadScene(string sceneName, bool useNetworkSceneManager, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        AsyncOperation loadOperation = null;
        
        if (useNetworkSceneManager)
        {
            if (IsSpawned && IsNetworkSceneManagementEnabled && !NetworkManager.ShutdownInProgress)
            {
                if (NetworkManager.IsServer)
                {
                    // If is active server and NetworkManager uses scene management, load scene using NetworkManager's SceneManager
                    NetworkManager.SceneManager.LoadScene(sceneName, loadSceneMode);
                }
            }
        }
        else
        {
            // Load using SceneManager
            loadOperation = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
        }

        return loadOperation;
    }

}