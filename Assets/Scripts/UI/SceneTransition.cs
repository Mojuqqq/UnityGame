using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class SceneTransition : MonoBehaviour
{
    [Header("Fade")]
    [SerializeField, Min(0.01f)] private float fadeOutDuration = 0.22f;
    [SerializeField, Min(0.01f)] private float fadeInDuration = 0.30f;
    [SerializeField] private Color fadeColor = Color.black;

    private static SceneTransition instance;
    private CanvasGroup group;
    private GameObject overlay;
    private bool busy;

    public static bool IsTransitioning => instance != null && instance.busy;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        // Place the optional configured component on a dedicated root object.
        if (transform.parent != null)
            transform.SetParent(null, true);
        DontDestroyOnLoad(gameObject);
        CreateOverlay();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
            if (overlay != null)
                Destroy(overlay);
        }
    }

    private static SceneTransition GetOrCreate()
    {
        if (instance != null)
            return instance;

        return new GameObject("SceneTransition").AddComponent<SceneTransition>();
    }

    private void CreateOverlay()
    {
        overlay = new GameObject("TransitionCanvas", typeof(RectTransform),
            typeof(Canvas), typeof(GraphicRaycaster), typeof(CanvasGroup));
        DontDestroyOnLoad(overlay);

        var canvas = overlay.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        group = overlay.GetComponent<CanvasGroup>();

        var imageObject = new GameObject("Fade", typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(overlay.transform, false);
        var rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var image = imageObject.GetComponent<Image>();
        var color = fadeColor;
        color.a = 1f;
        image.color = color;
        image.raycastTarget = true;

        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    // Standard single-scene transition. False means the request was rejected.
    public static bool TryLoad(string sceneName)
    {
        if (IsTransitioning)
            return false;

        if (string.IsNullOrWhiteSpace(sceneName) ||
            !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"SceneTransition: scene '{sceneName}' is not in the build.");
            return false;
        }

        var transition = GetOrCreate();
        transition.Begin(sceneName, null);
        return true;
    }

    // Used only by LoadingScreenManager. The caller has already started loading
    // and kept allowSceneActivation = false. Ownership passes to this service.
    public static bool TryActivatePreloaded(AsyncOperation operation)
    {
        if (operation == null || operation.isDone || IsTransitioning)
            return false;

        var transition = GetOrCreate();
        transition.Begin(null, operation);
        return true;
    }

    private void Begin(string sceneName, AsyncOperation operation)
    {
        busy = true;
        group.blocksRaycasts = true;
        group.interactable = true;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
        StartCoroutine(TransitionRoutine(sceneName, operation));
    }

    private IEnumerator TransitionRoutine(string sceneName, AsyncOperation operation)
    {
        yield return FadeTo(1f, fadeOutDuration);
        // Let the fully opaque overlay render before switching scenes.
        yield return null;

        if (operation == null)
        {
            try
            {
                operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        if (operation != null)
        {
            operation.allowSceneActivation = true;
            while (!operation.isDone)
                yield return null;

            // Allow Start and initial UI layout in the newly activated scene.
            yield return null;
            Canvas.ForceUpdateCanvases();
            yield return null;
        }
        else
        {
            Debug.LogError($"SceneTransition: failed to load '{sceneName}'.");
        }

        yield return FadeTo(0f, fadeInDuration);
        group.blocksRaycasts = false;
        group.interactable = false;
        busy = false;
    }

    private IEnumerator FadeTo(float target, float duration)
    {
        float start = group.alpha;
        float elapsed = 0f;
        duration = Mathf.Max(0.01f, duration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            group.alpha = Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        group.alpha = target;
    }
}
