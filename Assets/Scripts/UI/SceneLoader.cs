using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadHomeMenu()
    {
        if (SceneTransition.TryLoad("Menu"))
            LevelSelectionState.ClearSelection();
    }

    // Existing UnityEvents may still use this method name.
    public void LoadLevelSelect()
    {
        LoadHomeMenu();
    }

    public void LoadGameScene()
    {
        SceneTransition.TryLoad("Level");
    }

    public void LoadMainMenu()
    {
        if (SceneTransition.TryLoad("Loading"))
            LevelSelectionState.ClearSelection();
    }

    public void ReloadCurrentScene()
    {
        SceneTransition.TryLoad(SceneManager.GetActiveScene().name);
    }
}
