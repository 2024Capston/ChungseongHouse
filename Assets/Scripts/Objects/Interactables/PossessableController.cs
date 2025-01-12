using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PossessableController : PlayerDependantBehaviour, IInteractable
{
    [SerializeField] private ColorType _possessableColor;
    public ColorType PossessableColor
    {
        get => _possessableColor;
        set => _possessableColor = value;
    }

    [SerializeField] private Material[] _materials;

    private Rigidbody _rigidbody;
    private BoxCollider _boxCollider;

    private NetworkSyncTransform _networkSyncTransform;
    private NetworkInterpolator _networkInterpolator;

    private PlayerController _interactingPlayer;
    private PlayerRenderer _interactingPlayerRenderer;
    private CharacterController _characterController;
    private Rigidbody _platform;

    private Outline _outline;
    public Outline Outline
    {
        get => _outline;
        set => _outline = value;
    }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _boxCollider = GetComponent<BoxCollider>();

        _networkSyncTransform = GetComponent<NetworkSyncTransform>();
        _networkInterpolator = GetComponent<NetworkInterpolator>();

        _networkInterpolator.AddVisualReferenceDependantFunction(() =>
        {
            _outline = _networkInterpolator.VisualReference.GetComponent<Outline>();
            _outline.enabled = false;
        });

        GetComponent<MeshRenderer>().material = _materials[(int)_possessableColor - 1];
    }

    public override void OnPlayerInitialized()
    {
        if (_possessableColor == PlayerController.LocalPlayer.PlayerColor && IsClient)
        {
            RequestOwnershipServerRpc(NetworkManager.LocalClientId);
        }
        else
        {
            _rigidbody.isKinematic = true;
        }
    }

    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (_interactingPlayer)
        {
            transform.position = _interactingPlayer.transform.position;
            transform.rotation = _interactingPlayer.transform.rotation;
        }
        else
        {
            HandlePlatform();
        }
    }

    private void HandlePlatform()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 4f))
        {
            if (_platform?.gameObject != hit.collider.gameObject)
            {
                // 새로운 플랫폼을 발견한 경우
                if (hit.collider.gameObject.TryGetComponent<NetworkObject>(out NetworkObject networkObject) &&
                    hit.collider.gameObject.TryGetComponent<Rigidbody>(out _platform))
                {
                    _networkSyncTransform.SetParent(networkObject.gameObject);
                }
                else if (_platform != null)
                {
                    _networkSyncTransform.SetParent(null);
                    _platform = null;
                }
            }
        }
        else if (_platform != null)
        {
            _networkSyncTransform.SetParent(null);
            _platform = null;
        }
    }

    private bool CheckDispossessionPosition()
    {
        Vector3 origin = _characterController.transform.position;
        origin.y -= _boxCollider.size.y;
        origin.y += PlayerController.INITIAL_CAPSULE_HEIGHT;

        Vector3 verticalPad = Vector3.up * PlayerController.INITIAL_CAPSULE_HEIGHT / 2f;
        float radius = (_boxCollider.size.x / 2f + PlayerController.INITIAL_CAPSULE_RADIUS) * 1.2f;

        for (int i = 0; i < 9; i++)
        {
            Vector3 newPoint;

            newPoint = origin + Quaternion.Euler(0, i * 20, 0) * transform.forward * radius;

            if (Physics.OverlapCapsule(newPoint + verticalPad, newPoint - verticalPad, PlayerController.INITIAL_CAPSULE_RADIUS).Length == 0)
            {
                _characterController.enabled = false;
                _characterController.transform.position = newPoint;
                _characterController.enabled = true;

                return true;
            }

            newPoint = origin + Quaternion.Euler(0, -i * 20, 0) * transform.forward * radius;

            if (Physics.OverlapCapsule(newPoint + verticalPad, newPoint - verticalPad, PlayerController.INITIAL_CAPSULE_RADIUS).Length == 0)
            {
                _characterController.enabled = false;
                _characterController.transform.position = newPoint;
                _characterController.enabled = true;

                return true;
            }
        }

        return false;
    }

    public bool IsInteractable(PlayerController player)
    {
        return _possessableColor == player.PlayerColor;
    }

    public bool StartInteraction(PlayerController player)
    {
        _interactingPlayer = player;
        _interactingPlayerRenderer = player.GetComponent<PlayerRenderer>();
        _characterController = player.GetComponent<CharacterController>();

        _rigidbody.isKinematic = true;
        _boxCollider.enabled = false;

        player.UpdateCollider(_boxCollider);

        _characterController.enabled = false;
        _characterController.transform.position = transform.position;
        _characterController.transform.rotation = transform.rotation;
        _characterController.enabled = true;

        _interactingPlayerRenderer.HidePlayerRenderServerRpc();

        return true;
    }

    public bool StopInteraction(PlayerController player)
    {
        if (CheckDispossessionPosition())
        {
            _interactingPlayerRenderer.ShowPlayerRenderServerRpc();

            _interactingPlayer = null;
            _interactingPlayerRenderer = null;
            _characterController = null;

            _rigidbody.isKinematic = false;
            _rigidbody.velocity = Vector3.zero;
            _boxCollider.enabled = true;

            player.UpdateCollider(null);

            return true;
        }
        else
        {
            return false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestOwnershipServerRpc(ulong clientId)
    {
        GetComponent<NetworkObject>().ChangeOwnership(clientId);
    }
}
