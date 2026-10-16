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
        "Полоса не заполнится быстрее этого времени."
    )]
    [SerializeField]
    [Min(0.1f)]
    private float minimumDisplayTime =
        3f;


    // =====================================================
    // TEST
    // =====================================================

    [Header("Test")]

    [Tooltip(
        "Тестировать только анимацию полосы, " +
        "не переходя на другую сцену."
    )]
    [SerializeField]
    private bool testMode =
        false;


    [Tooltip(
        "После достижения 100% повторять тест."
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
    // BAR CONFIGURATION
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


        // 0 = заполнение слева направо.
        loadingFill.fillOrigin =
            0;


        loadingFill.fillAmount =
            0f;


        loadingFill.preserveAspect =
            false;
    }


    // =====================================================
    // PROGRESS
    // =====================================================

    private void SetProgress(
        float value
    )
    {
        value =
            Mathf.Clamp01(
                value
            );


        if (loadingFill != null)
        {
            loadingFill.fillAmount =
                value;
        }


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


        // Не позволяем Unity переключить сцену сама.
        operation.allowSceneActivation =
            false;


        while (true)
        {
            // -----------------------------------------
            // 1. Реальный прогресс Unity
            // -----------------------------------------

            // LoadSceneAsync доходит примерно до 0.9,
            // пока allowSceneActivation = false.

            float realProgress =
                Mathf.Clamp01(
                    operation.progress /
                    0.9f
                );


            // -----------------------------------------
            // 2. Прогресс минимального времени
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
            // 3. Что показываем игроку
            // -----------------------------------------

            // Полоса не может:
            // - обогнать реальную загрузку;
            // - заполниться быстрее minimumDisplayTime.

            float displayedProgress =
                Mathf.Min(
                    realProgress,
                    timeProgress
                );


            SetProgress(
                displayedProgress
            );


            // -----------------------------------------
            // 4. Можно ли открыть следующую сцену?
            // -----------------------------------------

            bool sceneLoaded =
                operation.progress >=
                0.9f;


            bool minimumTimeFinished =
                timeProgress >=
                1f;


            if (
                sceneLoaded &&
                minimumTimeFinished
            )
            {
                break;
            }


            yield return null;
        }


        // Показываем полноценные 100%.
        SetProgress(
            1f
        );


        // Оставляем 100% хотя бы на один кадр,
        // чтобы он реально успел отрисоваться.
        yield return null;


        operation.allowSceneActivation =
            true;


        // Ждём фактической активации сцены.
        while (!operation.isDone)
        {
            yield return null;
        }
    }


    // =====================================================
    // TEST MODE
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