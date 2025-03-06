using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class PlateSpawner : NetworkObjectSpawner
{
    [SerializeField] ColorType _color;

    [SerializeField] DelegateWrapper[] _eventsOnEnter;
    [SerializeField] DelegateWrapper[] _eventsOnExit;

    private new void Awake()
    {
        base.Awake();

        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        _spawnedObject = Instantiate(_prefab);

        _spawnedObject.transform.position = transform.position;
        _spawnedObject.transform.rotation = transform.rotation;
        _spawnedObject.transform.localScale = transform.lossyScale;

        _spawnedObject.GetComponent<NetworkObject>().Spawn();
        _spawnedObject.GetComponent<PlateController>().Initialize(_color);
    }

    private void Start()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        PlateController plateController = _spawnedObject.GetComponent<PlateController>();

        // 스폰한 발판에 이벤트를 추가
        foreach (DelegateWrapper action in _eventsOnEnter)
        {
            plateController.AddEventOnEnter((UnityAction<PlateController, GameObject>)action.GetDelegate());
        }

        foreach (DelegateWrapper action in _eventsOnExit)
        {
            plateController.AddEventOnExit((UnityAction<PlateController, GameObject>)action.GetDelegate());
        }
    }
}
