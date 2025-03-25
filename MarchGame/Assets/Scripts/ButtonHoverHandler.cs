using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Button button;
    private Vector3 originalScale;

    void Start()
    {
        button = GetComponent<Button>();
        originalScale = button.transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        LeanTween.scale(button.gameObject, originalScale * 1.1f, 0.2f).setEase(LeanTweenType.easeOutQuad).setIgnoreTimeScale(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        LeanTween.scale(button.gameObject, originalScale, 0.2f).setEase(LeanTweenType.easeOutQuad).setIgnoreTimeScale(true);
    }
}
