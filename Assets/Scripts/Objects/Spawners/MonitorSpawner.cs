using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class MonitorSpawner : NetworkObjectSpawner
{
    [SerializeField] private MonitorType _defaultMonitorType;
    [SerializeField] private EventType[] _subscribeForMonitorUpdate;

    public override void SpawnObject()
    {
        base.SpawnObject();

        _spawnedObject = Instantiate(_prefab);

        _spawnedObject.transform.position = transform.position;
        _spawnedObject.transform.rotation = transform.rotation;
        _spawnedObject.transform.localScale = transform.lossyScale;

        Transform frontMonitorTransform = _spawnedObject.transform.Find("Monitor_Front").transform;
        frontMonitorTransform.rotation = transform.Find("Monitor_Front").rotation;

        _spawnedObject.GetComponent<NetworkObject>().Spawn();
        _spawnedObject.GetComponentInChildren<MonitorController>().Initialize(frontMonitorTransform.rotation, _defaultMonitorType, _subscribeForMonitorUpdate);
    }
}
