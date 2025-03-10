using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class PlateSpawner : NetworkObjectSpawner
{
    [SerializeField] private ColorType _color;

    [SerializeField] private EventType[] _publishOnEnter;
    [SerializeField] private EventType[] _publishOnExit;

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
        _spawnedObject.GetComponent<PlateController>().Initialize(_color, _publishOnEnter, _publishOnExit);
    }
}
