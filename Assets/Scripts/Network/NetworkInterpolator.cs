using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 위치 보간용으로 별도의 Visual 오브젝트를 생성하는 Class
/// </summary>
public class NetworkInterpolator : NetworkBehaviour
{
    /// <summary>
    /// 항상 Local 수준으로 보간할 지 여부
    /// </summary>
    [SerializeField] private bool _alwaysLocal = false;

    private NetworkInterpolatorUtil _networkInterpolatorUtil;
    private Action _visualReferenceCreated;

    /// <summary>
    /// 보간용 오브젝트
    /// </summary>
    private GameObject _visualReference;
    public GameObject VisualReference
    {
        get => _visualReference;
    }

    public override void OnNetworkSpawn()
    {
        _visualReference = new GameObject(gameObject.name + " (Visual)");

        _visualReference.transform.position = transform.position;
        _visualReference.transform.rotation = transform.rotation;
        _visualReference.transform.localScale = transform.localScale;

        MeshFilter meshFilter;
        MeshRenderer meshRenderer;

        // 시각 정보를 복사한다.
        if (TryGetComponent<MeshFilter>(out meshFilter) && TryGetComponent<MeshRenderer>(out meshRenderer))
        {
            _visualReference.AddComponent<MeshFilter>().mesh = meshFilter.mesh;
            _visualReference.AddComponent<MeshRenderer>().materials = meshRenderer.materials;
            Destroy(meshFilter);
            Destroy(meshRenderer);
        }

        foreach (Transform child in transform)
        {
            if (child.TryGetComponent<MeshFilter>(out meshFilter) && child.TryGetComponent<MeshRenderer>(out meshRenderer))
            {
                GameObject childVisual = new GameObject(child.name + " (Visual)");

                childVisual.transform.parent = _visualReference.transform;

                childVisual.transform.localPosition = child.localPosition;
                childVisual.transform.localRotation = child.localRotation;
                childVisual.transform.localScale = child.localScale;

                childVisual.AddComponent<MeshFilter>().mesh = meshFilter.mesh;
                childVisual.AddComponent<MeshRenderer>().materials = meshRenderer.materials;

                Destroy(meshFilter);
                Destroy(meshRenderer);
            }
        }

        // Outline 컴포넌트가 있다면 복사한다.
        if (TryGetComponent<Outline>(out Outline outline))
        {
            Outline newOutline = _visualReference.AddComponent<Outline>();

            newOutline.OutlineMode = outline.OutlineMode;
            newOutline.OutlineColor = outline.OutlineColor;
            newOutline.OutlineWidth = outline.OutlineWidth;

            Destroy(outline);
        }

        // Visual 오브젝트의 타겟을 지정한다.
        _networkInterpolatorUtil = _visualReference.AddComponent<NetworkInterpolatorUtil>();
        _networkInterpolatorUtil.SetTarget(transform, _alwaysLocal | IsOwner);

        _visualReferenceCreated?.Invoke();
    }

    public override void OnNetworkDespawn()
    {
        if (_visualReference is not null)
        {
            Destroy(_visualReference);
            _visualReferenceCreated = null;
        }
        
        base.OnNetworkDespawn();
    }

    /// <summary>
    /// Owner가 바뀌면 보간 속력을 조절한다.
    /// </summary>
    protected override void OnOwnershipChanged(ulong previous, ulong current)
    {
        _networkInterpolatorUtil.ChangeLerpSpeed(_alwaysLocal | IsOwner);
    }

    /// <summary>
    /// 보간 물체의 Transform을 즉시 설정한다.
    /// </summary>
    /// <param name="position">위치</param>
    /// <param name="rotation">회전</param>
    public void SetInstantTransform(Vector3 position, Quaternion rotation)
    {
        _networkInterpolatorUtil.transform.position = position;
        _networkInterpolatorUtil.transform.rotation = rotation;
    }

    /// <summary>
    /// 시각용 오브젝트가 생생된 직후 호출할 함수를 등록한다.
    /// </summary>
    /// <param name="action">등록할 함수</param>
    public void AddVisualReferenceDependantFunction(Action action)
    {
        if (_visualReference)
        {
            action.Invoke();
        }
        else
        {
            _visualReferenceCreated += action;
        }
    }
}
