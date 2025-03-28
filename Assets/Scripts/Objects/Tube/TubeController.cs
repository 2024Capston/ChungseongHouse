using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TubeController : NetworkBehaviour
{
    private static readonly int FILL_ID = Shader.PropertyToID("_Fill");
    private static readonly int WOBBLE_X_ID = Shader.PropertyToID("_WobbleX");
    private static readonly int WOBBLE_Z_ID = Shader.PropertyToID("_WobbleZ");
    
    [SerializeField] private Renderer _liquidRenderer;
    
    private NetworkVariable<float> _fill = new NetworkVariable<float>(.5f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public float Fill
    {   
        get => _fill.Value;
        set => _fill.Value = value;
    }
    
    [Tooltip("흔들림 세기")] [SerializeField] private float _maxWobble = 1f;
    
    [Tooltip("흔들림 속도")] [SerializeField] private float _wobbleSpeed = 1f;

    [Tooltip("감쇄 속도")] [SerializeField] private float _recoverySpeed = 1f;
    
    private float _wobbleAmountX = 0.0f;
    private float _wobbleAmountZ = 0.0f;
    private float _wobbleAmountToAddX = 0.0f;
    private float _wobbleAmountToAddZ = 0.0f;
    private float _pulse;
    private float _time = 0.5f;

    [SerializeField]
    private ColorType _color;

    public ColorType Color
    {
        get => _color;
        set
        {
            _color = value;
            _liquidRenderer.material.SetObjectColor(_color);
        }
    }
    private ColorType _playerColor;
    private int _viewMode = 1;
    
    
    private void Awake()
    {
        _fill.OnValueChanged += OnValueChanged;
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _playerColor = NetworkManager.Singleton.IsHost ? ColorType.Blue : ColorType.Red;
        
        _liquidRenderer.material.SetFloat(FILL_ID, Mathf.Clamp(_fill.Value, 0f, 1f));
        _liquidRenderer.material.SetMaterial(Color, _playerColor, _viewMode);
    }
    
    private void Update()
    {
        _time += Time.deltaTime;
        _wobbleAmountToAddX = Mathf.Lerp(_wobbleAmountToAddX, 1f, Time.deltaTime * _recoverySpeed);
        _wobbleAmountToAddZ = Mathf.Lerp(_wobbleAmountToAddZ, 1f, Time.deltaTime * _recoverySpeed);
        
        _pulse = 2 * Mathf.PI * _wobbleSpeed;
        _wobbleAmountX = _wobbleAmountToAddX * Mathf.Sin(_pulse * _time);
        _wobbleAmountZ = _wobbleAmountToAddZ * Mathf.Sin(_pulse * _time);
        
        _liquidRenderer.material.SetFloat(WOBBLE_X_ID, _wobbleAmountX);
        _liquidRenderer.material.SetFloat(WOBBLE_Z_ID, _wobbleAmountZ);
    }
    
    /// <summary>
    /// Tube의 FillAmount를 value만큼 변경한다.
    /// FillAmount는 항상 0~1 사이로 유지된다.
    /// </summary>
    /// <param name="value">FillAmount 변화량</param>
    public void UpdateValue(float value)
    {
        _fill.Value = Mathf.Clamp(_fill.Value + value, 0f, 1f);
    }
    
    private void OnValueChanged(float s, float e)
    {
        StartCoroutine(CoDecreaseValue(s, e));
    }

    private IEnumerator CoDecreaseValue(float s, float e)
    {
        float duration = 1f;
        float elapsed = 0f;
        float wobbleStrength = _maxWobble * Mathf.Abs(s - e);
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float currentValue = Mathf.Lerp(s, e, t);
            _liquidRenderer.material.SetFloat(FILL_ID, currentValue);
            
            _wobbleAmountToAddX += wobbleStrength * (1 - t);  // 처음엔 강하고 점점 감소
            _wobbleAmountToAddZ += wobbleStrength * (1 - t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        _liquidRenderer.material.SetFloat(FILL_ID, e);
    }
}
