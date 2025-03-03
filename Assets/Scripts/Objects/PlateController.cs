using UnityEngine;
using Unity.Netcode;

using UnityEngine.Events;
using System.Collections.Generic;
using Unity.VisualScripting;

/// <summary>
/// 발판을 조작하는 Class
/// </summary>
public class PlateController : NetworkBehaviour
{
    /// <summary>
    /// 발판 옆에 표시될 빛 색깔
    /// </summary>
    [SerializeField] ColorType _color;

    /// <summary>
    /// 발판에 물체가 들어오면 호출할 이벤트
    /// </summary>
    [SerializeField] UnityEvent<PlateController, GameObject> _eventsOnEnter;

    /// <summary>
    /// 발판에서 물체가 나가면 호출할 이벤트
    /// </summary>
    [SerializeField] UnityEvent<PlateController, GameObject> _eventsOnExit;

    /// <summary>
    /// 발판 옆의 빛에 대한 레퍼런스
    /// </summary>
    [SerializeField] MeshRenderer _lightMeshRenderer;

    /// <summary>
    /// 발판 옆의 빛에 대한 매터리얼
    /// </summary>
    [SerializeField] Material[] _materials;

    private Animator _animator;
    private BoxCollider _boxCollider;

    private List<GameObject> _objectsOnPlate;
    public List<GameObject> ObjectsOnPlate
    {
        get => _objectsOnPlate;
    }

    public override void OnNetworkSpawn()
    {
        _animator = GetComponent<Animator>();

        if (IsServer)
        {
            _boxCollider = GetComponent<BoxCollider>();
            _objectsOnPlate = new List<GameObject>();
        }
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        RaycastHit[] hits = Physics.BoxCastAll(transform.position, _boxCollider.bounds.extents, Vector3.up, transform.rotation, 1f);
        List<GameObject> newObjects = new List<GameObject>();

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.GetComponent<PlayerController>() ||
                hit.collider.gameObject.GetComponent<CubeController>() ||
                hit.collider.gameObject.GetComponent<PossessableController>())
            {
                newObjects.Add(hit.collider.gameObject);

                if (!_objectsOnPlate.Contains(hit.collider.gameObject))
                {
                    _eventsOnEnter?.Invoke(this, hit.collider.gameObject);
                }
            }
        }

        foreach (GameObject existingObjects in _objectsOnPlate)
        {
            if (!newObjects.Contains(existingObjects))
            {
                _eventsOnExit?.Invoke(this, existingObjects);
            }
        }

        if (_objectsOnPlate.Count == 0 && newObjects.Count > 0)
        {
            SetPressStateClientRpc(true);
        }
        else if (_objectsOnPlate.Count > 0 && newObjects.Count == 0)
        {
            SetPressStateClientRpc(false);
        }

        _objectsOnPlate = newObjects;
    }

    [ClientRpc]
    private void SetPressStateClientRpc(bool isPressed)
    {
        if (isPressed)
        {
            _animator.SetBool("IsPressed", true);
        }
        else
        {
            _animator.SetBool("IsPressed", false);
        }
    }

    [ClientRpc]
    private void InitializeClientRpc(ColorType color)
    {
        _color = color;

        Material[] materials = _lightMeshRenderer.materials;
        materials[1] = _materials[(int)_color];
        _lightMeshRenderer.materials = materials;
    }

    public void Initialize(ColorType color, UnityEvent<PlateController, GameObject> eventsOnEnter, UnityEvent<PlateController, GameObject> eventsOnExit)
    {
        InitializeClientRpc(color);

        _eventsOnEnter = eventsOnEnter;
        _eventsOnExit = eventsOnExit;
    }
}
