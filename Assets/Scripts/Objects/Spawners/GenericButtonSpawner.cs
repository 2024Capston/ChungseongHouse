using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class GenericButtonSpawner : NetworkObjectSpawner
{
    [SerializeField] private ColorType _color;
    [SerializeField] private ButtonType _buttonType;

    [SerializeField] private GameObject[] _activatables;

    [SerializeField] private float _temporaryCooldown;
    [SerializeField] private bool _requiresBoth;
    [SerializeField] private float _detectionRadius;

    [SerializeField] private DelegateWrapper[] _events;

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
        _spawnedObject.GetComponentInChildren<GenericButtonController>().Initialize(_color, _buttonType, _temporaryCooldown, _requiresBoth, _detectionRadius);
    }

    private void Start()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        GenericButtonController buttonController = _spawnedObject.GetComponentInChildren<GenericButtonController>();

        // 스폰한 버튼에 Activatable을 추가
        foreach (GameObject activatable in _activatables)
        {
            Type type = Type.GetType("IActivatable");

            // 대상 오브젝트도 Network Object Spawner로 스폰된 경우
            if (activatable.TryGetComponent<NetworkObjectSpawner>(out NetworkObjectSpawner networkObjectSpawner))
            {
                buttonController.AddActivatable(networkObjectSpawner.SpawnedObject.GetComponentInChildren(type).gameObject);
            }
            else
            {
                buttonController.AddActivatable(activatable.GetComponentInChildren(type).gameObject);
            }
        }

        // 스폰한 버튼에 이벤트를 추가
        foreach (DelegateWrapper action in _events)
        {
            buttonController.AddEvent((UnityAction)action.GetAction());
        }
    }
}
