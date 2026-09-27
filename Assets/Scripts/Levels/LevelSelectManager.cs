using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectManager :
    MonoBehaviour
{
    [Header("Levels")]

    [SerializeField]
    private LevelDatabase levelDatabase;


    [Header("UI")]

    [SerializeField]
    private RectTransform
        buttonsContainer;

    [SerializeField]
    private LevelButtonView
        levelButtonPrefab;


    [Header("Scenes")]

    [SerializeField]
    private string gameSceneName =
        "Level01";


    private void Start()
    {
        UnlockFirstLevel();

        BuildLevelButtons();
    }


    private void UnlockFirstLevel()
    {
        if (levelDatabase == null)
        {
            return;
        }

        LevelData firstLevel =
            levelDatabase.GetFirstLevel();

        if (firstLevel == null)
        {
            return;
        }

        ProgressManager.UnlockLevel(
            firstLevel.LevelNumber
        );
    }


    private void BuildLevelButtons()
    {
        ClearButtons();


        if (levelDatabase == null)
        {
            Debug.LogError(
                "LevelSelectManager: LevelDatabase is not assigned."
            );

            return;
        }


        if (buttonsContainer == null)
        {
            Debug.LogError(
                "LevelSelectManager: Buttons Container is not assigned."
            );

            return;
        }


        if (levelButtonPrefab == null)
        {
            Debug.LogError(
                "LevelSelectManager: Level Button Prefab is not assigned."
            );

            return;
        }


        foreach (
            LevelData level
            in levelDatabase.Levels
        )
        {
            if (level == null)
            {
                continue;
            }


            bool unlocked =
                ProgressManager
                    .IsLevelUnlocked(
                        level.LevelNumber
                    );


            bool completed =
                ProgressManager
                    .IsLevelCompleted(
                        level.LevelNumber
                    );


            LevelButtonView button =
                Instantiate(
                    levelButtonPrefab,
                    buttonsContainer
                );


            button.name =
                $"LevelButton_{level.LevelNumber}";


            button.Initialize(
                level,
                this,
                unlocked,
                completed
            );
        }
    }


    public void SelectLevel(
        LevelData level
    )
    {
        if (level == null)
        {
            return;
        }


        if (
            !ProgressManager
                .IsLevelUnlocked(
                    level.LevelNumber
                )
        )
        {
            return;
        }


        LevelSelectionState.SelectLevel(
            level
        );


        SceneManager.LoadScene(
            gameSceneName
        );
    }


    private void ClearButtons()
    {
        if (buttonsContainer == null)
        {
            return;
        }


        for (
            int i =
                buttonsContainer.childCount - 1;
            i >= 0;
            i--
        )
        {
            GameObject child =
                buttonsContainer
                    .GetChild(i)
                    .gameObject;

            child.SetActive(false);

            Destroy(child);
        }
    }


    [ContextMenu(
        "Reset Progress For Testing"
    )]
    private void ResetProgressForTesting()
    {
        ProgressManager.ResetProgress(
            levelDatabase
        );

        UnlockFirstLevel();

        BuildLevelButtons();

        Debug.Log(
            "Level progress reset."
        );
    }
}