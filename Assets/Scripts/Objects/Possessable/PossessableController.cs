using UnityEngine;
using Unity.Netcode;

namespace Possessable
{
    /// <summary>
    /// 빙의 가능한 물체를 조작하는 Class
    /// </summary>
    public class PossessableController : NetworkBehaviour, IInteractable
    {
        /// <summary>
        /// 물체의 색깔
        /// </summary>
        private ColorType _color;
        public ColorType Color
        {
            get => _color;
            set => _color = value;
        }

        private Rigidbody _rigidbody;
        private Collider _collider;

        private NetworkInterpolator _networkInterpolator;
        private MeshRenderer[] _meshRenderers;

        private float _transparentThreshold;    // 물체와 카메라의 거리가 이것보다 적으면 투명화 시작

        // 빙의한 플레이어에 대한 레퍼런스
        private PlayerController _interactingPlayer;
        private Rigidbody _interactingRigidbody;
        private PlayerRenderer _interactingPlayerRenderer;
        private CameraController _interactingCameraController;

        private Outline _outline;
        public Outline Outline
        {
            get => _outline;
            set => _outline = value;
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();

            _networkInterpolator = GetComponent<NetworkInterpolator>();
            _networkInterpolator.AddVisualReferenceDependantFunction(() =>
            {
                _outline = _networkInterpolator.VisualReference.GetComponent<Outline>();
                _outline.enabled = false;
            });

            _transparentThreshold = (_collider.bounds.extents.x * transform.localScale.x + _collider.bounds.extents.z * transform.localScale.z) / 2f;
        }

        void Update()
        {
            if (!IsOwner)
            {
                return;
            }

            // 빙의 상태에선 플레이어의 위치로 계속 이동
            if (_interactingPlayer)
            {
                transform.position = _interactingPlayer.transform.position;
                transform.rotation = _interactingPlayer.transform.rotation;

                float distance = Vector3.Distance(transform.position, Camera.main.transform.position);

                if (distance < _transparentThreshold)
                {
                    SetAlphaValue(distance / _transparentThreshold);
                }
                else
                {
                    SetAlphaValue(1.0f);
                }
            }
        }

        /// <summary>
        /// 빙의를 해제할 때 플레이어가 들어갈 자리가 있는지 파악하고, 있다면 그곳으로 이동시킨다.
        /// </summary>
        /// <returns>빙의 해제 가능 여부</returns>
        private bool CheckDispossessionPosition(PlayerController player)
        {
            // 콜라이더 값을 사용하기 위해 잠시 활성화
            _collider.enabled = true;

            Vector3 origin = player.transform.position;

            origin.y -= _collider.bounds.extents.y;
            origin.y += PlayerController.INITIAL_CAPSULE_HEIGHT / 2f * player.transform.localScale.y;

            Vector3 offset = Vector3.up * (PlayerController.INITIAL_CAPSULE_HEIGHT / 2f * player.transform.localScale.y - PlayerController.INITIAL_CAPSULE_RADIUS * player.transform.localScale.x) * 0.9f;
            float radius = _collider.bounds.size.x + PlayerController.INITIAL_CAPSULE_RADIUS * player.transform.localScale.x;

            // 물체를 중심으로, 주변을 원으로 탐색한다.
            for (int i = 0; i < 9; i++)
            {
                Vector3 newPoint;

                // 정면으로부터 0~180도 회전
                newPoint = origin + Quaternion.Euler(0, i * 20, 0) * transform.forward * radius;

                if (Physics.OverlapCapsule(newPoint + offset, newPoint - offset, PlayerController.INITIAL_CAPSULE_RADIUS * player.transform.localScale.x).Length == 0)
                {
                    if (!Physics.Raycast(transform.position, newPoint - transform.position, (newPoint - transform.position).magnitude))
                    {
                        _interactingRigidbody.MovePosition(newPoint);
                        _collider.enabled = false;

                        return true;
                    }
                }

                // 정면으로부터 -180~0도 회전
                newPoint = origin + Quaternion.Euler(0, -i * 20, 0) * transform.forward * radius;

                if (Physics.OverlapCapsule(newPoint + offset, newPoint - offset, PlayerController.INITIAL_CAPSULE_RADIUS * player.transform.localScale.x).Length == 0)
                {
                    if (!Physics.Raycast(transform.position, newPoint - transform.position, (newPoint - transform.position).magnitude))
                    {
                        _interactingRigidbody.MovePosition(newPoint);
                        _collider.enabled = false;

                        return true;
                    }
                }
            }

            _collider.enabled = false;
            return false;
        }

        public bool IsInteractable(PlayerController player)
        {
            return _color == player.Color;
        }

        public bool StartInteraction(PlayerController player)
        {
            _interactingPlayer = player;
            _interactingRigidbody = player.GetComponent<Rigidbody>();

            _interactingCameraController = player.GetComponent<CameraController>();
            _interactingCameraController.ChangeCameraMode(false);

            // Local 환경과 Remote 환경에서 상태를 갱신한다.
            StartPossession(player);

            if (IsServer)
            {
                StartPossessionClientRpc(player.gameObject);
            }
            else
            {
                StartPossessionServerRpc(player.gameObject);
            }

            _interactingRigidbody.MovePosition(transform.position);
            _interactingRigidbody.MoveRotation(transform.rotation);

            return true;
        }

        public bool StopInteraction(PlayerController player)
        {
            // 빙의 해제 후 들어갈 여유 공간이 있다면
            if (CheckDispossessionPosition(player))
            {
                _interactingCameraController.ChangeCameraMode(true);
                _interactingCameraController = null;

                _interactingRigidbody = null;

                // Local 환경과 Remote 환경에서 상태를 갱신한다.
                StopPossession(player);

                if (IsServer)
                {
                    StopPossessionClientRpc(player.gameObject);
                }
                else
                {
                    StopPossessionServerRpc(player.gameObject);
                }

                _interactingPlayer = null;

                SetAlphaValue(1.0f);

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 빙의 물체의 알파 값을 설정한다. [0, 1]
        /// </summary>
        /// <param name="alphaValue">알파 값</param>
        private void SetAlphaValue(float alphaValue)
        {
            foreach (MeshRenderer meshRenderer in _meshRenderers)
            {
                Material[] materials = meshRenderer.materials;
                foreach (Material material in materials)
                {
                    if (material.HasProperty("_Alpha"))
                    {
                        material.SetFloat("_Alpha", alphaValue);
                    }
                }
            }
        }

        /// <summary>
        /// 플레이어 색깔에 따라 빙의 물체의 소유권을 요청한다.
        /// </summary>
        private void RequestOwnership()
        {
            if (_color == PlayerController.LocalPlayer.Color)
            {
                if (!IsHost)
                {
                    RequestOwnershipServerRpc(NetworkManager.LocalClientId);
                }
            }
            else
            {
                _rigidbody.isKinematic = true;
            }
        }

        /// <summary>
        /// 색깔이 같은 물체에 대해 서버에 Ownership을 요청한다.
        /// </summary>
        /// <param name="clientId">요청하는 플레이어 ID</param>
        [ServerRpc(RequireOwnership = false)]
        private void RequestOwnershipServerRpc(ulong clientId)
        {
            GetComponent<NetworkObject>().ChangeOwnership(clientId);
        }

        /// <summary>
        /// 빙의를 시작한다.
        /// </summary>
        /// <param name="player">빙의할 플레이어</param>
        private void StartPossession(PlayerController player)
        {
            _interactingPlayerRenderer = player.GetComponent<PlayerRenderer>();
            _interactingCameraController = player.GetComponent<CameraController>();

            // 플레이어의 Mesh, Material 갱신
            _interactingPlayerRenderer.HidePlayerRender();

            // 물체는 일시적으로 비활성화
            _rigidbody.isKinematic = true;
            _collider.enabled = false;

            // 플레이어의 Collider 정보 갱신
            player.UpdateCollider(_collider, transform.localScale);
        }

        /// <summary>
        /// 빙의를 중단한다.
        /// </summary>
        /// <param name="player">빙의 중단할 플레이어</param>
        private void StopPossession(PlayerController player)
        {
            // 플레이어의 Mesh, Material 복구
            _interactingPlayerRenderer.ShowPlayerRender();

            // 물체 다시 활성화
            _rigidbody.isKinematic = false;
            _rigidbody.velocity = Vector3.zero;
            _collider.enabled = true;

            // 플레이어의 Collider 정보 갱신
            player.UpdateCollider(null, Vector3.one);

            _interactingPlayerRenderer = null;
            _interactingCameraController = null;
        }

        /// <summary>
        /// 클라이언트에서 서버로 빙의 시작을 전달한다.
        /// </summary>
        /// <param name="player">빙의할 플레이어</param>
        [ServerRpc]
        private void StartPossessionServerRpc(NetworkObjectReference player)
        {
            if (player.TryGet(out NetworkObject networkObject))
            {
                StartPossession(networkObject.GetComponent<PlayerController>());
            }
        }

        /// <summary>
        /// 서버에서 클라이언트로 빙의 시작을 전달한다.
        /// </summary>
        /// <param name="player">빙의할 플레이어</param>
        [ClientRpc]
        private void StartPossessionClientRpc(NetworkObjectReference player)
        {
            if (IsServer)
            {
                return;
            }

            if (player.TryGet(out NetworkObject networkObject))
            {
                StartPossession(networkObject.GetComponent<PlayerController>());
            }
        }

        /// <summary>
        /// 클라이언트에서 서버로 빙의 중단을 전달한다.
        /// </summary>
        /// <param name="player">빙의 중단할 플레이어</param>
        [ServerRpc]
        private void StopPossessionServerRpc(NetworkObjectReference player)
        {
            if (player.TryGet(out NetworkObject networkObject))
            {
                StopPossession(networkObject.GetComponent<PlayerController>());
            }
        }

        /// <summary>
        /// 서버에서 클라이언트로 빙의 중단을 전달한다.
        /// </summary>
        /// <param name="player">빙의 중단할 플레이어</param>
        [ClientRpc]
        private void StopPossessionClientRpc(NetworkObjectReference player)
        {
            if (IsServer)
            {
                return;
            }

            if (player.TryGet(out NetworkObject networkObject))
            {
                StopPossession(networkObject.GetComponent<PlayerController>());
            }
        }

        /// <summary>
        /// 서버와 클라이언트의 초기 상태를 동기화한다. 이 함수는 서버와 클라이언트 모두에서 호출된다.
        /// </summary>
        /// <param name="color">색깔</param>
        /// <param name="position">위치</param>
        /// <param name="rotation">회전</param>
        /// <param name="scale">스케일</param>
        [ClientRpc]
        private void InitializeClientRpc(ColorType color, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            _color = color;
            _networkInterpolator.AddVisualReferenceDependantFunction(() =>
            {
                _meshRenderers =  _networkInterpolator.VisualReference.GetComponentsInChildren<MeshRenderer>();

                foreach (MeshRenderer meshRenderer in _meshRenderers)
                {
                    Material[] materials = meshRenderer.materials;
                    foreach (Material material in materials)
                    {
                        material.SetObjectColor(color);
                    }
                }
            });

            _rigidbody.MovePosition(position);
            _rigidbody.MoveRotation(rotation);
            transform.localScale = scale;

            if (PlayerController.LocalPlayer)
            {
                RequestOwnership();
            }
            else
            {
                PlayerController.LocalPlayerCreated += RequestOwnership;
            }
        }

        /// <summary>
        /// 빙의 물체 상태를 초기화하고 클라이언트와 동기화한다. 이 함수는 서버에서만 호출한다.
        /// </summary>
        /// <param name="color">색깔</param>
        public void Initialize(ColorType color)
        {
            InitializeClientRpc(color, transform.position, transform.rotation, transform.localScale);
        }
    }
}