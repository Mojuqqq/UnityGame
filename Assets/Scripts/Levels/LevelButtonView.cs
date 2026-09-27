using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButtonView :
    MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private Button button;

    [SerializeField]
    private TMP_Text levelNumberText;


    private LevelData levelData;

    private LevelSelectManager
        levelSelectManager;

    private bool isUnlocked;


    public void Initialize(
        LevelData newLevelData,
        LevelSelectManager manager,
        bool unlocked,
        bool completed
    )
    {
        levelData =
            newLevelData;

        levelSelectManager =
            manager;

        isUnlocked =
            unlocked;


        if (levelData == null)
        {
            Debug.LogError(
                "LevelButtonView: LevelData is null."
            );

            return;
        }


        if (button != null)
        {
            button.interactable =
                unlocked;

            button.onClick
                .RemoveAllListeners();

            button.onClick
                .AddListener(
                    OnButtonClicked
                );
        }


        if (levelNumberText != null)
        {
            if (!unlocked)
            {
                levelNumberText.text =
                    $"{levelData.LevelNumber}\nЗАКРЫТ";

                levelNumberText.fontSize =
                    24f;
            }
            else if (completed)
            {
                levelNumberText.text =
                    $"{levelData.LevelNumber}\nПРОЙДЕН";

                levelNumberText.fontSize =
                    24f;
            }
            else
            {
                levelNumberText.text =
                    levelData
                        .LevelNumber
                        .ToString();

                levelNumberText.fontSize =
                    42f;
            }
        }
    }


    private void OnButtonClicked()
    {
        if (!isUnlocked)
        {
            return;
        }

        if (
            levelSelectManager == null ||
            levelData == null
        )
        {
            return;
        }

        levelSelectManager.SelectLevel(
            levelData
        );
    }
}