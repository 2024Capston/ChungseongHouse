using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Runtime.CompilerServices;

/// <summary>
/// 문을 나타내는 클래스.
/// </summary>
public class LaserGun : NetworkBehaviour, IInteractable
{
    /// <summary>
    /// 두 명이 함께 있어야 열리는지 여부.
    /// </summary>
    [SerializeField] private bool _isRequireBoth;
    /// <summary>
    /// 어떤 색깔 플레이어가 스위치를 눌러야 하는지.
    /// _isRequireBoth가 체크돼 있으면 이 변수는 무시한다.
    /// </summary>
    [SerializeField] private ColorType _switchColor;
    /// <summary>
    /// 두 명이 함께 있어야 할 때, 플레이어 인식 범위
    /// </summary>
    [SerializeField] private float _switchRange;


    //[SerializeField] private bool _isOpen;
    [SerializeField] private float _shotDuration=1;
    [SerializeField] private float _shotCooltime;
    [SerializeField] private float _angle;
    private Outline _outline;
    public Outline Outline
    {
        get => _outline;
        set => _outline = value;
    }
    private RenderLine _renderLine;
    private Transform _gunPoint; 


    public bool IsInteractable(PlayerController player)
    {
        return true;
    }
    public bool StartInteraction(PlayerController player)
    {
        DestroyWallServerRpc();
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

    private void Start()
    {
        _renderLine = GetComponent<RenderLine>();
        _gunPoint = transform.Find("RayStartPoiont");
    }
    public void Update()
    {
        if (!IsServer)
        {
            return;
        }
        if (_shotCooltime > 0)
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
        //// 문 여닫기 애니메이션이 진행 중인지 확인
        //bool isPlaying = _animator.GetCurrentAnimatorStateInfo(0).length > _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        Debug.Log("발사기 작동");
        if (_shotCooltime < 0)
        {
            _shotCooltime = _shotDuration;
            Vector3 rotation = transform.rotation.eulerAngles; // 현재 오일러 각도 가져오기
            Vector3 direction = Quaternion.Euler(rotation.x, rotation.y, -_angle) * Vector3.left;
            if (Physics.Raycast(_gunPoint.position, direction, out RaycastHit hit, Mathf.Infinity))
            {
                // 충돌 좌표를 가져옵니다.
                Debug.Log("발사기 작동작동");

                Vector3 collisionPoint = hit.point;
                _renderLine.StartCoroutine(_renderLine.DrawLay(collisionPoint));
                Debug.Log(hit.collider.gameObject);
                NetworkObject networkObject = hit.collider.gameObject.GetComponent<NetworkObject>();
                if (networkObject != null)
                {
                    networkObject.Despawn();
                }
            }
        }
    }

    [ClientRpc]
    private void DestroyWallClientRpc()
    {
    }
    [ClientRpc]
    private void InitializeClientRpc(ColorType color, Vector3 position, Quaternion rotation, Vector3 scale,
        float shotDuration, float shotCooltime, float angle)
    {
        //_wallRenderer.Initialize();
        transform.position = position;
        transform.rotation = rotation;
        transform.localScale = scale;

        // 나머지 변수 동기화
        _shotDuration = shotDuration;
        _shotCooltime = shotCooltime;
        _angle = angle;

        //Color = color;  // 색 변화시 함수 호출

        //if (PlayerController.LocalPlayer)
        //{
        //    RequestOwnership();
        //}
        //else
        //{
        //    PlayerController.LocalPlayerCreated += RequestOwnership;
        //}
    }
    public void Initialize(float shotDuration, float shotCooltime, float angle)
    {
        //InitializeClientRpc(color, transform.position, transform.rotation, transform.localScale
        //    , shotDuration, shotCooltime, angle);
        ColorType color = ColorType.None;   // 임시 채운용
        InitializeClientRpc(color, transform.position, transform.rotation, transform.localScale
            , shotDuration, shotCooltime, angle);

    }
    private void OnValidate()
    {
        Vector3 rotation = transform.localEulerAngles; // 로컬 오일러 각도 사용
        rotation.z = -_angle; // Z축 값만 변경
        transform.localEulerAngles = rotation; // 다시 적용
    }
    /// <summary>
    /// 주변(_switchRange)에 있는 플레이어의 수를 반환한다.
    /// </summary>
    private int GetNumberOfPlayersNearby()
    {
        PlayerController[] playerControllers = GameObject.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        int numberOfPlayersInRange = 0;

        foreach (PlayerController playerController in playerControllers)
        {
            float distance = Vector3.Distance(transform.position, playerController.transform.position);

            if (distance <= _switchRange)
            {
                numberOfPlayersInRange++;
            }
        }

        return numberOfPlayersInRange;
    }
}