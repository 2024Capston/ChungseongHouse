using UnityEngine;
using Unity.Netcode;
using System.Collections;
using Unity.VisualScripting;

/// <summary>
/// 색깔 변환기를 조작하는 Class
/// </summary>
public class ColorChangerController : NetworkBehaviour
{
    /// <summary>
    /// 변환 시간
    /// </summary>
    [SerializeField] private float _changeTime;

    /// <summary>
    /// ColorChangerUtil과 시간 표시 UI를 포함한 프리팹
    /// </summary>
    [SerializeField] private GameObject _colorChangerUtilPrefab;

    [SerializeField] private MeshRenderer _gaugeMeshRenderer;
    [SerializeField] private Material[] _materials;

    private const float TRANSITION_TIME = 2f;

    private CubeController _cubeOnChanger;
    private Rigidbody _cubeRigidbody;
    private ColorChangerUtil _utilObject;

    private float _timer;

    void Update()
    {
        // 색깔 변환 중인 큐브가 있는 경우
        if (_cubeOnChanger)
        {
            // 큐브의 Owner가 큐브의 위치를 고정한다
            if (_cubeOnChanger.IsOwner && !_cubeOnChanger.IsTaken)
            {
                _timer += Time.deltaTime;

                _cubeRigidbody.MovePosition(Vector3.Lerp(_cubeRigidbody.position, transform.position + Vector3.up * Mathf.Sin(_timer) * 3f, Time.deltaTime * 10f));
                _cubeRigidbody.MoveRotation(Quaternion.Slerp(_cubeRigidbody.rotation, Quaternion.Euler(_timer * 30f, _timer * 30f, 0), Time.deltaTime * 10f));
            }

            // 플레이어가 큐브를 회수하면 색깔 변환 타이머를 시작한다
            if (IsServer && _cubeOnChanger.IsTaken)
            {
                RevertChangingColorClientRpc();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsServer)
        {
            return;
        }

        // 변환기 범위 안에 적당한 큐브가 있으면 작동
        if (other.gameObject.TryGetComponent<CubeController>(out CubeController cubeController) &&
            cubeController.GetComponent<NetworkInterpolator>().VisualReference.GetComponentInChildren<ColorChangerUtil>() == null)
        {
            if (!_cubeOnChanger)
            {
                StartChangingColorClientRpc(cubeController.gameObject);
                StartCoroutine("CoChangeCubeColor", cubeController);
            }
        }
    }

    /// <summary>
    /// 서버와 클라이언트 양측에서 색깔 변환 준비를 시작한다.
    /// </summary>
    /// <param name="cube">색깔을 바꿀 큐브</param>
    [ClientRpc]
    private void StartChangingColorClientRpc(NetworkObjectReference cube)
    {
        if (cube.TryGet(out NetworkObject networkObject))
        {
            _cubeOnChanger = networkObject.GetComponent<CubeController>();
            _cubeRigidbody = networkObject.GetComponent<Rigidbody>();

            // 큐브와 플레이어의 상호 작용을 중단한다.
            _cubeOnChanger.ForceStopInteraction();
            FixCubeClientRpc();

            _cubeOnChanger.GetComponent<CubeRenderer>().PlayTransitionAnimation(TRANSITION_TIME);

            // ColorChangerUtil 컴포넌트 추가
            _utilObject = Instantiate(_colorChangerUtilPrefab).GetComponent<ColorChangerUtil>();

            _utilObject.transform.SetParent(_cubeOnChanger.GetComponent<NetworkInterpolator>().VisualReference.transform);
            _utilObject.transform.localPosition = Vector3.zero;

            _utilObject.Initialize(_cubeOnChanger, _changeTime);
            StartCoroutine("CoUpdateGaugeColor", _cubeOnChanger);
        }
    }

    /// <summary>
    /// 큐브가 변환기에서 벗어나면 타이머를 가동한다.
    /// </summary>
    [ClientRpc]
    private void RevertChangingColorClientRpc()
    {
        _utilObject.StartTimer();

        if (_cubeOnChanger.IsOwner)
        {
            _cubeRigidbody.isKinematic = false;
        }

        _utilObject = null;
        _cubeOnChanger = null;
        _cubeRigidbody = null;
    }

    /// <summary>
    /// 큐브를 색깔 변환기에 고정할 때, Rigidbody의 상태를 동기화한다.
    /// </summary>
    [ClientRpc]
    private void FixCubeClientRpc()
    {
        _cubeRigidbody.isKinematic = true;
        _cubeRigidbody.useGravity = false;
    }

    /// <summary>
    /// 변환기에 올라온 큐브의 색깔을 바꾼다.
    /// </summary>
    /// <param name="cubeController">색깔을 바꿀 큐브</param>
    /// <returns></returns>
    private IEnumerator CoChangeCubeColor(CubeController cubeController)
    {
        cubeController.SetActive(false);

        yield return new WaitForSeconds(TRANSITION_TIME);

        cubeController.SetActive(true);

        // 3 - (ColorType enum): 색깔 교체
        cubeController.ChangeColor(3 - cubeController.Color);

        FixCubeClientRpc();
    }

    private IEnumerator CoUpdateGaugeColor(CubeController cubeController)
    {
        float preparationTime = TRANSITION_TIME * 0.2f;
        float fillTime = TRANSITION_TIME * 0.8f;

        Color initialColor = _gaugeMeshRenderer.material.GetColor("_FillColor");
        Color originalColor = _materials[(int)cubeController.Color - 1].color;
        Color newColor = _materials[2 - (int)cubeController.Color].color;

        _gaugeMeshRenderer.material.SetColor("_BackgroundColor", originalColor);

        float timer = 0f;
        float lastTime = Time.realtimeSinceStartup;

        while (timer < preparationTime)
        {
            _gaugeMeshRenderer.material.SetColor("_FillColor", Color.Lerp(initialColor, originalColor, timer / preparationTime));

            yield return new WaitForSeconds(0.01f);

            timer += Time.realtimeSinceStartup - lastTime;
            lastTime = Time.realtimeSinceStartup;
        }

        _gaugeMeshRenderer.material.SetFloat("_FillAmount", 0f);
        _gaugeMeshRenderer.material.SetColor("_FillColor", newColor);

        timer -= preparationTime;

        while (timer < fillTime)
        {
            _gaugeMeshRenderer.material.SetFloat("_FillAmount", timer / fillTime);

            yield return new WaitForSeconds(0.01f);

            timer += Time.realtimeSinceStartup - lastTime;
            lastTime = Time.realtimeSinceStartup;
        }
    }

    /// <summary>
    /// 색깔 변환기 상태를 초기화한다. 이 함수는 서버에서만 호출한다.
    /// (Change Time은 서버에서만 쓰이는 변수이고, 위치는 Network Transform이 동기화함.)
    /// </summary>
    /// <param name="changeTime">변환 시간</param>
    public void Initialize(float changeTime)
    {
        _changeTime = changeTime;
    }
}
