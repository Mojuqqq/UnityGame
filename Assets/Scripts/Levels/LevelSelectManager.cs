using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectManager :
    MonoBehaviour
{
    [Header("Levels")]

    [SerializeField]
    private LevelDatabase
        levelDatabase;


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
        BuildLevelButtons();
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


            LevelButtonView
                button =
                    Instantiate(
                        levelButtonPrefab,
                        buttonsContainer
                    );


            button.name =
                $"LevelButton_{level.LevelNumber}";


            button.Initialize(
                level,
                this
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


        LevelSelectionState
            .SelectLevel(
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
                buttonsContainer
                    .childCount - 1;
            i >= 0;
            i--
        )
        {
            GameObject child =
                buttonsContainer
                    .GetChild(i)
                    .gameObject;

            child.SetActive(
                false
            );

            Destroy(
                child
            );
        }
    }
}