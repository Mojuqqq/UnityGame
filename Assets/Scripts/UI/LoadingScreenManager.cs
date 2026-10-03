using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenManager :
    MonoBehaviour
{
    [Header("UI")]

    [SerializeField]
    private Image loadingFill;

    [SerializeField]
    private TMP_Text loadingText;

    [SerializeField]
    private TMP_Text versionText;


    [Header("Scene")]

    [SerializeField]
    private string homeSceneName =
        "LevelSelect";


    [Header("Timing")]

    [SerializeField]
    [Min(0f)]
    private float minimumDisplayTime =
        0.8f;


    private void Start()
    {
        LevelSelectionState
            .ClearSelection();


        if (versionText != null)
        {
            versionText.text =
                $"v{Application.version}";
        }


        StartCoroutine(
            LoadHomeScene()
        );
    }


    private IEnumerator LoadHomeScene()
    {
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
                $"could not load scene {homeSceneName}."
            );

            yield break;
        }


        // Не переключаем сцену мгновенно.
        operation.allowSceneActivation =
            false;


        while (!operation.isDone)
        {
            // Unity загружает сцену до 0.9,
            // а последние 0.1 оставляет
            // на активацию.
            float normalizedProgress =
                Mathf.Clamp01(
                    operation.progress /
                    0.9f
                );


            if (loadingFill != null)
            {
                loadingFill.fillAmount =
                    normalizedProgress;
            }


            if (loadingText != null)
            {
                int percent =
                    Mathf.RoundToInt(
                        normalizedProgress *
                        100f
                    );


                loadingText.text =
                    $"ЗАГРУЗКА... {percent}%";
            }


            bool sceneReady =
                operation.progress >=
                0.9f;


            bool minimumTimePassed =
                Time.unscaledTime -
                startTime
                >=
                minimumDisplayTime;


            if (
                sceneReady &&
                minimumTimePassed
            )
            {
                if (loadingFill != null)
                {
                    loadingFill.fillAmount =
                        1f;
                }


                if (loadingText != null)
                {
                    loadingText.text =
                        "ЗАГРУЗКА... 100%";
                }


                operation.allowSceneActivation =
                    true;
            }


            yield return null;
        }
    }
}