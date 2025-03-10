using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 버튼을 렌더링하는 Class
/// </summary>
public class ButtonRenderer : MonoBehaviour
{
    /// <summary>
    /// 버튼 애니메이터
    /// </summary>
    [SerializeField] private Animator _animator;

    /// <summary>
    /// 버튼 조명 Mesh Renderer 및 매터리얼
    /// </summary>
    [SerializeField] private MeshRenderer _lightMeshRenderer;
    [SerializeField] private Material[] _lightMaterials;

    /// <summary>
    /// 버튼 유리 Mesh Renderer 및 매터리얼
    /// </summary>
    [SerializeField] private MeshRenderer[] _glassMeshRenderers;
    [SerializeField] private Material[] _glassMaterials;

    private ButtonController _buttonController;

    private void Start()
    {
        _buttonController = GetComponent<ButtonController>();
        SetButtonColor(_buttonController.Color);
    }

    /// <summary>
    /// 버튼에 새 색깔을 지정한다.
    /// </summary>
    /// <param name="newColor">새 색깔</param>
    public void SetButtonColor(ColorType newColor)
    {
        _lightMeshRenderer.material = _lightMaterials[(int)newColor];

        foreach (MeshRenderer glassMeshRenderer in _glassMeshRenderers)
        {
            glassMeshRenderer.material = _glassMaterials[(int)newColor];
        }
    }

    /// <summary>
    /// 애니메이터의 IsPressed 변수를 지정한다.
    /// </summary>
    public void SetAnimatorPressState(bool isPressed)
    {
        _animator.SetBool("IsPressed", isPressed);
    }

    /// <summary>
    /// 애니메이터의 IsToggled 트리거를 지정한다.
    /// </summary>
    public void SetAnimatorToggleTrigger()
    {
        _animator.SetTrigger("IsToggled");
    }
}
