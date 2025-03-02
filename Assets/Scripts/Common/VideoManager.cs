using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VideoManager : SingletonBehavior<VideoManager>
{
    private const string BrightnessKey = "BrightnessValue";
    
    private Volume _volume;
    private ColorAdjustments _colorAdjustments;
    private float _brightness = 0.5f;

    protected override void Init()
    {
        base.Init(); 

        _volume = GetComponent<Volume>();
        
        LoadBrightness();
        SaveBrightness();

        if (_volume != null && _volume.profile.TryGet(out _colorAdjustments))
        {
            Debug.Log("Color Adjustments Found!");
        }
        else
        {
            Debug.LogError("Color Adjustments Not Found in Volume!");
        }
        
        SetBrightness(_brightness);
    }

    public void SetBrightness(float value)
    {
        if (_colorAdjustments != null)
        {
            _brightness = value;
            _colorAdjustments.postExposure.value = Mathf.Lerp(-2f, 2f, value);
        }
    }

    public void SaveBrightness()
    {
        PlayerPrefs.SetFloat(BrightnessKey, _brightness);
        PlayerPrefs.Save();
    }

    private void LoadBrightness()
    {
        if (PlayerPrefs.HasKey(BrightnessKey))
        {
            _brightness = PlayerPrefs.GetFloat(BrightnessKey);
            Debug.Log("Brightness Loaded!" + _brightness);
        }
    }

    public float GetBrightness()
    {
        LoadBrightness();
        return _brightness;
    }
}
