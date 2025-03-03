using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 움직이는 플랫폼을 조작하는 Class
/// </summary>
public class PlatformController : NetworkBehaviour, IActivatable
{
    private struct Target{
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }

    /// <summary>
    /// 목표 위치 (오름차순으로 순서대로 이동)
    /// </summary>
    [SerializeField] private Target[] _targets;

    /// <summary>
    /// 이동 속력
    /// </summary>
    [SerializeField] private float _moveSpeed;

    private Rigidbody _rigidbody;

    private bool _isInitialized;
    private bool _isActive;
    private float _timer;

    private int _previousTarget = 0;    // 이전 목표 위치
    private int _currentTarget = 1;     // 현재 목표 위치
    private float _targetMoveTime;      // 이동에 걸릴 시간

    public override void OnNetworkSpawn()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!_isActive || !_isInitialized)
        {
            return;
        }

        _timer += Time.fixedDeltaTime;
        float lerpCoefficient = EaseInOut(_timer / _targetMoveTime);

        _rigidbody.MovePosition(Vector3.Lerp(_targets[_previousTarget].position, _targets[_currentTarget].position, lerpCoefficient));
        _rigidbody.MoveRotation(Quaternion.Slerp(_targets[_previousTarget].rotation, _targets[_currentTarget].rotation, lerpCoefficient));
        transform.localScale = Vector3.Lerp(_targets[_previousTarget].scale, _targets[_currentTarget].scale, lerpCoefficient);

        // 목표에 도달했으면 목표 위치 갱신
        if (Vector3.Distance(transform.position, _targets[_currentTarget].position) < 0.1f)
        {
            _timer = 0f;

            _previousTarget = _currentTarget;
            _currentTarget = (_currentTarget + 1) % _targets.Length;

            _targetMoveTime = (_targets[_currentTarget].position - _targets[_previousTarget].position).magnitude / _moveSpeed;

            // 서버 측에서 클라이언트 측으로 갱신 알림
            if (IsServer)
            {
                UpdatePlatformPositionClientRpc(transform.position, transform.rotation, transform.localScale, _currentTarget);
            }
        }
    }

    public bool IsActivatable(GameObject activator = null)
    {
        return true;
    }

    public bool Activate(GameObject activator = null)
    {
        SetActiveServerRpc(true);

        return true;
    }

    public bool Deactivate(GameObject activator = null)
    {
        SetActiveServerRpc(false);

        return true;
    }

    private float EaseInOut(float timer)
    {
        return -(Mathf.Cos(Mathf.PI * timer) - 1f) / 2f;
    }

    /// <summary>
    /// 활성화 여부를 설정한다.
    /// </summary>
    /// <param name="isActive">활성화 여부</param>
    [ServerRpc(RequireOwnership = false)]
    private void SetActiveServerRpc(bool isActive)
    {
        SetActiveClientRpc(isActive);
    }

    [ClientRpc]
    private void SetActiveClientRpc(bool isActive)
    {
        _isActive = isActive;
    }

    /// <summary>
    /// 서버 측에서 클라이언트 측으로 갱신 알림을 보낸다.
    /// </summary>
    /// <param name="position">갱신 위치</param>
    /// <param name="rotation">갱신 회전</param>
    /// <param name="currentTarget">갱신 목표 위치</param>
    [ClientRpc]
    private void UpdatePlatformPositionClientRpc(Vector3 position, Quaternion rotation, Vector3 scale, int currentTarget)
    {
        if (IsServer)
        {
            return;
        }

        _timer = 0f;

        _rigidbody.MovePosition(position);
        _rigidbody.MoveRotation(rotation);
        transform.localScale = scale;

        _currentTarget = currentTarget;

        if (_currentTarget == 0)
        {
            _previousTarget = _targets.Length - 1;
        }
        else
        {
            _previousTarget = _currentTarget - 1;
        }

        _targetMoveTime = (_targets[_currentTarget].position - _targets[_previousTarget].position).magnitude / _moveSpeed;
    }

    [ClientRpc]
    private void InitializeClientRpc(int targetsLength, float moveSpeed)
    {
        _moveSpeed = moveSpeed;
        _targets = new Target[targetsLength];
    }

    [ClientRpc]
    private void AppendTargetClientRpc(int index, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        _targets[index].position = position;
        _targets[index].rotation = rotation;
        _targets[index].scale = scale;

        if (index == 0)
        {
            _rigidbody.position = _targets[0].position;
            _rigidbody.rotation = _targets[0].rotation;
            transform.localScale = _targets[0].scale;
        }
        else if (index == 1)
        {
            _targetMoveTime = (_targets[_currentTarget].position - _targets[_previousTarget].position).magnitude / _moveSpeed;
        }

        if (index == _targets.Length - 1)
        {
            _isInitialized = true;
        }
    }

    public void Initialize(Transform[] targets, float moveSpeed)
    {
        InitializeClientRpc(targets.Length, moveSpeed);

        for (int i = 0; i < targets.Length; i++)
        {
            AppendTargetClientRpc(i, targets[i].position, targets[i].rotation, targets[i].localScale);
        }
    }
}
