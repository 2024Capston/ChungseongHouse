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

namespace ColorObject
{
    public class ColorWallController : NetworkBehaviour
    {
        // 각종 설정 체크 변수
        [SerializeField] private MeshRenderer _colorMeshRenderer;

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
        private Transform _childRedLight;   // light오브젝트
        private Transform _childBlueLight;  // light오브젝트
        private MeshCollider _childCollider;




        [SerializeField] private NetworkVariable<ColorType> _color = new NetworkVariable<ColorType>();
        public NetworkVariable<ColorType> Color
        {
            get => _color;
        }
        private void Awake()
        {
            _childWall = transform.Find("Color_Wall");
            _childRedLight = transform.Find("Color_Wall_Red").Find("PointLight");
            _childBlueLight = transform.Find("Color_Wall_Blue").Find("PointLight");
            _colorMeshRenderer = _childWall.GetComponent<MeshRenderer>();
            _childCollider = _childWall.GetComponent<MeshCollider>();
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
        }

        private void Update()
        {
            // 위치는 (서버)에 의해서만 갱신되도록 한다
            if (!IsServer)
            {
                return;
            }

        }
        /// <summary>
        /// 색깔을 갱신한다.
        /// </summary>
        /// <param name="before">변경 전 색깔</param>
        /// <param name="after">변경 후 색깔</param>
        private void OnWallColorChanged(ColorType before, ColorType after)
        {
            Color newColor = (after == ColorType.Red) ? new Color(1, 0, 0) : (after == ColorType.Blue) ? new Color(0, 0, 1) : new Color(1, 0, 1);

            int newLayer = (after == ColorType.Red) ? LayerMask.NameToLayer("Red") : (after == ColorType.Blue) ? LayerMask.NameToLayer("Blue") : LayerMask.NameToLayer("Purple");
            //int excludedLayer = (after == ColorType.Red) ? LayerMask.GetMask("Blue") : LayerMask.GetMask("Red");

            // 색깔이 다른 물체는 투명도 추가
            if ((IsHost && _color.Value == ColorType.Blue) || (!IsHost && _color.Value == ColorType.Red))
            {
                _childCollider.enabled = false;
            }
            //if (after != )
            //{
            //    _childCollider.
            //    //newColor.a = 0.7f;
            //}
            if (Color.Value == ColorType.Blue)
            {
                _childRedLight.gameObject.SetActive(false);
                _childBlueLight.gameObject.SetActive(true);
            }
            else if(Color.Value == ColorType.Red)
            {
                _childRedLight.gameObject.SetActive(true);
                _childBlueLight.gameObject.SetActive(false);
            }

            newColor.a = 0.7f;
            _colorMeshRenderer.material.color = newColor;
            //gameObject.layer = newLayer;
            Debug.Log("fffsdf");
            // 다른 색깔 물체와는 물리 상호작용하지 않도록 지정
            //_boxCollider.excludeLayers = excludedLayer;

        }
    }
}

