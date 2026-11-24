using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


[RequireComponent(typeof(Button))]
public class UIButtonPressFeedback :
    MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler
{
    // =====================================================
    // SETTINGS
    // =====================================================

    [Header("Scale")]

    [SerializeField]
    [Range(0.8f, 1f)]
    private float pressedScale =
        0.94f;


    [SerializeField]
    [Min(0.01f)]
    private float pressDuration =
        0.06f;


    [SerializeField]
    [Min(0.01f)]
    private float releaseDuration =
        0.10f;


    // =====================================================
    // STATE
    // =====================================================

    private Button button;


    private RectTransform rectTransform;


    private Vector3 baseScale;


    private Coroutine scaleRoutine;


    // =====================================================
    // UNITY
    // =====================================================

    private void Awake()
    {
        button =
            GetComponent<Button>();


        rectTransform =
            transform as RectTransform;


        baseScale =
            transform.localScale;
    }


    private void OnDisable()
    {
        if (scaleRoutine != null)
        {
            StopCoroutine(
                scaleRoutine
            );


            scaleRoutine =
                null;
        }


        transform.localScale =
            baseScale;
    }


    // =====================================================
    // POINTER
    // =====================================================

    public void OnPointerDown(
        PointerEventData eventData
    )
    {
        if (
            button == null ||
            !button.interactable
        )
        {
            return;
        }


        AnimateScale(
            baseScale *
            pressedScale,
            pressDuration
        );
    }


    public void OnPointerUp(
        PointerEventData eventData
    )
    {
        AnimateScale(
            baseScale,
            releaseDuration
        );
    }


    public void OnPointerExit(
        PointerEventData eventData
    )
    {
        AnimateScale(
            baseScale,
            releaseDuration
        );
    }


    // =====================================================
    // ANIMATION
    // =====================================================

    private void AnimateScale(
        Vector3 targetScale,
        float duration
    )
    {
        if (scaleRoutine != null)
        {
            StopCoroutine(
                scaleRoutine
            );
        }


        scaleRoutine =
            StartCoroutine(
                ScaleRoutine(
                    targetScale,
                    duration
                )
            );
    }


    private IEnumerator ScaleRoutine(
        Vector3 targetScale,
        float duration
    )
    {
        Vector3 startScale =
            transform.localScale;


        float elapsed =
            0f;


        while (elapsed < duration)
        {
            elapsed +=
                Time.unscaledDeltaTime;


            float t =
                Mathf.Clamp01(
                    elapsed /
                    duration
                );


            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );


            transform.localScale =
                Vector3.Lerp(
                    startScale,
                    targetScale,
                    t
                );


            yield return null;
        }


        transform.localScale =
            targetScale;


        scaleRoutine =
            null;
    }
}