using System.Collections;
using System.Collections.Generic;
using Possessable;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PossessableSpawner : NetworkObjectSpawner
{
    [SerializeField] ColorType _color;

    public override void SpawnObject()
    {
        base.SpawnObject();

        _spawnedObject = Instantiate(_prefab);

        _spawnedObject.transform.position = transform.position;
        _spawnedObject.transform.rotation = transform.rotation;
        _spawnedObject.transform.localScale = transform.lossyScale;

        _spawnedObject.GetComponent<NetworkObject>().Spawn();
        _spawnedObject.GetComponent<PossessableController>().Initialize(_color);
    }
}
