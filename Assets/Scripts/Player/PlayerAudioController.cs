using UnityEngine;

public class PlayerAudioController : MonoBehaviour
{
    [SerializeField] AudioSource _footstepAudioSource;
    [SerializeField] AudioSource _jumpAudioSource;
    [SerializeField] AudioSource _landAudioSource;

    [SerializeField] AudioClip[] _footstepClips;

    private SkinnedMeshRenderer _meshRenderer;
    private Animator _animator;

    private float _footStepAudioCooldown;

    private void Start()
    {
        _meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (_footStepAudioCooldown > 0f)
        {
            _footStepAudioCooldown -= Time.deltaTime;
        }
    }

    public void PlayFootstepSound()
    {
        if (_meshRenderer.enabled && _footStepAudioCooldown <= 0f)
        {
            if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Land Blend Tree") && _animator.GetFloat("Velocity") < 0.1f)
            {
                return;
            }

            _footstepAudioSource.clip = _footstepClips[Random.Range(0, _footstepClips.Length)];
            _footstepAudioSource.Play();
            _footStepAudioCooldown = 0.1f;
        }
    }

    public void PlayJumpSound()
    {
        _jumpAudioSource.Play();
    }

    public void PlayLandSound()
    {
        _landAudioSource.Play();
    }
}
