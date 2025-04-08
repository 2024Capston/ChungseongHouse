using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlatformSpawner : NetworkObjectSpawner
{
    [SerializeField] private Transform[] _targets;
    [SerializeField] private float _moveSpeed;

    [SerializeField] private EventType[] _subscribeForActivation;
    [SerializeField] private EventType[] _subscribeForDeactivation;

    public override void SpawnObject()
    {
        base.SpawnObject();

        _spawnedObject = Instantiate(_prefab);

        _spawnedObject.transform.position = transform.position;
        _spawnedObject.transform.rotation = transform.rotation;
        _spawnedObject.transform.localScale = transform.lossyScale;

        _spawnedObject.GetComponent<NetworkObject>().Spawn();
        _spawnedObject.GetComponent<PlatformController>().Initialize(_targets, _moveSpeed, _subscribeForActivation, _subscribeForDeactivation);
    }
}