using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class UIAudioAutoBinder :
    MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded +=
            HandleSceneLoaded;
    }


    private void OnDisable()
    {
        SceneManager.sceneLoaded -=
            HandleSceneLoaded;
    }


    private void Start()
    {
        StartCoroutine(
            BindNextFrame()
        );
    }


    private void HandleSceneLoaded(
        Scene scene,
        LoadSceneMode mode
    )
    {
        StartCoroutine(
            BindNextFrame()
        );
    }


    private IEnumerator BindNextFrame()
    {
        // Даём сцене создать UI.
        yield return null;


        BindCurrentScene();
    }


    public static void BindCurrentScene()
    {
        Button[] buttons =
            FindObjectsByType<Button>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );


        foreach (
            Button button
            in buttons
        )
        {
            if (button == null)
            {
                continue;
            }


            if (
                button.GetComponent<
                    UIButtonClickSfx
                >() != null
            )
            {
                continue;
            }


            button.gameObject.AddComponent<
                UIButtonClickSfx
            >();
        }
    }
}