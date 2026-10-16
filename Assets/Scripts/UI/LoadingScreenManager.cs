using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class LoadingScreenManager : MonoBehaviour
{
    // =====================================================
    // UI
    // =====================================================

    [Header("UI")]

    [SerializeField]
    private Image loadingFill;

    [SerializeField]
    private TMP_Text loadingText;

    [SerializeField]
    private TMP_Text versionText;


    // =====================================================
    // SCENE
    // =====================================================

    [Header("Scene")]

    [SerializeField]
    private string homeSceneName =
        "LevelSelect";


    // =====================================================
    // TIMING
    // =====================================================

    [Header("Timing")]

    [Tooltip(
        "Минимальное время показа загрузочного экрана. " +
        "Полоса будет заполняться не быстрее этого времени."
    )]
    [SerializeField]
    [Min(0.1f)]
    private float minimumDisplayTime =
        5f;


    [Tooltip(
        "Сколько времени показывать 100% " +
        "перед переходом на следующую сцену."
    )]
    [SerializeField]
    [Min(0f)]
    private float completionHoldTime =
        0.15f;


    // =====================================================
    // TEST
    // =====================================================

    [Header("Test")]

    [Tooltip(
        "Тестировать только полосу загрузки " +
        "без перехода на другую сцену."
    )]
    [SerializeField]
    private bool testMode =
        false;


    [Tooltip(
        "Повторять тест полосы после 100%."
    )]
    [SerializeField]
    private bool loopTest =
        true;


    [Tooltip(
        "Пауза между тестовыми циклами."
    )]
    [SerializeField]
    [Min(0f)]
    private float testPause =
        1f;


    // =====================================================
    // UNITY
    // =====================================================

    private void Awake()
    {
        ConfigureFill();

        SetProgress(
            0f
        );
    }


    private void Start()
    {
        LevelSelectionState
            .ClearSelection();


        if (versionText != null)
        {
            versionText.text =
                $"v{Application.version}";
        }


        if (testMode)
        {
            StartCoroutine(
                RunTest()
            );
        }
        else
        {
            StartCoroutine(
                LoadHomeScene()
            );
        }
    }


    // =====================================================
    // FILL SETUP
    // =====================================================

    private void ConfigureFill()
    {
        if (loadingFill == null)
        {
            Debug.LogError(
                "LoadingScreenManager: " +
                "Loading Fill is not assigned."
            );

            return;
        }


        loadingFill.type =
            Image.Type.Filled;


        loadingFill.fillMethod =
            Image.FillMethod.Horizontal;


        // Horizontal:
        // 0 = Left
        // 1 = Right
        loadingFill.fillOrigin =
            0;


        loadingFill.fillAmount =
            0f;


        loadingFill.preserveAspect =
            false;
    }


    // =====================================================
    // SET PROGRESS
    // =====================================================

    private void SetProgress(
        float value
    )
    {
        value =
            Mathf.Clamp01(
                value
            );


        // ---------------------------------------------
        // BAR
        // ---------------------------------------------

        if (loadingFill != null)
        {
            loadingFill.fillAmount =
                value;
        }


        // ---------------------------------------------
        // TEXT
        // ---------------------------------------------

        if (loadingText != null)
        {
            int percent =
                Mathf.RoundToInt(
                    value *
                    100f
                );


            loadingText.text =
                $"ЗАГРУЗКА... {percent}%";
        }
    }


    // =====================================================
    // REAL LOADING
    // =====================================================

    private IEnumerator LoadHomeScene()
    {
        SetProgress(
            0f
        );


        float startTime =
            Time.unscaledTime;


        AsyncOperation operation =
            SceneManager.LoadSceneAsync(
                homeSceneName
            );


        if (operation == null)
        {
            Debug.LogError(
                "LoadingScreenManager: " +
                $"Could not load scene '{homeSceneName}'."
            );

            yield break;
        }


        // Сцену можно загрузить,
        // но пока запрещаем Unity её активировать.
        operation.allowSceneActivation =
            false;


        while (true)
        {
            // -----------------------------------------
            // TIMER
            // -----------------------------------------

            float elapsed =
                Time.unscaledTime -
                startTime;


            float timeProgress =
                Mathf.Clamp01(
                    elapsed /
                    minimumDisplayTime
                );


            // -----------------------------------------
            // UNITY SCENE STATE
            // -----------------------------------------

            bool sceneReady =
                operation.progress >=
                0.9f;


            // -----------------------------------------
            // VISUAL PROGRESS
            // -----------------------------------------

            float displayedProgress;


            if (sceneReady)
            {
                // Сцена уже готова.
                // Просто продолжаем красивую анимацию
                // по таймеру.
                displayedProgress =
                    timeProgress;
            }
            else
            {
                // Если наши минимальные секунды
                // уже закончились, но Unity ещё
                // не загрузила сцену,
                // держимся на 99%.
                displayedProgress =
                    Mathf.Min(
                        timeProgress,
                        0.99f
                    );
            }


            SetProgress(
                displayedProgress
            );


            // -----------------------------------------
            // FINISH
            // -----------------------------------------

            bool timerFinished =
                timeProgress >=
                1f;


            if (
                sceneReady &&
                timerFinished
            )
            {
                break;
            }


            yield return null;
        }


        // =================================================
        // 100%
        // =================================================

        SetProgress(
            1f
        );


        Debug.Log(
            "LoadingScreenManager: " +
            $"loading screen displayed for " +
            $"{Time.unscaledTime - startTime:F2} seconds."
        );


        // Дадим игроку увидеть заполненную полосу.
        if (completionHoldTime > 0f)
        {
            yield return
                new WaitForSecondsRealtime(
                    completionHoldTime
                );
        }


        // Теперь разрешаем переключить сцену.
        operation.allowSceneActivation =
            true;


        while (!operation.isDone)
        {
            yield return null;
        }
    }


    // =====================================================
    // TEST
    // =====================================================

    private IEnumerator RunTest()
    {
        do
        {
            SetProgress(
                0f
            );


            float startTime =
                Time.unscaledTime;


            while (true)
            {
                float elapsed =
                    Time.unscaledTime -
                    startTime;


                float progress =
                    Mathf.Clamp01(
                        elapsed /
                        minimumDisplayTime
                    );


                SetProgress(
                    progress
                );


                if (progress >= 1f)
                {
                    break;
                }


                yield return null;
            }


            if (!loopTest)
            {
                yield break;
            }


            if (testPause > 0f)
            {
                yield return
                    new WaitForSecondsRealtime(
                        testPause
                    );
            }
        }
        while (true);
    }
}