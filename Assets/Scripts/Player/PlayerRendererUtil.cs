using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Rendering;

/// <summary>
/// 생성된 플레이어 리깅 모델을 관리하는 Class
/// </summary>
public class PlayerRendererUtil : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer _bodyMeshRenderer;
    [SerializeField] private SkinnedMeshRenderer _eyesMeshRenderer;

    [SerializeField] private Transform _headTarget;
    [SerializeField] private Transform _headCenter;

    [SerializeField] private Transform _leftArmTarget;
    [SerializeField] private Transform _rightArmTarget;

    [SerializeField] private Rig _leftArmRig;
    [SerializeField] private Rig _rightArmRig;

    private PlayerController _playerController;
    private Animator _animator;

    // 목표 위치 보간용
    private Vector3 _headInterpolator;
    private Vector3 _leftArmInterpolator;
    private Vector3 _rightArmInterpolator;

    // 목표 Weight 보간용
    private float _leftWeightInterpolator;
    private float _rightWeightInterpolator;

    // Coroutine에서 Weight 변경 도중에 SetArmWeight() 함수가 호출된 경우를 확인
    private bool _touchInterrupted;

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        _headInterpolator = _headTarget.position;

        _leftArmInterpolator = _leftArmTarget.position;
        _rightArmInterpolator = _rightArmTarget.position;

        _leftWeightInterpolator = _leftArmRig.weight;
        _rightWeightInterpolator = _rightArmRig.weight;
    }

    private void Update()
    {
        Vector3 velocity = _playerController.Velocity;
        velocity.y = 0;
        velocity = Quaternion.Inverse(transform.rotation) * velocity;

        if (_playerController.MoveInput.x == 0)
        {
            velocity.x = 0;
        }

        if (_playerController.MoveInput.z == 0)
        {
            velocity.z = 0;
        }

        _animator.SetFloat("Velocity", velocity.magnitude / 80f, 0.1f, Time.deltaTime);
        _animator.SetFloat("Angular Velocity", _playerController.AngularVelocity.y / 360f, 0.1f, Time.deltaTime);

        _animator.SetFloat("Side Velocity", velocity.x / 80f, 0.1f, Time.deltaTime);
        _animator.SetFloat("Forward Velocity", velocity.z / 80f, 0.1f, Time.deltaTime);

        _animator.SetBool("IsJumping", _playerController.IsJumping);
        _animator.SetBool("IsGrounded", _playerController.IsGrounded);

        // 목표 위치로 target 오브젝트 보간
        _headTarget.position = Vector3.Lerp(_headTarget.position, _headInterpolator, 32f * Time.deltaTime);

        _leftArmTarget.position = Vector3.Lerp(_leftArmTarget.position, _leftArmInterpolator, 32f * Time.deltaTime);
        _rightArmTarget.position = Vector3.Lerp(_rightArmTarget.position, _rightArmInterpolator, 32f * Time.deltaTime);

        // 목표 값으로 Rig Weight 보간
        _leftArmRig.weight = Mathf.Lerp(_leftArmRig.weight, _leftWeightInterpolator, 16f * Time.deltaTime);
        _rightArmRig.weight = Mathf.Lerp(_rightArmRig.weight, _rightWeightInterpolator, 16f * Time.deltaTime);
    }

    public void SetPlayerController(PlayerController playerController)
    {
        _playerController = playerController;
    }

    public void SetHeadTarget(Vector3 forward)
    {
        _headInterpolator = _headCenter.position + forward * 64f;
    }

    public void SetArmTarget(ArmType armType, Vector3 targetPosition)
    {
        if (armType == ArmType.LeftArm || armType == ArmType.BothArms)
        {
            _leftArmInterpolator = targetPosition;
        }

        if (armType == ArmType.RightArm || armType == ArmType.BothArms)
        {
            _touchInterrupted = true;
            _rightArmInterpolator = targetPosition;
        }
    }

    public void SetArmWeight(ArmType armType, float weight)
    {
        if (armType == ArmType.LeftArm || armType == ArmType.BothArms)
        {
            _leftWeightInterpolator = weight;
        }

        if (armType == ArmType.RightArm || armType == ArmType.BothArms)
        {
            _touchInterrupted = true;
            _rightWeightInterpolator = weight;
        }
    }

    public void PlayTouchAnimation(Vector3 touchPosition)
    {
        StartCoroutine(CoPlayTouchAnimation(touchPosition));
    }

    private IEnumerator CoPlayTouchAnimation(Vector3 touchPosition)
    {
        _rightArmInterpolator = touchPosition;
        _rightWeightInterpolator = 1.0f;

        _touchInterrupted = false;

        yield return new WaitForSeconds(0.2f);

        if (!_touchInterrupted)
        {
            _rightWeightInterpolator = 0.0f;
        }
    }

    /// <summary>
    /// 로컬 플레이어의 메쉬를 그림자만 표시한다. (1인칭 카메라를 위해)
    /// </summary>
    public void HideFirstPersonPlayerRender()
    {
        _bodyMeshRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
        _eyesMeshRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
    }

    /// <summary>
    /// 플레이어를 메쉬를 표시한다.
    /// </summary>
    public void ShowPlayerRender()
    {
        _bodyMeshRenderer.enabled = true;
        _eyesMeshRenderer.enabled = true;
    }

    /// <summary>
    /// 플레이어 메쉬를 숨긴다.
    /// </summary>
    public void HidePlayerRender()
    {
        _bodyMeshRenderer.enabled = false;
        _eyesMeshRenderer.enabled = false;
    }
}
