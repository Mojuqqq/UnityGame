using System.Collections;
using UnityEngine;


public class TutorialPointerAnimator :
    MonoBehaviour
{
    // =====================================================
    // REFERENCES
    // =====================================================

    [Header("References")]

    [SerializeField]
    private RectTransform tutorialRootRect;


    [SerializeField]
    private RectTransform pointerRect;


    [SerializeField]
    private CanvasGroup pointerCanvasGroup;


    [SerializeField]
    private RectTransform tapRing;


    [SerializeField]
    private CanvasGroup tapRingCanvasGroup;


    // =====================================================
    // TAP
    // =====================================================

    [Header("Tap Animation")]

    [SerializeField]
    private float tapPause =
        0.7f;


    [SerializeField]
    private float pressDuration =
        0.12f;


    [SerializeField]
    private float releaseDuration =
        0.16f;


    [SerializeField]
    private float pressedScale =
        0.78f;


    // =====================================================
    // DRAG
    // =====================================================

    [Header("Drag Animation")]

    [SerializeField]
    private float dragDuration =
        0.85f;


    [SerializeField]
    private float dragStartPause =
        0.35f;


    [SerializeField]
    private float dragEndPause =
        0.45f;


    // =====================================================
    // RING
    // =====================================================

    [Header("Tap Ring")]

    [SerializeField]
    private float ringDuration =
        0.35f;


    [SerializeField]
    private float ringStartScale =
        0.45f;


    [SerializeField]
    private float ringEndScale =
        1.35f;


    // =====================================================
    // STATE
    // =====================================================

    private Coroutine animationRoutine;


    // =====================================================
    // UNITY
    // =====================================================

    private void Awake()
    {
        if (pointerRect == null)
        {
            pointerRect =
                GetComponent<RectTransform>();
        }


        if (pointerCanvasGroup == null)
        {
            pointerCanvasGroup =
                GetComponent<CanvasGroup>();
        }


        HideImmediate();
    }


    // =====================================================
    // PUBLIC
    // =====================================================

    public void ShowTap(
        RectTransform target
    )
    {
        if (target == null)
        {
            HidePointer();

            return;
        }


        RestartAnimation(
            AnimateTap(
                target
            )
        );
    }


    public void ShowDrag(
        RectTransform from,
        RectTransform to
    )
    {
        if (
            from == null ||
            to == null
        )
        {
            HidePointer();

            return;
        }


        RestartAnimation(
            AnimateDrag(
                from,
                to
            )
        );
    }


    public void HidePointer()
{
    StopAllCoroutines();


    animationRoutine =
        null;


    HideImmediate();
}

    // =====================================================
    // START ANIMATION
    // =====================================================

    private void RestartAnimation(
    IEnumerator animation
)
{
    StopAllCoroutines();


    if (pointerCanvasGroup != null)
    {
        pointerCanvasGroup.alpha =
            1f;


        pointerCanvasGroup.interactable =
            false;


        pointerCanvasGroup.blocksRaycasts =
            false;
    }


    HideRing();


    animationRoutine =
        StartCoroutine(
            animation
        );
}

    // =====================================================
    // TAP
    // =====================================================

    private IEnumerator AnimateTap(
        RectTransform target
    )
    {
        while (true)
        {
            if (target == null)
            {
                yield break;
            }


            Vector2 targetPosition =
                GetTargetCenter(
                    target
                );


            pointerRect
                .anchoredPosition =
                    targetPosition;


            pointerRect.localScale =
                Vector3.one;


            if (pointerCanvasGroup != null)
            {
                pointerCanvasGroup.alpha =
                    1f;
            }


            yield return
                WaitUnscaled(
                    0.2f
                );


            // Нажатие.
            yield return
                AnimateScale(
                    1f,
                    pressedScale,
                    pressDuration
                );


            // Круг клика.
            yield return
                AnimateTapRing();


            // Отпускание.
            yield return
                AnimateScale(
                    pressedScale,
                    1f,
                    releaseDuration
                );


            yield return
                WaitUnscaled(
                    tapPause
                );
        }
    }


    // =====================================================
    // DRAG
    // =====================================================

    private IEnumerator AnimateDrag(
        RectTransform from,
        RectTransform to
    )
    {
        while (true)
        {
            if (
                from == null ||
                to == null
            )
            {
                yield break;
            }


            Vector2 start =
                GetTargetCenter(
                    from
                );


            Vector2 end =
                GetTargetCenter(
                    to
                );


            pointerRect
                .anchoredPosition =
                    start;


            pointerRect.localScale =
                Vector3.one;


            if (pointerCanvasGroup != null)
            {
                pointerCanvasGroup.alpha =
                    1f;
            }


            yield return
                WaitUnscaled(
                    dragStartPause
                );


            // Палец "нажимает".
            yield return
                AnimateScale(
                    1f,
                    pressedScale,
                    pressDuration
                );


            // Перемещение.
            float elapsed =
                0f;


            while (
                elapsed <
                dragDuration
            )
            {
                elapsed +=
                    Time.unscaledDeltaTime;


                float t =
                    Mathf.Clamp01(
                        elapsed /
                        dragDuration
                    );


                // Мягкое ускорение/торможение.
                float smooth =
                    t *
                    t *
                    (
                        3f -
                        2f * t
                    );


                pointerRect
                    .anchoredPosition =
                        Vector2.Lerp(
                            start,
                            end,
                            smooth
                        );


                yield return null;
            }


            pointerRect
                .anchoredPosition =
                    end;


            // Отпускаем.
            yield return
                AnimateScale(
                    pressedScale,
                    1f,
                    releaseDuration
                );


            yield return
                AnimateTapRing();


            yield return
                WaitUnscaled(
                    dragEndPause
                );


            // Между повторами скрываем,
            // чтобы указатель не летел
            // обратно по экрану.
            if (pointerCanvasGroup != null)
            {
                pointerCanvasGroup.alpha =
                    0f;
            }


            yield return
                WaitUnscaled(
                    0.25f
                );
        }
    }


    // =====================================================
    // TAP RING
    // =====================================================

    private IEnumerator AnimateTapRing()
    {
        if (
            tapRing == null ||
            tapRingCanvasGroup == null
        )
        {
            yield break;
        }


        tapRing.gameObject
            .SetActive(
                true
            );


        tapRing.localScale =
            Vector3.one *
            ringStartScale;


        tapRingCanvasGroup.alpha =
            0.7f;


        float elapsed =
            0f;


        while (
            elapsed <
            ringDuration
        )
        {
            elapsed +=
                Time.unscaledDeltaTime;


            float t =
                Mathf.Clamp01(
                    elapsed /
                    ringDuration
                );


            float scale =
                Mathf.Lerp(
                    ringStartScale,
                    ringEndScale,
                    t
                );


            tapRing.localScale =
                Vector3.one *
                scale;


            tapRingCanvasGroup.alpha =
                Mathf.Lerp(
                    0.7f,
                    0f,
                    t
                );


            yield return null;
        }


        HideRing();
    }


    private void HideRing()
    {
        if (tapRingCanvasGroup != null)
        {
            tapRingCanvasGroup.alpha =
                0f;
        }


        if (tapRing != null)
        {
            tapRing.gameObject
                .SetActive(
                    false
                );
        }
    }


    // =====================================================
    // SCALE
    // =====================================================

    private IEnumerator AnimateScale(
        float from,
        float to,
        float duration
    )
    {
        float elapsed =
            0f;


        while (
            elapsed <
            duration
        )
        {
            elapsed +=
                Time.unscaledDeltaTime;


            float t =
                Mathf.Clamp01(
                    elapsed /
                    duration
                );


            float scale =
                Mathf.Lerp(
                    from,
                    to,
                    t
                );


            pointerRect.localScale =
                Vector3.one *
                scale;


            yield return null;
        }


        pointerRect.localScale =
            Vector3.one *
            to;
    }


    // =====================================================
    // POSITION
    // =====================================================

    private Vector2 GetTargetCenter(
        RectTransform target
    )
    {
        if (
            target == null ||
            tutorialRootRect == null
        )
        {
            return Vector2.zero;
        }


        Vector3[] corners =
            new Vector3[4];


        target.GetWorldCorners(
            corners
        );


        Vector3 worldCenter =
            (
                corners[0] +
                corners[2]
            )
            *
            0.5f;


        Vector3 local =
            tutorialRootRect
                .InverseTransformPoint(
                    worldCenter
                );


        return new Vector2(
            local.x,
            local.y
        );
    }


    // =====================================================
    // WAIT
    // =====================================================

    private IEnumerator WaitUnscaled(
        float duration
    )
    {
        float elapsed =
            0f;


        while (
            elapsed <
            duration
        )
        {
            elapsed +=
                Time.unscaledDeltaTime;


            yield return null;
        }
    }


    // =====================================================
    // HIDE
    // =====================================================

    private void HideImmediate()
{
    HideRing();


    if (pointerCanvasGroup != null)
    {
        pointerCanvasGroup.alpha =
            0f;


        pointerCanvasGroup.interactable =
            false;


        pointerCanvasGroup.blocksRaycasts =
            false;
    }


    if (pointerRect != null)
    {
        pointerRect.localScale =
            Vector3.one;
    }
}
}