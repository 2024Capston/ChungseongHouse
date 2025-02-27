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

    private DropdownField _resolution;
    public VideoUIController(VisualElement root, Action OnClickCloseVideoUIButtonClick)
    {
        _root = root;
        OnCloseVideo = OnClickCloseVideoUIButtonClick;
        _closeVideoUIButton = _root.Q<Button>("CloseVideoUIButton");

        InitResoultions();
        
        _closeVideoUIButton.RegisterCallback<ClickEvent>(OnClickCloseVideoUIButton);
    }

    private void InitResoultions()
    {
        _resolution = _root.Q<DropdownField>("Resolution");
        _resolution.choices = Screen.resolutions.Select(resolution => $"{resolution.width}x{resolution.height}").ToList();
        _resolution.index = Screen.resolutions
            .Select((resolution, index) => (resolution, index))
            .First((value)=>value.resolution.width == Screen.currentResolution.width && value.resolution.height == Screen.currentResolution.height)
            .index;
    }
    private void OnClickCloseVideoUIButton(ClickEvent evt)
    {
        OnCloseVideo?.Invoke();
    }
}
