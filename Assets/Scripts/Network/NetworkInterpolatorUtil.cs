using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 보간용으로 생성된 Visual 오브젝트의 보간을 수행하는 Class
/// </summary>
public class NetworkInterpolatorUtil : MonoBehaviour
{
    // Local 수준으로 빠르게 보간하거나, Remote 수준으로 느리게 보간할 수 있다
    private const float LOCAL_LERP_SPEED = 16f;
    private const float OTHER_LERP_SPEED = 8f;

    private Transform _target;

    private float _lerpSpeed = LOCAL_LERP_SPEED;    // 현재 보간 속력

    void Update()
    {
        if (_target)
        {
            transform.position = Vector3.Lerp(transform.position, _target.position, Time.deltaTime * _lerpSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, _target.rotation, Time.deltaTime * _lerpSpeed);
            transform.localScale = Vector3.Lerp(transform.localScale, _target.localScale, Time.deltaTime * _lerpSpeed);
        }
    }

    /// <summary>
    /// 보간 목표를 설정한다.
    /// </summary>
    /// <param name="target">목표</param>
    /// <param name="isLocal">보간 속력</param>
    public void SetTarget(Transform target, bool isLocal)
    {
        _target = target;

        if (isLocal)
        {
            _lerpSpeed = LOCAL_LERP_SPEED;
        }
        else
        {
            _lerpSpeed = OTHER_LERP_SPEED;
        }
    }

    /// <summary>
    /// 보간 속력을 갱신한다.
    /// </summary>
    /// <param name="isLocal">보간 속력</param>
    public void ChangeLerpSpeed(bool isLocal)
    {
        if (isLocal)
        {
            _lerpSpeed = LOCAL_LERP_SPEED;
        }
        else
        {
            _lerpSpeed = OTHER_LERP_SPEED;
        }
    }
}
