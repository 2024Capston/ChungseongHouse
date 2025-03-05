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
        List<GameObject> objectsToEnter =new List<GameObject>();
        List<GameObject> objectsToExit = new List<GameObject>();

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.GetComponent<PlayerController>() ||
                hit.collider.gameObject.GetComponent<CubeController>() ||
                hit.collider.gameObject.GetComponent<PossessableController>())
            {
                newObjects.Add(hit.collider.gameObject);

                if (!_objectsOnPlate.Contains(hit.collider.gameObject))
                {
                    objectsToEnter.Add(hit.collider.gameObject);
                }
            }
        }

        foreach (GameObject existingObjects in _objectsOnPlate)
        {
            if (!newObjects.Contains(existingObjects))
            {
                objectsToExit.Add(existingObjects);
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

        foreach(GameObject gameObject in objectsToEnter)
        {
            _eventsOnEnter.Invoke(this, gameObject);
        }

        foreach(GameObject gameObject in objectsToExit)
        {
            _eventsOnExit.Invoke(this, gameObject);
        }
    }

    public override void OnNetworkDespawn()
    {
        _eventsOnEnter = null;
        _eventsOnExit = null;
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

    /// <summary>
    /// 서버와 클라이언트의 초기 상태를 동기화한다. 이 함수는 서버와 클라이언트 모두에서 호출된다.
    /// </summary>
    /// <param name="color">색깔</param>
    [ClientRpc]
    private void InitializeClientRpc(ColorType color)
    {
        _color = color;

        Material[] materials = _lightMeshRenderer.materials;
        materials[1] = _materials[(int)_color];
        _lightMeshRenderer.materials = materials;
    }

    /// <summary>
    /// 발판 상태를 초기화하고 클라이언트와 동기화한다. 이 함수는 서버에서만 호출한다.
    /// </summary>
    /// <param name="color">색깔</param>
    public void Initialize(ColorType color)
    {
        InitializeClientRpc(color);

        _eventsOnEnter = new UnityEvent<PlateController, GameObject>();
        _eventsOnExit = new UnityEvent<PlateController, GameObject>();
    }

    /// <summary>
    /// 발판에 물체가 들어오면 호출될 이벤트를 추가한다.
    /// </summary>
    /// <param name="evn">이벤트</param>
    public void AddEventOnEnter(UnityAction<PlateController, GameObject> evn) {
        _eventsOnEnter.AddListener(evn);
    }

    /// <summary>
    /// 발판에서 물체가 나가면 호출될 이벤트를 추가한다.
    /// </summary>
    /// <param name="evn">이벤트</param>
    public void AddEventOnExit(UnityAction<PlateController, GameObject> evn)
    {
        _eventsOnExit.AddListener(evn);
    }
}
