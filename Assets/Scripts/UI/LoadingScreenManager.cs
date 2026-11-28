using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image loadingFill;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private TMP_Text versionText;

    [Header("Scene")]
    [SerializeField] private string homeSceneName = "Menu";

    [Header("Timing")]
    [Tooltip("Minimum display time of the loading screen.")]
    [SerializeField, Min(0.1f)] private float minimumDisplayTime = 5f;
    [Tooltip("Time to display 100% before the transition.")]
    [SerializeField, Min(0f)] private float completionHoldTime = 0.15f;

    [Header("Test")]
    [SerializeField] private bool testMode = false;
    [SerializeField] private bool loopTest = true;
    [SerializeField, Min(0f)] private float testPause = 1f;

    private void Awake()
    {
        ConfigureFill();
        SetProgress(0f);
    }

    private void Start()
    {
        LevelSelectionState.ClearSelection();

        if (versionText != null)
            versionText.text = $"v{Application.version}";

        if (testMode)
            StartCoroutine(RunTest());
        else
            StartCoroutine(LoadHomeScene());
    }

    private void ConfigureFill()
    {
        if (loadingFill == null)
        {
            Debug.LogError("LoadingScreenManager: Loading Fill is not assigned.");
            return;
        }

        loadingFill.type = Image.Type.Filled;
        loadingFill.fillMethod = Image.FillMethod.Horizontal;
        loadingFill.fillOrigin = 0;
        loadingFill.fillAmount = 0f;
        loadingFill.preserveAspect = false;
    }

    private void SetProgress(float value)
    {
        value = Mathf.Clamp01(value);

        if (loadingFill != null)
            loadingFill.fillAmount = value;

        if (loadingText != null)
            loadingText.text = $"ЗАГРУЗКА... {Mathf.RoundToInt(value * 100f)}%";
    }

    private IEnumerator LoadHomeScene()
    {
        SetProgress(0f);

        // If Loading was opened through SceneTransition, let its fade-in finish.
        while (SceneTransition.IsTransitioning)
            yield return null;

        if (string.IsNullOrWhiteSpace(homeSceneName) ||
            !Application.CanStreamedLevelBeLoaded(homeSceneName))
        {
            Debug.LogError($"LoadingScreenManager: scene '{homeSceneName}' is not in the build.");
            if (loadingText != null)
                loadingText.text = "ОШИБКА ЗАГРУЗКИ";
            yield break;
        }

        float startTime = Time.unscaledTime;
        float displayTime = Mathf.Max(0.1f, minimumDisplayTime);
        AsyncOperation operation = null;

        try
        {
            operation = SceneManager.LoadSceneAsync(homeSceneName, LoadSceneMode.Single);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }

        if (operation == null)
        {
            Debug.LogError($"LoadingScreenManager: could not load '{homeSceneName}'.");
            if (loadingText != null)
                loadingText.text = "ОШИБКА ЗАГРУЗКИ";
            yield break;
        }

        operation.allowSceneActivation = false;

        while (true)
        {
            float timeProgress = Mathf.Clamp01((Time.unscaledTime - startTime) / displayTime);
            bool sceneReady = operation.progress >= 0.9f;
            SetProgress(sceneReady ? timeProgress : Mathf.Min(timeProgress, 0.99f));

            if (sceneReady && timeProgress >= 1f)
                break;

            yield return null;
        }

        SetProgress(1f);

        if (completionHoldTime > 0f)
            yield return new WaitForSecondsRealtime(completionHoldTime);

        // Use the existing AsyncOperation; do not load Menu a second time.
        if (SceneTransition.TryActivatePreloaded(operation))
            yield break;

        // Never leave an async operation permanently blocked if the handoff fails.
        Debug.LogWarning("LoadingScreenManager: transition handoff failed; activating the preloaded scene.");
        operation.allowSceneActivation = true;
        while (!operation.isDone)
            yield return null;
    }

    private IEnumerator RunTest()
    {
        do
        {
            SetProgress(0f);
            float startTime = Time.unscaledTime;
            float displayTime = Mathf.Max(0.1f, minimumDisplayTime);

            while (true)
            {
                float progress = Mathf.Clamp01((Time.unscaledTime - startTime) / displayTime);
                SetProgress(progress);

                if (progress >= 1f)
                    break;

                yield return null;
            }

            if (!loopTest)
                yield break;

            if (testPause > 0f)
                yield return new WaitForSecondsRealtime(testPause);
        }
        while (true);
    }
}
