using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BasicSlider
{
    
    private VisualElement _slider;

    private VisualElement _dragger;
    
    private VisualElement _bar;
    
    private VisualElement _newDragger;
    // Start is called before the first frame update
    public void Initialize(VisualElement sliderElement)
    {
        _slider = sliderElement;
        _dragger = _slider.Q<VisualElement>("unity-dragger");

        AddElements();

        _slider.RegisterCallback<ChangeEvent<float>>(SliderValueChanged);
        _slider.RegisterCallback<GeometryChangedEvent>(SliderInit);
    }

    private void AddElements()
    {
        _bar = new VisualElement();
        _dragger.Add(_bar);
        _bar.name = "Bar";
        _bar.AddToClassList("bar");
        
        _newDragger = new VisualElement();
        _slider.Add(_newDragger);
        _newDragger.name = "NewDragger";
        _newDragger.AddToClassList("newDragger");
        _newDragger.pickingMode = PickingMode.Ignore;
    }

    private void SliderValueChanged(ChangeEvent<float> value)
    {
        Vector2 dist = new Vector2((_newDragger.layout.width - _dragger.layout.width) / 2, (_newDragger.layout.height - _dragger.layout.height) / 2);
        Vector2 pos = _dragger.parent.LocalToWorld(_dragger.transform.position);
        _newDragger.transform.position = _newDragger.parent.WorldToLocal(pos-dist);
    }
    
    private void SliderInit(GeometryChangedEvent evt)
    {
        Vector2 dist = new Vector2((_newDragger.layout.width - _dragger.layout.width) / 2, (_newDragger.layout.height - _dragger.layout.height) / 2);
        Vector2 pos = _dragger.parent.LocalToWorld(_dragger.transform.position);
        _newDragger.transform.position = _newDragger.parent.WorldToLocal(pos-dist);
    }
}
