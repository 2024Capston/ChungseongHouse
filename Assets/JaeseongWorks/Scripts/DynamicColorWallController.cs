using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Burst.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace ColorWall
{
    enum MoveDirection { Up, Down, Left, Right, Forward, Back } // 벽 이동 방향
    enum MovementType { None, Linear, Oscillating }   //벽움직임의 종류
    enum CollisionHandleType { Passable, Blocked, Bouncy, Deadly}
    public class DynamicColorWallController : NetworkBehaviour
    {
        // 각종 설정 체크 변수
        [SerializeField] private MeshRenderer _colorMeshRenderer;
        [SerializeField] private MovementType _movementType;
        [SerializeField] private MoveDirection _moveDirection;
        [SerializeField] private float _movingSpeed = 1f;
        [SerializeField] private float _moveDistance = 10f;

        [SerializeField] private bool _canSeeOtherColor = false;
        [SerializeField] private CollisionHandleType _handleSameColor;
        [SerializeField] private CollisionHandleType _handleDiffrentColor;

        /// <summary>
        /// Inspector 상에서 초기 색깔을 설정하는 데 쓰이는 변수
        /// </summary>
        [SerializeField] private ColorType _initColor;

        private Vector3 _spawnedPosition;
        private Rigidbody _rigidbody;
        private BoxCollider _boxCollider;
        private MeshRenderer[] _meshRenderers;

        // 자식 오브젝트 변수
        private Transform _childWall;

        // 플레이어 조작에 쓰이는 보조 변수
        private Vector3 _pastPosition;
        private float _pitchAngle;
        private Vector3 _movingVector;

        private float _turnedTime = 0;


        [SerializeField] private NetworkVariable<ColorType> _color = new NetworkVariable<ColorType>();
        public NetworkVariable<ColorType> Color
        {
            get => _color;
        }
        private void Awake()
        {
            _childWall = transform.Find("Color_Wall");
            _colorMeshRenderer = _childWall.GetComponent<MeshRenderer>();
        }
        public override void OnNetworkSpawn()
        {
            if (IsServer && _initColor == ColorType.None)
            {
                _initColor = (ColorType)UnityEngine.Random.Range(1, 4);
            }
            _color.Value = _initColor;
            _meshRenderers = GetComponentsInChildren<MeshRenderer>();
            _rigidbody = GetComponent<Rigidbody>();
            _boxCollider = GetComponent<BoxCollider>();

            // 큐브의 색깔이 변하면 함수 호출하도록 지정
            _color.OnValueChanged += (ColorType before, ColorType after) => {
                OnWallColorChanged(before, after);
            };
            // 큐브 최초 생성 후 초기화 작업을 수행
            // MultiplayerManager의 LocalPlayer를 참조하므로, 해당 변수가 지정될 때까지 대기
            //if (MultiplayerManager.Instance.LocalPlayer == null)
            //{
            //    MultiplayerManager.LocalPlayerSet.AddListener(() =>
            //    {
            //        _color.OnValueChanged.Invoke(_color.Value, _color.Value);
            //    });
            //}
            //else
            //{
            //    _color.OnValueChanged.Invoke(_color.Value, _color.Value);
            //}
            _color.OnValueChanged.Invoke(_color.Value, _color.Value);
        }
        private void Start()
        {
            _meshRenderers = GetComponentsInChildren<MeshRenderer>();
            _rigidbody = GetComponent<Rigidbody>();
            _boxCollider = GetComponent<BoxCollider>();
            _spawnedPosition = transform.position;
            _movingVector = Vector3.zero;
            // 방향에 따라 이동 벡터 설정
            switch (_moveDirection)
            {
                case MoveDirection.Up:
                    _movingVector = Vector3.up;
                    break;
                case MoveDirection.Down:
                    _movingVector = Vector3.down;
                    break;
                case MoveDirection.Left:
                    _movingVector = Vector3.left;
                    break;
                case MoveDirection.Right:
                    _movingVector = Vector3.right;
                    break;
                case MoveDirection.Forward:
                    _movingVector = Vector3.forward;
                    break;
                case MoveDirection.Back:
                    _movingVector = Vector3.back;
                    break;
            }
            _turnedTime = 0;
        }

        private void Update()
        {
            // 위치는 (서버)에 의해서만 갱신되도록 한다
            if (!IsServer)
            {
                return;
            }
            switch (_movementType)
            {
                case MovementType.None:
                    break;
                case MovementType.Linear:
                    Debug.Log($"movingVector{_movingVector}");
                    transform.position += _movingVector * _movingSpeed * Time.deltaTime;
                    if (Vector3.Distance(transform.position, _spawnedPosition) >= _moveDistance)
                    {
                        Destroy(gameObject);
                    }
                    break;
                case MovementType.Oscillating:
                    if (Vector3.Distance(transform.position, _spawnedPosition) >= _moveDistance && _turnedTime <= 0)
                    {
                        _movingVector = -_movingVector;
                        transform.position = _pastPosition;
                        _turnedTime += 0.1f;
                    }
                    transform.position += _movingVector * _movingSpeed * Time.deltaTime;
                    break;
            }
            if (_turnedTime > 0) _turnedTime -= Time.deltaTime;


            _pastPosition = transform.position;

        }
        /// <summary>
        /// 색깔을 갱신한다.
        /// </summary>
        /// <param name="before">변경 전 색깔</param>
        /// <param name="after">변경 후 색깔</param>
        private void OnWallColorChanged(ColorType before, ColorType after)
        {
            Color newColor = (after == ColorType.Red) ? new Color(1, 0, 0) 
                            : (after == ColorType.Blue) ? new Color(0, 0, 1) 
                            : (after == ColorType.Purple) ? new Color(1, 0, 1)
                            : new Color(0, 0, 0);

            int newLayer = (after == ColorType.Red) ? LayerMask.NameToLayer("Red") : (after == ColorType.Blue) ? LayerMask.NameToLayer("Blue") : LayerMask.NameToLayer("Purple");
            //int excludedLayer = (after == ColorType.Red) ? LayerMask.GetMask("Blue") : LayerMask.GetMask("Red");

            // 색깔이 다른 물체는 투명도 추가로 추가    // IsHost==True는 P가 Blue란뜻
            if ((IsHost && _color.Value == ColorType.Blue) || (!IsHost && _color.Value == ColorType.Red) ||
                (_color.Value == ColorType.Purple))   // 현 옵젝과 플레이어의 색이 같다면
            {
                if (_handleSameColor == CollisionHandleType.Passable)
                {
                    GetComponent<BoxCollider>().enabled = false;
                }
                else
                {
                    if (GetComponent<BoxCollider>().enabled == false) GetComponent<BoxCollider>().enabled = true;
                }
                newColor.a = 0.7f;  //반투명
            }
            else   // 현 옵젝과 플레이어의 색이 다르다면
            {
                if (_handleDiffrentColor == CollisionHandleType.Passable)
                {
                    GetComponent<BoxCollider>().enabled = false;
                }
                else
                {
                    if (GetComponent<BoxCollider>().enabled == false) GetComponent<BoxCollider>().enabled = true;
                }
                newColor.a = 0.0f;  //투명으로
                _colorMeshRenderer.enabled = _canSeeOtherColor;
            }

            _colorMeshRenderer.material.color = newColor;
            

            //gameObject.layer = newLayer;
            // 다른 색깔 물체와는 물리 상호작용하지 않도록 지정
            //_boxCollider.excludeLayers = excludedLayer;

        }

        /// <summary>
        /// 서버와 클라이언트의 오브젝트 상태를 동기화한다.
        /// </summary>
        /// <param name="color">색깔</param>
        /// <param name="position">위치</param>
        /// <param name="rotation">회전</param>
        /// <param name="scale">스케일</param>
        [ClientRpc]
        private void InitializeClientRpc(ColorType color, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            _color = color;
            _wallRenderer.Initialize();

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


        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log($"충돌!");
            if (collision == null) return;
            GameObject collisionObject = collision.gameObject;
            Rigidbody collisionRB = collision.gameObject.GetComponent<Rigidbody>();

            if (collisionObject.tag == "Player")
            {
                if ((IsHost && _color.Value == ColorType.Blue) || (!IsHost && _color.Value == ColorType.Red))   // 현 옵젝과 플레이어의 색이 같다면
                {
                    switch (_handleSameColor)
                    {
                        case CollisionHandleType.Blocked:
                            break;
                        case CollisionHandleType.Passable:  //passble이었으면 애초에 충돌이 안나긴함 ㅎㅎ;
                            break;
                        case CollisionHandleType.Bouncy:
                            collisionRB.AddForce((collision.transform.position - transform.position).normalized * 50);
                            break;
                        case CollisionHandleType.Deadly:
                            GameObject spawnPoint = null;
                            if (collisionObject.layer == LayerMask.NameToLayer("Red"))
                            {

                                spawnPoint = GameObject.FindWithTag("Red Spawn Point");
                            }
                            else if (collisionObject.layer == LayerMask.NameToLayer("Blue"))
                            {
                                spawnPoint = GameObject.FindWithTag("Blue Spawn Point");
                            }
                            if (spawnPoint != null)
                            {
                                collisionRB.MovePosition(spawnPoint.transform.position);
                                //collisionObject.transform.position = spawnPoint.transform.position;
                            }
                            spawnPoint = GameObject.FindWithTag("Red Spawn Point");
                            Vector3 hardSpawnPoint = new Vector3(0, 1000, 0);
                            //collisionObject.transform.position = hardSpawnPoint;
                            //collisionRB.position = hardSpawnPoint;
                            Debug.Log(collisionObject.transform.position);
                            //collisionRB.MovePosition(hardSpawnPoint);
                            collisionObject.GetComponent<CharacterController>().Move(hardSpawnPoint - collisionObject.transform.position);
                            Debug.Log(collisionObject.transform.position);

                            break;
                    }
                }
                else   // 현 옵젝과 플레이어의 색이 다르다면
                {
                    switch (_handleDiffrentColor)
                    {
                        case CollisionHandleType.Blocked:
                            break;
                        case CollisionHandleType.Passable:  //passble이었으면 애초에 충돌이 안나긴함 ㅎㅎ;
                            break;
                        case CollisionHandleType.Bouncy:
                            collisionRB.AddForce((collision.transform.position - transform.position).normalized * 50);
                            break;
                        case CollisionHandleType.Deadly:
                            GameObject spawnPoint = null;
                            if (collisionObject.layer == LayerMask.NameToLayer("Red"))
                            {

                                spawnPoint = GameObject.FindWithTag("Red Spawn Point");
                            }
                            else if (collisionObject.layer == LayerMask.NameToLayer("Blue"))
                            {
                                spawnPoint = GameObject.FindWithTag("Blue Spawn Point");
                            }
                            if (spawnPoint != null)
                            {
                                collisionRB.MovePosition(spawnPoint.transform.position);
                                //collisionObject.transform.position = spawnPoint.transform.position;
                            }
                            spawnPoint = GameObject.FindWithTag("Red Spawn Point");
                            Vector3 hardSpawnPoint = new Vector3(0, 1000, 0);
                            //collisionObject.transform.position = hardSpawnPoint;
                            //collisionRB.position = hardSpawnPoint;
                            Debug.Log(collisionObject.transform.position);
                            //collisionRB.MovePosition(hardSpawnPoint);
                            collisionObject.GetComponent<CharacterController>().Move(hardSpawnPoint - collisionObject.transform.position);
                            Debug.Log(collisionObject.transform.position);

                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 오브젝트 상태를 초기화하고 클라이언트와 동기화한다. 이 함수는 서버에서만 호출한다.
        /// </summary>
        /// <param name="color">오브젝트 색깔</param>
        public void Initialize(ColorType color)
        {
            InitializeClientRpc(color, transform.position, transform.rotation, transform.localScale);
        }
    }
}

