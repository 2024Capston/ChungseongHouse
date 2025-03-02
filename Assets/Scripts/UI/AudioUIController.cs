using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Audio;

public class AudioUIController 
{
    private VisualElement _root;
    
    private Action OnCloseVideo;

    private Button _resetAudioButton;
    private Button _closeAudioUIButton;
    private Button _applyButton; 
    
    private BasicSlider _basicSlider;
    
    private Slider _masterSlider;
    private float savedMaster;
    
    private Slider _BGMSlider;
    private float savedBGM;
    
    private Slider _SFXSlider;
    private float savedSFX;
    
    public AudioUIController(VisualElement root, Action OnClickCloseAudioUIButtonClick)
    {
        _root = root;
        
        SoundManager.RegisterButtonClickSound(_root);
        
        OnCloseVideo = OnClickCloseAudioUIButtonClick;
        
        _resetAudioButton = _root.Q<Button>("ResetAudioUIButton");
        _closeAudioUIButton = _root.Q<Button>("CloseAudioUIButton");
        _applyButton = _root.Q<Button>("ApplyAudioUIButton"); 
        
        _masterSlider = _root.Q<Slider>("MasterSlider");
        _BGMSlider = _root.Q<Slider>("BGMSlider");
        _SFXSlider = _root.Q<Slider>("SFXSlider");
        

        InitSlider(_masterSlider, "Master");
        InitSlider(_BGMSlider, "BGM");
        InitSlider(_SFXSlider, "SFX");

        _resetAudioButton.RegisterCallback<ClickEvent>(OnClickResetAudioUIButton);
        _closeAudioUIButton.RegisterCallback<ClickEvent>(OnClickCloseAudioUIButton);
        _applyButton.RegisterCallback<ClickEvent>(OnClickApplyAudioUIButton);
        
        _applyButton.style.display = DisplayStyle.None;  // 처음에는 숨김
    }
    
    private void InitSlider(Slider slider, string Parameter)
    {
        if (slider != null)
        {
            _basicSlider = new BasicSlider();
            _basicSlider.Initialize(slider);

            slider.value = AudioManager.Instance.GetValue(Parameter);

            slider.RegisterValueChangedCallback(evt =>
            {
                AudioManager.Instance._audioMixer.SetFloat(Parameter, evt.newValue);
                _applyButton.style.display = DisplayStyle.Flex;
            });
                
        }
    }
    
    private void OnClickResetAudioUIButton(ClickEvent evt)
    {
        AudioManager.Instance.SetValue(0, 0, 0);
    }

    
    private void OnClickApplyAudioUIButton(ClickEvent evt)
    {
        AudioManager.Instance.SetValue(_masterSlider.value, _BGMSlider.value, _SFXSlider.value);
        
        _applyButton.style.display = DisplayStyle.None; // 다시 숨김
    }

    private void OnClickCloseAudioUIButton(ClickEvent evt)
    {
        AudioManager.Instance.ApplyAudioMixerValues();
        
        OnCloseVideo?.Invoke();
    }
}
