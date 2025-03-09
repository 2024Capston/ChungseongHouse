using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlatformSpawner : NetworkObjectSpawner
{
    [SerializeField] private Transform[] _targets;
    [SerializeField] private float _moveSpeed;

    private void Start()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        _spawnedObject = Instantiate(_prefab);

        _spawnedObject.transform.position = transform.position;
        _spawnedObject.transform.rotation = transform.rotation;
        _spawnedObject.transform.localScale = transform.lossyScale;

        _spawnedObject.GetComponent<NetworkObject>().Spawn();
        _spawnedObject.GetComponent<PlatformController>().Initialize(_targets, _moveSpeed);
    }
}