using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButtonView : MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private Button button;

    [SerializeField]
    private TMP_Text levelNumberText;


    private LevelData levelData;

    private LevelSelectManager
        levelSelectManager;


    public void Initialize(
        LevelData newLevelData,
        LevelSelectManager manager
    )
    {
        levelData =
            newLevelData;

        levelSelectManager =
            manager;


        if (
            levelData == null
        )
        {
            Debug.LogError(
                "LevelButtonView: LevelData is null."
            );

            return;
        }


        if (
            levelNumberText != null
        )
        {
            levelNumberText.text =
                levelData
                    .LevelNumber
                    .ToString();
        }


        if (button != null)
        {
            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(
                OnButtonClicked
            );
        }
    }


    private void OnButtonClicked()
    {
        if (
            levelSelectManager == null ||
            levelData == null
        )
        {
            return;
        }


        levelSelectManager
            .SelectLevel(
                levelData
            );
    }
}