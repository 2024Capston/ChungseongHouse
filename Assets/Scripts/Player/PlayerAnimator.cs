using TMPro;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] Transform _headTarget;
    [SerializeField] Transform _headCenter;

    [SerializeField] Transform _leftArmTarget;
    [SerializeField] Transform _rightArmTarget;

    [SerializeField] Rig _leftArmRig;
    [SerializeField] Rig _rightArmRig;

    private PlayerController _playerController;
    private Animator _animator;

    private Vector3 _headInterpolator;
    private Vector3 _leftArmInterpolator;
    private Vector3 _rightArmInterpolator;

    private float _leftWeightInterpolator;
    private float _rightWeightInterpolator;

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

        _animator.SetFloat("Velocity", velocity.magnitude / 80f, 0.1f, Time.deltaTime);
        _animator.SetFloat("Angular Velocity", _playerController.AngularVelocity.y / 360f, 0.1f, Time.deltaTime);

        _animator.SetFloat("Side Velocity", velocity.x / 80f, 0.1f, Time.deltaTime);
        _animator.SetFloat("Forward Velocity", velocity.z / 80f, 0.1f, Time.deltaTime);

        _animator.SetBool("IsJumping", _playerController.IsJumping);
        _animator.SetBool("IsGrounded", _playerController.IsGrounded);

        _headTarget.position = Vector3.Lerp(_headTarget.position, _headInterpolator, 32f * Time.deltaTime);

        _leftArmTarget.position = Vector3.Lerp(_leftArmTarget.position, _leftArmInterpolator, 32f * Time.deltaTime);
        _rightArmTarget.position = Vector3.Lerp(_rightArmTarget.position, _rightArmInterpolator, 32f * Time.deltaTime);

        _leftArmRig.weight = Mathf.Lerp(_leftArmRig.weight, _leftWeightInterpolator, 16f * Time.deltaTime);
        _rightArmRig.weight = Mathf.Lerp(_rightArmRig.weight, _rightWeightInterpolator, 16f * Time.deltaTime);
    }

    public void SetPlayerController(PlayerController playerController)
    {
        _playerController = playerController;
    }

    public void SetHeadTarget(Vector3 cameraForward)
    {
        _headInterpolator = _headCenter.position + cameraForward * 64f;
    }

    public void SetArmTarget(ArmType armType, Vector3 targetPosition)
    {
        if (armType == ArmType.LeftArm || armType == ArmType.BothArms)
        {
            _leftArmInterpolator = targetPosition;
        }

        if (armType == ArmType.RightArm || armType == ArmType.BothArms)
        {
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
            _rightWeightInterpolator = weight;
        }
    }

    public void PlayFootstepSound()
    {
        print("Footstep");
    }

    public void PlayJumpSound()
    {
        print("Jump");
    }

    public void PlayLandSound()
    {
        print("Land");
    }
}
