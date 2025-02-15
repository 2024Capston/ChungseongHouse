using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public static class PopupUIManager 
{
    // Popup 애니메이션 관리
    public static IEnumerator PopupIn(VisualElement PopupPanel)
    {
        // 짧은 지연 시간 후 팝업 표시
        yield return new WaitForSeconds(0.01f);
        PopupPanel.AddToClassList("center"); // center 클래스를 추가하여 애니메이션 적용
    }

    public static IEnumerator PopupOut(VisualElement PopupPanel)
    {
        // 짧은 지연 시간 후 팝업 숨김
        yield return new WaitForSeconds(0.01f);
        PopupPanel.RemoveFromClassList("center"); // center 클래스를 제거하여 애니메이션 적용
        yield return new WaitForSeconds(2f);
        PopupPanel.RemoveFromHierarchy();
    }
}