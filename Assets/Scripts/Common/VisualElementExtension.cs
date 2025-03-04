using UnityEngine;
using UnityEngine.UIElements;

public static class VisualElementExtension
{
    public static void RegisterButtonClickSound(this VisualElement root)
    {
        foreach (var button in root.Query<Button>().ToList())
        {
            button.clicked += () => AudioManager.Instance.PlaySFX(SFX.ButtonClick);
        }
    }
}