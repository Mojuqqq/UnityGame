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
            "Menu"
        );
    }


    // Оставляем старое имя метода,
    // чтобы существующие UnityEvents не сломались.
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
            "Level"
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
            "Loading"
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