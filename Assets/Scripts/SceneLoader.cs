using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader :
    MonoBehaviour
{
    public void LoadLevelSelect()
    {
        SceneManager.LoadScene(
            "LevelSelect"
        );
    }


    public void LoadGameScene()
    {
        SceneManager.LoadScene(
            "Level01"
        );
    }


    public void LoadMainMenu()
    {
        LevelSelectionState
            .ClearSelection();

        SceneManager.LoadScene(
            "MainMenu"
        );
    }


    public void ReloadCurrentScene()
    {
        Scene currentScene =
            SceneManager
                .GetActiveScene();

        SceneManager.LoadScene(
            currentScene.name
        );
    }
}