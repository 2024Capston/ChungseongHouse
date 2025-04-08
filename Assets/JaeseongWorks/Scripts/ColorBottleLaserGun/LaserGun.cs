using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Runtime.CompilerServices;
using ColorWall;
using Unity.VisualScripting;

/// <summary>
/// 문을 나타내는 클래스.
/// </summary>
public class LaserGun : NetworkBehaviour, IInteractable
{
    [SerializeField] private Material[] _colorMaterials;
    /// <summary>
    /// 어떤 색깔 플레이어가 스위치를 눌러야 하는지.
    /// </summary>
    [SerializeField] private ColorType _color;
    ///// <summary>
    ///// 어떤 색깔의 오브젝트를 부술지.
    ///// </summary>
    //[SerializeField] private ColorType _color;


    //[SerializeField] private bool _isOpen;
    [SerializeField] private float _shotDuration=1;
    [SerializeField] private float _shotCooltime=0;
    [SerializeField] private float _angle;
    private Outline _outline;
    public Outline Outline
    {
        get => _outline;
        set => _outline = value;
    }
    private RenderLine _renderLine;
    [SerializeField] private Transform _gunPoint; 
    public Transform GunPoint { get=> _gunPoint; set=>_gunPoint=value; }


    public bool IsInteractable(PlayerController player)
    {
        return true;
    }
    public bool StartInteraction(PlayerController player)
    {
        if (player.Color == _color || _color==ColorType.Purple)
        {
            DestroyWallServerRpc();
        }
        return false;
    }
    public bool StopInteraction(PlayerController playerController)
    {
        return true;
    }

    public override void OnNetworkSpawn()
    {
        _shotCooltime = 0;
    }
    void Awake()
    {
        _renderLine = GetComponent<RenderLine>();
    }

    private void Start()
    {
        _outline = GetComponent<Outline>();
        _outline.enabled = false;
        _gunPoint = transform.Find("RayStartPoint");
    }
    public void Update()
    {
        if (!IsServer)
        {
            return;
        }
        if (_shotCooltime >= 0)
        {
            _shotCooltime -= Time.deltaTime;
        }
    }

    /// <summary>
    /// 서버 단에서 문을 연다.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void DestroyWallServerRpc()
    {
        DestroyWallClientRpc();
    }

    [ClientRpc]
    private void DestroyWallClientRpc()
    {
        //// 문 여닫기 애니메이션이 진행 중인지 확인
        //bool isPlaying = _animator.GetCurrentAnimatorStateInfo(0).length > _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        Debug.Log("발사기 작동");

        if (_shotCooltime < 0)
        {
            _shotCooltime = _shotDuration;
            Vector3 rotation = transform.rotation.eulerAngles; // 현재 오일러 각도 가져오기
            Vector3 direction = Quaternion.Euler(rotation.x, rotation.y, -_angle) * Vector3.left;
            //int layerMask = 1 << LayerMask.NameToLayer("Wall");
            Debug.Log($"발사기 좌표{_gunPoint.position}");
            if (Physics.Raycast(_gunPoint.position, direction, out RaycastHit hit, Mathf.Infinity))
            {
                // 충돌 좌표를 가져옵니다.
                Debug.Log("발사기 작동작동");

                if (hit.collider == null)
                {
                    Debug.LogError("Raycast가 충돌했지만 collider가 없음.");
                    return;
                }
                Vector3 collisionPoint = hit.point;
                _renderLine.StartCoroutine(_renderLine.DrawLay(collisionPoint));
                Debug.Log(hit.collider.gameObject);
                NetworkObject networkObject = hit.collider.gameObject.GetComponent<NetworkObject>();
                if (networkObject != null)
                {
                    DynamicColorWallController dynamicColorWallController = hit.collider.GetComponent<DynamicColorWallController>();
                    if (dynamicColorWallController == null) return;
                    if (dynamicColorWallController.Color == _color || dynamicColorWallController.Color == ColorType.Purple)
                    {
                        networkObject.Despawn();
                    }
                }
            }
        }
    }
    [ClientRpc]
    private void InitializeClientRpc(ColorType color, Vector3 position, Quaternion rotation, Vector3 scale
                        , float shotDuration, float shotCooltime, float angle)
    {
        //_wallRenderer.Initialize();
        transform.position = position;
        transform.rotation = rotation;
        transform.localScale = scale;

        // 나머지 변수 동기화
        _color = color;
        //_color = color;
        _shotDuration = shotDuration;
        _shotCooltime = shotCooltime;
        _angle = angle;

        //_renderLine.lineRenderer.materials[0] = _colorMaterials[(int)color];
        if (_colorMaterials.Length > 0)
        {
            Debug.Log($"색0:{_colorMaterials[0].name},색0:{_colorMaterials[1].name},색0:{_colorMaterials[2].name}");
            Debug.Log("색 추가됨!");
            //LineRenderer lineRenderer = _renderLine.lineRenderer; // 기존 배열 가져오기 //왜 null로 뜰까???/
            //Debug.Log($"LineREndere{lineRenderer}");
            //Material[] newMaterials = lineRenderer.materials; // 기존 배열 가져오기 //왜 null로 뜰까???/
            Material[] newMaterials = _renderLine.lineRenderer.materials; // 기존 배열 가져오기 //왜 null로 뜰까???/
            newMaterials[0] = _colorMaterials[(int)color]; // 첫 번째 머티리얼 변경
            _renderLine.lineRenderer.materials = newMaterials; // 새로운 배열 할당


            //_renderLine.lineRenderer.material = new Material(_colorMaterials[(int)_color]);
            //_renderLine.lineRenderer.materials[0] = _colorMaterials[(int)color];
        }

        //if (PlayerController.LocalPlayer)
        //{
        //    RequestOwnership();
        //}
        //else
        //{
        //    PlayerController.LocalPlayerCreated += RequestOwnership;
        //}
    }
    public void Initialize(ColorType color, float shotDuration, float _shotCooltime, float angle)
    {
        //InitializeClientRpc(color, transform.position, transform.rotation, transform.localScale
        //    , shotDuration, shotCooltime, angle);
        InitializeClientRpc(color, transform.position, transform.rotation, transform.localScale
            , shotDuration, _shotCooltime, angle);
        

    }
    private void OnValidate()
    {
        Vector3 rotation = transform.localEulerAngles; // 로컬 오일러 각도 사용
        rotation.z = -_angle; // Z축 값만 변경
        transform.localEulerAngles = rotation; // 다시 적용
        //if (_colorMaterials.Length > 0)
        //    _renderLine.lineRenderer.material = new Material(_colorMaterials[(int)_color]);
    }
}