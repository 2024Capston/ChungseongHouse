using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 오브젝트 Transform의 동기화를 담당하는 Class.
/// 오브젝트의 Owner가 상대방에게 Transform 정보를 전달한다.
/// </summary>
[RequireComponent(typeof(NetworkInterpolator))]
public class NetworkSyncTransform : NetworkBehaviour
{
    [SerializeField] private bool _parentUnderVisualReference = true;   // 부모의 시각용 오브젝트에 parenting할지 여부
    [SerializeField] private float _sendThreshold = 0.001f;             // transform 값을 전송하는 기준 값
    [SerializeField] private float _parentingCooldown = 0.5f;           // parenting 보간에 걸리는 시간

    private GameObject _parent;                      // 현재 오브젝트가 부모로 간주하는 오브젝트
    private GameObject _parentVisualReference;       // 현재 오브젝트가 부모로 간주하는 시각용 오브젝트
    private List<NetworkSyncTransform> _children;    // 현재 오브젝트가 자식으로 간주하는 오브젝트

    // transform 정보 관련
    private Vector3 _lastSyncedPosition;    // 마지막으로 주고 받은 위치
    private Quaternion _lastSyncedRotation; // 마지막으로 주고 받은 회전
    private Vector3 _lastSyncedScale;       // 마지막으로 주고 받은 스케일

    // parenting 보간 관련
    private bool _isParenting;              // parenting 보간 중인지 여부              
    private float _parentingTime;           // parenting 보간에 쓰이는 타이머

    public override void OnNetworkSpawn()
    {
        _children = new List<NetworkSyncTransform>();

        _lastSyncedPosition = transform.position;
        _lastSyncedRotation = transform.rotation;
        _lastSyncedScale = transform.localScale;
    }

    void Update()
    {
        if (!IsServer && !IsClient)
        {
            return;
        }

        if (_isParenting)
        {
            _parentingTime -= Time.deltaTime;
            
            if (_parentingTime < 0f)
            {
                _isParenting = false;
            }
        }

        if (IsOwner)
        {
            SendTransform();
        }
        else if (_lastSyncedPosition != null && _lastSyncedRotation != null && _lastSyncedScale != null)
        {
            UpdateFetchedTransform();
        }
    }

    /// <summary>
    /// 현재 transform을 상대방에게 전달한다.
    /// </summary>
    private void SendTransform()
    {
        Vector3 positionToSend = transform.position;

        // parent가 있는 있는 경우, 상대(relative) 위치를 전송.
        if (_parentVisualReference)
        {
            positionToSend -= _parentVisualReference.transform.position;
        }
        else if (_parent)
        {
            positionToSend -= _parent.transform.position;
        }

        float positionDiff = Vector3.Distance(transform.position, _lastSyncedPosition);
        float rotationDiff = Quaternion.Angle(transform.rotation, _lastSyncedRotation) / 10f;
        float scaleDiff = Vector3.Distance(transform.localScale, _lastSyncedScale);

        // transform 변화값이 기준을 넘어서는 경우에만 전송
        if (positionDiff < _sendThreshold && rotationDiff < _sendThreshold && scaleDiff < _sendThreshold)
        {
            return;
        }

        // 마지막으로 전송한 transform 값 저장
        _lastSyncedPosition = positionToSend;
        _lastSyncedRotation = transform.rotation;
        _lastSyncedScale = transform.localScale;

        if (IsServer)
        {
            SendTransformClientRpc(positionToSend, transform.rotation, transform.localScale);
        }
        else
        {
            SendTransformServerRpc(positionToSend, transform.rotation, transform.localScale);
        }
    }

    /// <summary>
    /// 상대방으로부터 받아온 transform을 적용한다.
    /// </summary>
    private void UpdateFetchedTransform()
    {
        Vector3 lastPosition = transform.position;

        if (_parentVisualReference)
        {
            transform.position = _lastSyncedPosition + _parentVisualReference.transform.position;
        }
        else if (_parent)
        {
            transform.position = _lastSyncedPosition + _parent.transform.position;
        }
        else
        {
            transform.position = _lastSyncedPosition;
        }

        transform.rotation = _lastSyncedRotation;
        transform.localScale = _lastSyncedScale;

        // parenting 중이라면 transform이 튀는 것을 방지
        if (_isParenting && Vector3.Distance(transform.position, lastPosition) > 100f)
        {
            transform.position = lastPosition;
        }
    }

    /// <summary>
    /// RPC를 통해 전달받은 parent를 설정한다. 필요하면 parenting 보간을 시작한다.
    /// </summary>
    /// <param name="parent"></param>
    private void UpdateFetchedParent(GameObject parent)
    {
        NetworkSyncTransform parentTransform;

        if (_parent && _parent.TryGetComponent<NetworkSyncTransform>(out parentTransform))
        {
            parentTransform.RemoveChild(this);
        }

        if (parent && parent.TryGetComponent<NetworkSyncTransform>(out parentTransform))
        {
            parentTransform.AddChild(this);
        }

        _parent = parent;
        _parentVisualReference = null;

        if (_parentUnderVisualReference && _parent)
        {
            _parent.TryGetComponent<NetworkInterpolator>(out NetworkInterpolator parentInterpolator);
            _parentVisualReference = parentInterpolator?.VisualReference;
        }

        StartParenting(_parentingCooldown);

        foreach (NetworkSyncTransform child in _children)
        {
            child.StartParenting(_parentingCooldown);
        }
    }

    /// <summary>
    /// 현재 오브젝트에 자식으로 간주할 오브젝트를 추가한다.
    /// </summary>
    /// <param name="child">자식으로 간주할 오브젝트</param>
    internal void AddChild(NetworkSyncTransform child)
    {
        if (!_children.Contains(child))
        {
            _children.Add(child);
        }
    }

    /// <summary>
    /// 현재 오브젝트에서 자식으로 간주할 오브젝트를 삭제한다.
    /// </summary>
    /// <param name="child">자식으로 간주할 오브젝트</param>
    internal void RemoveChild(NetworkSyncTransform child)
    {
        if (_children.Contains(child))
        {
            _children.Remove(child);
        }
    }

    /// <summary>
    /// 클라이언트에서 서버로 transform을 전달한다.
    /// </summary>
    /// <param name="position">위치</param>
    /// <param name="rotation">회전</param>
    /// <param name="scale">스케일</param>
    [ServerRpc(RequireOwnership = false)]
    private void SendTransformServerRpc(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        _lastSyncedPosition = position;
        _lastSyncedRotation = rotation;
        _lastSyncedScale = scale;
    }

    /// <summary>
    /// 서버에서 클라이언트로 transform을 전달한다.
    /// </summary>
    /// <param name="position">위치</param>
    /// <param name="rotation">회전</param>
    /// /// <param name="scale">스케일</param>
    [ClientRpc]
    private void SendTransformClientRpc(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        if (IsServer)
        {
            return;
        }

        _lastSyncedPosition = position;
        _lastSyncedRotation = rotation;
        _lastSyncedScale = scale;
    }

    /// <summary>
    /// 클라이언트에서 서버로 새 parent를 전달한다.
    /// </summary>
    /// <param name="parent">parent</param>
    [ServerRpc]
    private void SetParentServerRpc(NetworkObjectReference parent)
    {
        if (parent.TryGet(out NetworkObject networkObject))
        {
            UpdateFetchedParent(parent);
        }
    }

    /// <summary>
    /// 서버에서 클라이언트로 새 parent를 전달한다.
    /// </summary>
    /// <param name="parent">parent</param>
    [ClientRpc]
    private void SetParentClientRpc(NetworkObjectReference parent)
    {
        if (IsServer)
        {
            return;
        }

        if (parent.TryGet(out NetworkObject networkObject))
        {
            UpdateFetchedParent(parent);
        }
    }

    /// <summary>
    /// 클라이언트에서 서버로 parent 초기화를 요청한다.
    /// </summary>
    [ServerRpc]
    private void ResetParentServerRpc()
    {
        UpdateFetchedParent(null);
    }

    /// <summary>
    /// 서버에서 클라이언트로 parent 초기화를 요청한다.
    /// </summary>
    [ClientRpc]
    private void ResetParentClientRpc()
    {
        if (IsServer)
        {
            return;
        }

        UpdateFetchedParent(null);
    }

    /// <summary>
    /// parenting 보간을 시작한다.
    /// </summary>
    /// <param name="parentingCooldown"></param>
    internal void StartParenting(float parentingCooldown)
    {
        _isParenting = true;
        _parentingTime = parentingCooldown;
    }

    /// <summary>
    /// 오브젝트의 parent를 갱신한다. NULL로 설정하면 parent를 제거한다.
    /// </summary>
    /// <param name="parent">parent</param>
    public void SetParent(GameObject parent)
    {
        NetworkSyncTransform parentTransform;

        if (parent)
        {
            if (parent.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
            {
                if (_parent && _parent.TryGetComponent<NetworkSyncTransform>(out parentTransform))
                {
                    parentTransform.RemoveChild(this);
                }

                if (parent.TryGetComponent<NetworkSyncTransform>(out parentTransform))
                {
                    parentTransform.AddChild(this);
                }

                _parent = parent;
                _parentVisualReference = null;

                if (_parentUnderVisualReference && _parent)
                {
                    _parent.TryGetComponent<NetworkInterpolator>(out NetworkInterpolator parentInterpolator);
                    _parentVisualReference = parentInterpolator?.VisualReference;
                }

                if (IsServer)
                {
                    SetParentClientRpc(networkObject);
                }
                else
                {
                    SetParentServerRpc(networkObject);
                }
            }
            else
            {
                Logger.Log("Synced Parent must have Network Object as its component.");
            }
        }
        else
        {
            if (_parent && _parent.TryGetComponent<NetworkSyncTransform>(out parentTransform))
            {
                parentTransform.RemoveChild(this);
            }

            _parent = null;
            _parentVisualReference = null;

            if (IsServer)
            {
                ResetParentClientRpc();
            }
            else
            {
                ResetParentServerRpc();
            }
        }
    }
}
