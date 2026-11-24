using System.Collections;
using UnityEngine;


[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class PopupAnimator :
    MonoBehaviour
{
    // =====================================================
    // REFERENCES
    // =====================================================

    [Header("References")]

    [SerializeField]
    private RectTransform panel;


    // =====================================================
    // ANIMATION
    // =====================================================

    [Header("Animation")]

    [SerializeField]
    [Range(0.8f, 1f)]
    private float hiddenScale =
        0.92f;


    [SerializeField]
    [Min(0.01f)]
    private float openDuration =
        0.18f;


    [SerializeField]
    [Min(0.01f)]
    private float closeDuration =
        0.12f;


    // =====================================================
    // STATE
    // =====================================================

    private CanvasGroup canvasGroup;


    private Vector3 panelBaseScale =
        Vector3.one;


    private Coroutine animationRoutine;


    private bool initialized;


    // =====================================================
    // UNITY
    // =====================================================

    private void Awake()
    {
        EnsureInitialized();
    }


    private void OnDisable()
    {
        if (animationRoutine != null)
        {
            StopCoroutine(
                animationRoutine
            );


            animationRoutine =
                null;
        }
    }


    // =====================================================
    // INITIALIZE
    // =====================================================

    private void EnsureInitialized()
    {
        if (initialized)
        {
            return;
        }


        canvasGroup =
            GetComponent<CanvasGroup>();


        if (panel != null)
        {
            panelBaseScale =
                panel.localScale;
        }


        initialized =
            true;
    }


    // =====================================================
    // OPEN
    // =====================================================

    public void Open()
    {
        EnsureInitialized();


        if (panel == null)
        {
            Debug.LogError(
                "PopupAnimator: Panel is not assigned."
            );

            return;
        }


        StopCurrentAnimation();


        if (!gameObject.activeSelf)
        {
            canvasGroup.alpha =
                0f;


            canvasGroup.interactable =
                false;


            canvasGroup.blocksRaycasts =
                true;


            panel.localScale =
                panelBaseScale *
                hiddenScale;


            gameObject.SetActive(
                true
            );
        }


        canvasGroup.interactable =
            false;


        canvasGroup.blocksRaycasts =
            true;


        animationRoutine =
            StartCoroutine(
                OpenRoutine()
            );
    }


    private IEnumerator OpenRoutine()
    {
        float startAlpha =
            canvasGroup.alpha;


        Vector3 startScale =
            panel.localScale;


        float elapsed =
            0f;


        while (
            elapsed <
            openDuration
        )
        {
            elapsed +=
                Time.unscaledDeltaTime;


            float t =
                Mathf.Clamp01(
                    elapsed /
                    openDuration
                );


            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );


            canvasGroup.alpha =
                Mathf.Lerp(
                    startAlpha,
                    1f,
                    t
                );


            panel.localScale =
                Vector3.Lerp(
                    startScale,
                    panelBaseScale,
                    t
                );


            yield return null;
        }


        canvasGroup.alpha =
            1f;


        panel.localScale =
            panelBaseScale;


        canvasGroup.interactable =
            true;


        canvasGroup.blocksRaycasts =
            true;


        animationRoutine =
            null;
    }


    // =====================================================
    // CLOSE
    // =====================================================

    public void Close()
    {
        EnsureInitialized();


        if (
            panel == null ||
            !gameObject.activeSelf
        )
        {
            return;
        }


        StopCurrentAnimation();


        canvasGroup.interactable =
            false;


        // Пока окно исчезает,
        // не пропускаем клик на UI под ним.
        canvasGroup.blocksRaycasts =
            true;


        animationRoutine =
            StartCoroutine(
                CloseRoutine()
            );
    }


    private IEnumerator CloseRoutine()
    {
        float startAlpha =
            canvasGroup.alpha;


        Vector3 startScale =
            panel.localScale;


        Vector3 targetScale =
            panelBaseScale *
            hiddenScale;


        float elapsed =
            0f;


        while (
            elapsed <
            closeDuration
        )
        {
            elapsed +=
                Time.unscaledDeltaTime;


            float t =
                Mathf.Clamp01(
                    elapsed /
                    closeDuration
                );


            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );


            canvasGroup.alpha =
                Mathf.Lerp(
                    startAlpha,
                    0f,
                    t
                );


            panel.localScale =
                Vector3.Lerp(
                    startScale,
                    targetScale,
                    t
                );


            yield return null;
        }


        canvasGroup.alpha =
            0f;


        panel.localScale =
            targetScale;


        canvasGroup.interactable =
            false;


        canvasGroup.blocksRaycasts =
            false;


        animationRoutine =
            null;


        gameObject.SetActive(
            false
        );
    }


    // =====================================================
    // IMMEDIATE
    // =====================================================

    public void HideImmediate()
    {
        EnsureInitialized();


        StopCurrentAnimation();


        canvasGroup.alpha =
            0f;


        canvasGroup.interactable =
            false;


        canvasGroup.blocksRaycasts =
            false;


        if (panel != null)
        {
            panel.localScale =
                panelBaseScale *
                hiddenScale;
        }


        if (gameObject.activeSelf)
        {
            gameObject.SetActive(
                false
            );
        }
    }


    // =====================================================
    // HELPERS
    // =====================================================

    private void StopCurrentAnimation()
    {
        if (animationRoutine == null)
        {
            return;
        }


        StopCoroutine(
            animationRoutine
        );


        animationRoutine =
            null;
    }
}