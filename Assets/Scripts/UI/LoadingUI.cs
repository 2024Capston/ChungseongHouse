using System.Collections;
using TMPro;
using UnityEngine;

public class LoadingUI : BaseUI
{
    [SerializeField]
    private TMP_Text _loadingText;

    private readonly string TEXT = "Loading";

    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);
        //StartCoroutine(CoChangeText());
    }

    public override void CloseUI(bool isCloseAll = false)
    {
        //StopCoroutine(CoChangeText());
        base.CloseUI(isCloseAll);
    }

    IEnumerator CoChangeText()
    {
        while (true)
        {
            _loadingText.text = TEXT + ".";
            yield return new WaitForSeconds(0.5f);
            _loadingText.text = TEXT + "..";
            yield return new WaitForSeconds(0.5f);
            _loadingText.text = TEXT + "...";
            yield return new WaitForSeconds(0.5f);
        }
    }
}
