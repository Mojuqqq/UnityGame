using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Button))]
public class UIButtonClickSfx :
    MonoBehaviour
{
    private Button button;


    private void Awake()
    {
        button =
            GetComponent<Button>();
    }


    private void OnEnable()
    {
        if (button == null)
        {
            button =
                GetComponent<Button>();
        }


        button.onClick.AddListener(
            PlayClick
        );
    }


    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(
                PlayClick
            );
        }
    }


    private void PlayClick()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }


        AudioManager.Instance.PlaySfx(
            GameSfx.UiClick
        );
    }
}