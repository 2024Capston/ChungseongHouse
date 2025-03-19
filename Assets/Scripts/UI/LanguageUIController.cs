using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

public class LanguageUIController 
{
    private VisualElement _root;
    
    private Action _onCloseLanguage;

    private Button _korean;
    private Button _english;
    private Button _japanese;
    private Button _spanish;
    private Button _french;
    private Button _sc;
    private Button _tc;
    private Button _german;
    private Button _portuguese;
    private Button _russian;
    
    private Button _closeLanguageUIButton;


    public LanguageUIController(VisualElement root, Action onClickCloseLanguageUIButtonClick)
    {
        _root = root;
        
        _root.RegisterButtonClickSound();
        
        _onCloseLanguage = onClickCloseLanguageUIButtonClick;
        
        _korean = _root.Q<Button>("KoreanButton");
        _english = _root.Q<Button>("EnglishButton");
        _japanese = _root.Q<Button>("JapaneseButton");
        _spanish = _root.Q<Button>("SpanishButton");
        _french = _root.Q<Button>("FrenchButton");
        _sc = _root.Q<Button>("SCButton");
        _tc = _root.Q<Button>("TCButton");
        _german = _root.Q<Button>("GermanButton");
        _portuguese = _root.Q<Button>("PortugueseButton");
        _russian = _root.Q<Button>("RussianButton");
        _closeLanguageUIButton = _root.Q<Button>("CloseLanguageUIButton");
        
        _korean.RegisterCallback<ClickEvent>(evt => UpdateLocales(6));
        _english.RegisterCallback<ClickEvent>(evt => UpdateLocales(2));
        _japanese.RegisterCallback<ClickEvent>(evt => UpdateLocales(5));
        _spanish.RegisterCallback<ClickEvent>(evt => UpdateLocales(9));
        _french.RegisterCallback<ClickEvent>(evt => UpdateLocales(3));
        _sc.RegisterCallback<ClickEvent>(evt => UpdateLocales(0));
        _tc.RegisterCallback<ClickEvent>(evt => UpdateLocales(1));
        _german.RegisterCallback<ClickEvent>(evt => UpdateLocales(4));
        _portuguese.RegisterCallback<ClickEvent>(evt => UpdateLocales(7));
        _russian.RegisterCallback<ClickEvent>(evt => UpdateLocales(8));
        
        _closeLanguageUIButton.RegisterCallback<ClickEvent>(OnClickCloseLanguageUIButton);
    }

    private void UpdateLocales(int index)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        UIManager.Instance.ResetUI();
    }
    
    private void OnClickCloseLanguageUIButton(ClickEvent evt)
    {
        _onCloseLanguage?.Invoke();
    }
}
