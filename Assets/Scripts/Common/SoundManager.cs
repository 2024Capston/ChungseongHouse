using UnityEngine;
using UnityEngine.UIElements;

public class SoundManager
{
    public static void RegisterButtonClickSound(VisualElement root)
    {
        foreach (var button in root.Query<Button>().ToList())
        {
            button.RegisterCallback<PointerDownEvent>(OnButtonPointerDown, TrickleDown.TrickleDown);
        }
    }

    private static void OnButtonPointerDown(PointerDownEvent evt)
    {
        AudioManager.Instance.PlaySFX(SFX.ButtonClick);
    }
}