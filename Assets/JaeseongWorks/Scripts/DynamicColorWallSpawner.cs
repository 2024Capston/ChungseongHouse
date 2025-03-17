using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.PlayerLoop;
using ColorWall;

public class DynamicColorWallSpawner : NetworkObjectSpawner
{
    // 각종 설정 체크 변수
    [SerializeField] private MovementType _movementType;
    [SerializeField] private MoveDirection _moveDirection;
    [SerializeField] private float _movingSpeed = 1f;
    [SerializeField] private float _moveDistance = 10f;

    [SerializeField] private bool _canSeeOtherColor = false;
    [SerializeField] private CollisionHandleType _handleSameColor;
    [SerializeField] private CollisionHandleType _handleDiffrentColor;

    [SerializeField] ColorType _color;

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
        _spawnedObject.GetComponent<DynamicColorWallController>().Initialize(_color
        ,_movementType,_moveDirection,_movingSpeed,_moveDistance,_canSeeOtherColor,_handleSameColor,_handleDiffrentColor);
    }
}
