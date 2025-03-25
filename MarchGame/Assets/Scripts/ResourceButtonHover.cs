using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ResourceButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private int woodCost;
    [SerializeField] private int foodCost;
    [SerializeField] private GameObject woodNeeded;
    [SerializeField] private GameObject foodNeeded;
    [SerializeField] private TextMeshProUGUI woodNeededText;
    [SerializeField] private TextMeshProUGUI foodNeededText;

    private Vector3 originalScale;
    private Vector3 scaledUpSize;
    private float tweenTime = 0.2f; // Animation duration

    void Start()
    {
        originalScale = transform.localScale;
        scaledUpSize = originalScale * 1.2f;

        if (woodNeeded != null)
        {
            woodNeededText.text = "x " + woodCost;
            woodNeeded.SetActive(false); // Hide initially
        }
        if (foodNeeded != null)
        {
            foodNeededText.text = "x " + foodCost;
            foodNeeded.SetActive(false); // Hide initially
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        LeanTween.scale(gameObject, scaledUpSize, tweenTime).setEase(LeanTweenType.easeOutQuad).setIgnoreTimeScale(true);

        if (foodNeeded != null)
        {
            foodNeeded.SetActive(true);
        }
        if (woodNeeded != null)
        {
            woodNeeded.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        LeanTween.scale(gameObject, originalScale, tweenTime).setEase(LeanTweenType.easeOutQuad).setIgnoreTimeScale(true);

        if (foodNeeded != null)
        {
            foodNeeded.SetActive(false);
        }
        if (woodNeeded != null)
        {
            woodNeeded.SetActive(false);
        }
    }
}
