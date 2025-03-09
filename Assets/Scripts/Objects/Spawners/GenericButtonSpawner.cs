using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class GenericButtonSpawner : NetworkObjectSpawner
{
    [SerializeField] private ColorType _color;
    [SerializeField] private ButtonType _buttonType;

    [SerializeField] private float _temporaryCooldown;
    [SerializeField] private bool _requiresBoth;
    [SerializeField] private float _detectionRadius;

    [SerializeField] private EventType[] _publishOnPress;
    [SerializeField] private EventType[] _publishOnUnpress;
    [SerializeField] private EventType[] _subscribeForEnable;
    [SerializeField] private EventType[] _subscribeForDisable;

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
        _spawnedObject.GetComponentInChildren<GenericButtonController>().Initialize(_color, _buttonType, _temporaryCooldown, _requiresBoth, _detectionRadius, _publishOnPress, _publishOnUnpress, _subscribeForEnable, _subscribeForDisable);
    }
}
