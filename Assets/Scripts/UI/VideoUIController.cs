using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class VideoUIController 
{
    private VisualElement _root;
    
    private Action OnCloseVideo;

    private Button _resetVideoUIButton;
    private Button _closeVideoUIButton;
    private Button _applyButton; 
    private bool isApplied = false;

    private DropdownField _resolution;
    private string _selectedResolution;
    private int savedWidth;
    private int savedHeight;
    
    private Toggle _fullScreenToggle;
    private bool _isFullScreen;
    private bool savedFullScreen;

    private BasicSlider _basicSlider;
    private Slider _brightnessSlider;
    private float savedBrightness;
    

    public VideoUIController(VisualElement root, Action OnClickCloseVideoUIButtonClick)
    {
        _root = root;
        
        SoundManager.RegisterButtonClickSound(_root);

        OnCloseVideo = OnClickCloseVideoUIButtonClick;
        
        _resetVideoUIButton = _root.Q<Button>("ResetVideoUIButton");
        _closeVideoUIButton = _root.Q<Button>("CloseVideoUIButton");
        _applyButton = _root.Q<Button>("ApplyVideoUIButton"); // Apply 버튼 가져오기
        
        _brightnessSlider = _root.Q<Slider>("BrightnessSlider");
        
        InitResolutions();
        InitFullScreen();
        InitBasicSlider(_brightnessSlider);

        _resetVideoUIButton.RegisterCallback<ClickEvent>(OnClickResetVideoUIButton);
        _closeVideoUIButton.RegisterCallback<ClickEvent>(OnClickCloseVideoUIButton);
        _applyButton.RegisterCallback<ClickEvent>(OnClickApplyVideoUIButton);
        
        string[] resolutionParts = _selectedResolution.Split('x');
        savedWidth = int.Parse(resolutionParts[0]);
        savedHeight = int.Parse(resolutionParts[1]);
        savedFullScreen = _isFullScreen;
        savedBrightness = PlayerPrefs.GetFloat("BrightnessValue");
        
        _applyButton.style.display = DisplayStyle.None;  // 처음에는 숨김
    }
    
    // 해상도 변경
    private void InitResolutions()
    {
        _resolution = _root.Q<DropdownField>("Resolution");
        _resolution.choices = Screen.resolutions
            .Select(resolution => $"{resolution.width}x{resolution.height}")
            .ToList();

        _selectedResolution = $"{Screen.currentResolution.width}x{Screen.currentResolution.height}";
        _resolution.index = _resolution.choices.IndexOf(_selectedResolution);

        // 사용자가 드롭다운을 변경할 때 Apply 버튼 표시
        _resolution.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue != _selectedResolution)
            {
                _selectedResolution = evt.newValue;
                _applyButton.style.display = DisplayStyle.Flex; // 버튼 표시
            }
        });
    }
    
    // 전체화면 토글
    private void InitFullScreen()
    {
        _fullScreenToggle = _root.Q<Toggle>("FullScreenToggle");
        _isFullScreen = Screen.fullScreen;
        _fullScreenToggle.value = _isFullScreen;

        _fullScreenToggle.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue != _isFullScreen)
            {
                _isFullScreen = evt.newValue;
                _applyButton.style.display = DisplayStyle.Flex; 
            }
        });
    }
    
    // brightnessSlider 밝기 조절
    private void InitBasicSlider(Slider slider)
    {
        if (slider != null)
        {
            _basicSlider = new BasicSlider();
            _basicSlider.Initialize(slider);

            slider.value = VideoManager.Instance.GetBrightness();
            
            slider.RegisterValueChangedCallback(evt => OnSliderValueChanged(evt.newValue));
        }
    }

    private void OnSliderValueChanged(float value)
    {
        VideoManager.Instance.SetBrightness(value);
        _applyButton.style.display = DisplayStyle.Flex; 
    }
    
    private void OnClickResetVideoUIButton(ClickEvent evt)
    {
        // 해상도를 기본값으로 설정
        _selectedResolution = $"{Screen.currentResolution.width}x{Screen.currentResolution.height}";
        _resolution.index = _resolution.choices.IndexOf(_selectedResolution);

        // 전체 화면 설정을 기본값으로
        _isFullScreen = true;
        _fullScreenToggle.value = _isFullScreen;
        
        Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, _isFullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);

        // 밝기를 기본값(0.5)으로 설정
        _brightnessSlider.value = 0.5f;

        // Apply 버튼 보이기 (설정이 바뀌었으므로)
        _applyButton.style.display = DisplayStyle.Flex; 
    }

    private void OnClickApplyVideoUIButton(ClickEvent evt)
    {
        // 선택된 해상도 적용
        string[] resolutionParts = _selectedResolution.Split('x');
        int width = int.Parse(resolutionParts[0]);
        int height = int.Parse(resolutionParts[1]);

        Screen.SetResolution(width, height, _isFullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        
        // PlayerPrefs에 Brightness 값 저장
        VideoManager.Instance.SaveBrightness();

        isApplied = true;

        _applyButton.style.display = DisplayStyle.None; // 다시 숨김
    }

    private void OnClickCloseVideoUIButton(ClickEvent evt)
    {
        if (!isApplied)
        {
            Screen.SetResolution(savedWidth, savedHeight, savedFullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
            VideoManager.Instance.SetBrightness(savedBrightness);
        }
        
        OnCloseVideo?.Invoke();
    }
}