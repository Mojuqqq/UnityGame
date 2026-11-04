using UnityEngine;


[RequireComponent(
    typeof(RectTransform)
)]
public class TutorialPulse :
    MonoBehaviour
{
    [SerializeField]
    private float pulseAmount =
        0.035f;


    [SerializeField]
    private float pulseSpeed =
        2.5f;


    private RectTransform rectTransform;

    private Vector3 originalScale;


    private void Awake()
    {
        rectTransform =
            GetComponent<RectTransform>();


        originalScale =
            rectTransform.localScale;
    }


    private void OnEnable()
    {
        if (rectTransform == null)
        {
            rectTransform =
                GetComponent<RectTransform>();
        }


        originalScale =
            Vector3.one;


        rectTransform.localScale =
            originalScale;
    }


    private void Update()
    {
        if (rectTransform == null)
        {
            return;
        }


        float wave =
            (
                Mathf.Sin(
                    Time.unscaledTime *
                    pulseSpeed
                )
                +
                1f
            )
            *
            0.5f;


        float scale =
            1f +
            wave *
            pulseAmount;


        rectTransform.localScale =
            originalScale *
            scale;
    }


    private void OnDisable()
    {
        if (rectTransform != null)
        {
            rectTransform.localScale =
                Vector3.one;
        }
    }
}