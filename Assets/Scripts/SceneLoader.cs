using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader :
    MonoBehaviour
{
    // =====================================================
    // HOME
    // =====================================================

    public void LoadHomeMenu()
    {
        LevelSelectionState
            .ClearSelection();


        SceneManager.LoadScene(
            "LevelSelect"
        );
    }


    // Оставляем старое имя,
    // чтобы старые ссылки случайно не сломались.
    public void LoadLevelSelect()
    {
        LoadHomeMenu();
    }


    // =====================================================
    // GAME
    // =====================================================

    public void LoadGameScene()
    {
        SceneManager.LoadScene(
            "Level01"
        );
    }


    // =====================================================
    // LOADING SCREEN
    // =====================================================

    public void LoadMainMenu()
    {
        LevelSelectionState
            .ClearSelection();


        SceneManager.LoadScene(
            "MainMenu"
        );
    }


    // =====================================================
    // RELOAD
    // =====================================================

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