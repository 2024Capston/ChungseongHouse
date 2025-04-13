using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DoorSpawner : NetworkObjectSpawner
{
    [SerializeField] string _name;

    [SerializeField] bool _isTrigerrable = false;
    [SerializeField] bool _isOpen = false;

    [SerializeField] EventType[] _subscribeForActivation;
    [SerializeField] EventType[] _subscribeForDeactivation;
    [SerializeField] EventType[] _subscribeForSetOpen;
    [SerializeField] EventType[] _subscribeForSetClose;

    public override void SpawnObject()
    {
        base.SpawnObject();

        _spawnedObject = Instantiate(_prefab);

        if (_name.Length > 0)
        {
            _spawnedObject.name = _name;
        }

        _spawnedObject.transform.position = transform.position;
        _spawnedObject.transform.rotation = transform.rotation;
        _spawnedObject.transform.localScale = transform.lossyScale;

        _spawnedObject.GetComponent<NetworkObject>().Spawn();
        _spawnedObject.GetComponent<DoorController>().Initialize(_isTrigerrable, _isOpen, _subscribeForActivation, _subscribeForDeactivation, _subscribeForSetOpen, _subscribeForSetClose);
    }
}
