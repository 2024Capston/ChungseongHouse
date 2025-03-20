using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Runtime.CompilerServices;
using ColorWall;
using StageJS;

/// <summary>
/// 문을 나타내는 클래스.
/// </summary>
public class LaserGunSpawner : NetworkObjectSpawner
{
    //[SerializeField] private bool _isOpen;
    [SerializeField] private float _shotDuration;
    [SerializeField] private float _shotCooltime;
    [SerializeField] private float _angle;
    private RenderLine _renderLine;

    //[SerializeField] private Animator _animator;

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
        _spawnedObject.GetComponent<LaserGun>().Initialize(_shotDuration, _shotCooltime, _angle);
    }
}