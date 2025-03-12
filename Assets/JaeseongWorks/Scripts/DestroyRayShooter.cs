using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Runtime.CompilerServices;

/// <summary>
/// 문을 나타내는 클래스.
/// </summary>
public class DestroyRayShooter : NetworkBehaviour, IActivatable
{
    //[SerializeField] private bool _isOpen;
    [SerializeField] private float _shotDuration;
    [SerializeField] private float _shotCooltime;
    [SerializeField] private float _angle;
    private RenderLine _renderLine;

    //[SerializeField] private Animator _animator;

    /// <summary>
    /// 문 열림 시간을 관리하기 위한 보조 변수.
    /// </summary>
    private float _timeTillClose;

    public override void OnNetworkSpawn()
    {
        //if (_isOpen)
        //{
        //    _animator.Play("Open");
        //}
        _shotCooltime = 0;
    }

    private void Start()
    {
        _renderLine = GetComponent<RenderLine>();        
    }
    public void Update()
    {
        if (!IsServer)
        {
            return;
        }
        _shotCooltime -= Time.deltaTime;
    }
    public bool IsActivatable(GameObject activator)
    {
        return true;
    }
    public bool Activate(GameObject activator)
    {
        DestroyWallServerRpc();
        return true;
    }

    public bool Deactivate(GameObject activator)
    {
        return true;
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
            if (Physics.Raycast(transform.position+ direction*4, direction, out RaycastHit hit, Mathf.Infinity))
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

    /// <summary>
    /// 클라이언트 단에서 문을 연다.
    /// 관련 변수를 각 클라이언트마다 갱신해 준다.
    /// </summary>
    [ClientRpc]
    private void DestroyWallClientRpc()
    {


    }
    //[ServerRpc(RequireOwnership = false)]
    //private void CloseDoorServerRpc()
    //{
    //    // 문 여닫기 애니메이션이 진행 중인지 확인
    //    bool isPlaying = _animator.GetCurrentAnimatorStateInfo(0).length > _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

    //    if (_isOpen && !isPlaying)
    //    {
    //        _animator.Play("Close");
    //        CloseDoorClientRpc();
    //    }
    //}

    ///// <summary>
    ///// 클라이언트 단에서 문을 닫는다.
    ///// 관련 변수를 각 클라이언트마다 갱신해 준다.
    ///// </summary>
    //[ClientRpc]
    //private void CloseDoorClientRpc()
    //{
    //    _isOpen = false;
    //}
    private void OnValidate()
    {
        Vector3 rotation = transform.localEulerAngles; // 로컬 오일러 각도 사용
        rotation.z = -_angle; // Z축 값만 변경
        transform.localEulerAngles = rotation; // 다시 적용
    }
}