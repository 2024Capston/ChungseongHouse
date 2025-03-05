using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 네트워크 오브젝트를 스폰하는 Class
/// </summary>
public class NetworkObjectSpawner : MonoBehaviour
{
    /// <summary>
    /// 스폰할 프리팹
    /// </summary>
    [SerializeField] protected GameObject _prefab;

    /// <summary>
    /// 스폰된 오브젝트
    /// </summary>
    protected GameObject _spawnedObject;
    public GameObject SpawnedObject
    {
        get => _spawnedObject;
    }

    protected void Awake()
    {
        // 기존에 MeshRenderer나 MeshFilter가 있다면 제거
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();

        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            Destroy(meshRenderer);
        }

        foreach (MeshFilter meshFilter in meshFilters)
        {
            Destroy(meshFilter);
        }
    }

    private void OnDestroy()
    {
        // 파괴 시, 스폰했던 오브젝트도 제거
        if (_spawnedObject)
        {
            _spawnedObject.GetComponent<NetworkObject>().Despawn();
        }
    }
}
