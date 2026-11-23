using UnityEngine;
using UnityEngine.UI;


public class SettingsPopupController :
    MonoBehaviour
{
    // =====================================================
    // POPUP
    // =====================================================

    [Header("Popup")]

    [SerializeField]
    private GameObject popupRoot;


    // =====================================================
    // TOGGLES
    // =====================================================

    [Header("Toggles")]

    [SerializeField]
    private Toggle soundToggle;


    [SerializeField]
    private Toggle musicToggle;


    [SerializeField]
    private Toggle hapticsToggle;


    // =====================================================
    // STATE
    // =====================================================

    private bool updatingUI;


    // =====================================================
    // UNITY
    // =====================================================

    private void Awake()
    {
        if (popupRoot != null)
        {
            popupRoot.SetActive(
                false
            );
        }
    }


    private void OnEnable()
    {
        GameSettings.SettingsChanged +=
            RefreshUI;
    }


    private void OnDisable()
    {
        GameSettings.SettingsChanged -=
            RefreshUI;
    }


    // =====================================================
    // OPEN / CLOSE
    // =====================================================

    public void Open()
    {
        RefreshUI();


        if (popupRoot != null)
        {
            popupRoot.SetActive(
                true
            );
        }
    }


    public void Close()
    {
        if (popupRoot != null)
        {
            popupRoot.SetActive(
                false
            );
        }
    }


    // =====================================================
    // TOGGLE CALLBACKS
    // =====================================================

    public void SetSoundEnabled(
        bool enabled
    )
    {
        if (updatingUI)
        {
            return;
        }


        GameSettings.SetSoundEnabled(
            enabled
        );
    }


    public void SetMusicEnabled(
        bool enabled
    )
    {
        if (updatingUI)
        {
            return;
        }


        GameSettings.SetMusicEnabled(
            enabled
        );
    }


    public void SetHapticsEnabled(
        bool enabled
    )
    {
        if (updatingUI)
        {
            return;
        }


        GameSettings.SetHapticsEnabled(
            enabled
        );
    }


    // =====================================================
    // REFRESH
    // =====================================================

    private void RefreshUI()
    {
        updatingUI =
            true;


        if (soundToggle != null)
        {
            soundToggle.SetIsOnWithoutNotify(
                GameSettings.SoundEnabled
            );
        }


        if (musicToggle != null)
        {
            musicToggle.SetIsOnWithoutNotify(
                GameSettings.MusicEnabled
            );
        }


        if (hapticsToggle != null)
        {
            hapticsToggle.SetIsOnWithoutNotify(
                GameSettings.HapticsEnabled
            );
        }


        updatingUI =
            false;
    }
}