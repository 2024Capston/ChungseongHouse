using UnityEngine;
using Unity.Netcode;
using System.Collections;
using Unity.VisualScripting;
using System.Runtime.CompilerServices;

/// <summary>
/// 큐브를 렌더링하는 Class
/// </summary>
public class CubeRenderer : NetworkBehaviour
{
    /// <summary>
    /// 렌더링에 쓰일 매터리얼. (파랑, 빨강 순)
    /// </summary>
    [SerializeField] Material[] _materials;

    private CubeController _cubeController;
    private NetworkInterpolator _networkInterpolator;
    private Outline _outline;

    private Transform[] _piecesTransforms;
    private MeshRenderer[] _piecesMeshRenderers;

    [ServerRpc(RequireOwnership = false)]
    private void UpdateColorServerRpc()
    {
        UpdateColorClientRpc();
    }

    [ClientRpc(RequireOwnership = false)]
    private void UpdateColorClientRpc()
    {
        bool originalOutline = false;

        // Outline 매터리얼까지 갱신되는 것을 막기 위해 잠시 비활성화
        if (_outline) {
            originalOutline = _outline.enabled;
            _outline.enabled = false;
        }

        int childCount = _piecesTransforms.Length;

        for (int i = 0; i < childCount; i++)
        {
            Material[] materials = _piecesMeshRenderers[i].materials;
            materials[1] = _materials[(int)_cubeController.Color - 1];
            _piecesMeshRenderers[i].materials = materials;
        }

        if (_outline) {
            _outline.enabled = originalOutline;
        }
    }

    public void Initialize()
    {
        _cubeController = GetComponent<CubeController>();
        _networkInterpolator = GetComponent<NetworkInterpolator>();

        _networkInterpolator.AddVisualReferenceDependantFunction(() =>
        {
            _outline = _networkInterpolator.VisualReference.GetComponent<Outline>();

            int childCount = _networkInterpolator.VisualReference.transform.childCount;
            _piecesTransforms = new Transform[childCount];
            _piecesMeshRenderers = new MeshRenderer[childCount];

            for (int i = 0; i < childCount; i++)
            {
                Transform child = _networkInterpolator.VisualReference.transform.GetChild(i);

                _piecesTransforms[i] = child.transform;
                _piecesMeshRenderers[i] = child.GetComponent<MeshRenderer>();

                Material[] materials = _piecesMeshRenderers[i].materials;
                materials[1] = _materials[(int)_cubeController.Color - 1];
                _piecesMeshRenderers[i].materials = materials;
            }
        });
    }

    /// <summary>
    /// 큐브의 렌더링 색깔을 갱신한다.
    /// </summary>
    public void UpdateColor()
    {
        UpdateColorServerRpc();
    }

    public void PlayTransitionAnimation(float transitionTime = 2f)
    {
        StartCoroutine("CoPlayTransitionAnimation", transitionTime);
    }

    private IEnumerator CoPlayTransitionAnimation(float transitionTime)
    {
        // Outline 매터리얼까지 갱신되는 것을 막기 위해 비활성화
        if (_outline) {
            _outline.enabled = false;
        }
        
        Material startMaterial = _materials[(int)_cubeController.Color - 1];
        Material targetMaterial = _materials[2 - (int)_cubeController.Color];

        float timer = 0f;
        float lastTime = Time.realtimeSinceStartup;

        // 1차 회전
        while (timer < transitionTime / 3f)
        {
            for (int i = 0; i < 4; i++)
            {
                _piecesTransforms[i].localRotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(0, 90, 0), timer * 3f / transitionTime);
                
                Material[] materials = _piecesMeshRenderers[0].materials;
                materials[1].Lerp(startMaterial, targetMaterial, timer * 3f / transitionTime);

                _piecesMeshRenderers[i].materials = materials;
            }

            yield return new WaitForSeconds(0.01f);

            timer += Time.realtimeSinceStartup - lastTime;
            lastTime = Time.realtimeSinceStartup;
        }

        for (int i = 0; i < 4; i++) {
            _piecesTransforms[i].localRotation = Quaternion.identity;
        }

        timer -= transitionTime / 3f;
        
        // 2차 회전전
        while (timer < transitionTime / 3f)
        {
            for (int i = 1; i < 8; i += 2)
            {
                _piecesTransforms[i].localRotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(90, 0, 0), timer * 3f / transitionTime);

                if (i == 5 || i == 7)
                {
                    Material[] materials = _piecesMeshRenderers[5].materials;
                    materials[1].Lerp(startMaterial, targetMaterial, timer * 3f / transitionTime);

                    _piecesMeshRenderers[i].materials = materials;
                }
            }

            yield return new WaitForSeconds(0.01f);

            timer += Time.realtimeSinceStartup - lastTime;
            lastTime = Time.realtimeSinceStartup;
        }

        for (int i = 1; i < 8; i += 2) {
            _piecesTransforms[i].localRotation = Quaternion.identity;
        }

        timer -= transitionTime / 3f;
        
        // 3차 회전
        while (timer < transitionTime / 3f)
        {
            for (int i = 4; i < 8; i++)
            {
                _piecesTransforms[i].localRotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(0, -90, 0), timer * 3f / transitionTime);

                if (i == 4 || i == 6)
                {
                    Material[] materials = _piecesMeshRenderers[4].materials;
                    materials[1].Lerp(startMaterial, targetMaterial, timer * 3f / transitionTime);

                    _piecesMeshRenderers[i].materials = materials;
                }
            }

            yield return new WaitForSeconds(0.01f);
            
            timer += Time.realtimeSinceStartup - lastTime;
            lastTime = Time.realtimeSinceStartup;
        }

        for (int i = 4; i < 8; i++) {
            _piecesTransforms[i].localRotation = Quaternion.identity;
        }
    }
}
