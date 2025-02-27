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

    private Button _closeVideoUIButton;
    private Button _applyButton;  // Apply 버튼 추가

    private DropdownField _resolution;
    private string _selectedResolution;  // 사용자가 선택한 해상도 저장

    public VideoUIController(VisualElement root, Action OnClickCloseVideoUIButtonClick)
    {
        _root = root;
        OnCloseVideo = OnClickCloseVideoUIButtonClick;
        _closeVideoUIButton = _root.Q<Button>("CloseVideoUIButton");
        _applyButton = _root.Q<Button>("ApplyVideoUIButton"); // Apply 버튼 가져오기
        
        InitResolutions();
        
        _closeVideoUIButton.RegisterCallback<ClickEvent>(OnClickCloseVideoUIButton);
        _applyButton.RegisterCallback<ClickEvent>(OnClickApplyVideoUIButton);
        
        _applyButton.style.display = DisplayStyle.None;  // 처음에는 숨김
    }

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

    private void OnClickApplyVideoUIButton(ClickEvent evt)
    {
        // 선택된 해상도 적용
        string[] resolutionParts = _selectedResolution.Split('x');
        int width = int.Parse(resolutionParts[0]);
        int height = int.Parse(resolutionParts[1]);

        Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow);
        Debug.Log($"해상도 변경됨: {width}x{height}");

        _applyButton.style.display = DisplayStyle.None; // 다시 숨김
    }

    private void OnClickCloseVideoUIButton(ClickEvent evt)
    {
        OnCloseVideo?.Invoke();
    }
}