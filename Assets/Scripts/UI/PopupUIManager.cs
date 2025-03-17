using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class PopupUIManager 
{
    // Popup 애니메이션 관리
    public static IEnumerator PopupIn(VisualElement popupPanel)
    {
        // 짧은 지연 시간 후 팝업 표시
        yield return new WaitForSeconds(0.01f);
        popupPanel.AddToClassList("center"); // center 클래스를 추가하여 애니메이션 적용
    }

    public static IEnumerator PopupOut(VisualElement popupPanel)
    {
        // 짧은 지연 시간 후 팝업 숨김
        yield return new WaitForSeconds(0.01f);
        popupPanel.RemoveFromClassList("center"); // center 클래스를 제거하여 애니메이션 적용
        
        yield return new WaitForSeconds(0.5f);
        popupPanel.RemoveFromHierarchy();
    }
}