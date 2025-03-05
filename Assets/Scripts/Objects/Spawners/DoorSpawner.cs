using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DoorSpawner : NetworkObjectSpawner
{
    [SerializeField] string _name;

    [SerializeField] bool _isTrigerrable = false;
    [SerializeField] bool _isOpen = false;

    private new void Awake()
    {
        base.Awake();

        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        _spawnedObject = Instantiate(_prefab);

        if (_name.Length > 0)
        {
            _spawnedObject.name = _name;
        }

        _spawnedObject.transform.position = transform.position;
        _spawnedObject.transform.rotation = transform.rotation;
        _spawnedObject.transform.localScale = transform.lossyScale;

        _spawnedObject.GetComponent<NetworkObject>().Spawn();
        _spawnedObject.GetComponent<DoorController>().Initialize(_isTrigerrable, _isOpen);
    }
}
