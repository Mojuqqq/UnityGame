using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPopupController : MonoBehaviour
{
    // =====================================================
    // POPUP
    // =====================================================

    [Header("Popup")]

    [SerializeField]
    private GameObject popupRoot;


    // =====================================================
    // CONTROLS
    // =====================================================

    [Header("Controls")]

    [SerializeField]
    private Slider soundSlider;

    [SerializeField]
    private Slider musicSlider;

    [SerializeField]
    private Toggle hapticsToggle;


    // =====================================================
    // VOLUME LABELS
    // =====================================================

    [Header("Volume Labels (Optional)")]

    [SerializeField]
    private TMP_Text soundValueText;

    [SerializeField]
    private TMP_Text musicValueText;


    // =====================================================
    // STATE
    // =====================================================

    private PopupAnimator popupAnimator;


    // =====================================================
    // UNITY
    // =====================================================

    private void Awake()
    {
        if (popupRoot == null)
            return;

        popupAnimator =
            popupRoot.GetComponent<PopupAnimator>();

        if (popupAnimator != null)
            popupAnimator.HideImmediate();
        else
            popupRoot.SetActive(false);
    }

    private void OnEnable()
    {
        GameSettings.SettingsChanged += RefreshUI;

        if (soundSlider != null)
            soundSlider.onValueChanged.AddListener(
                SetSoundVolume
            );

        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(
                SetMusicVolume
            );

        if (hapticsToggle != null)
            hapticsToggle.onValueChanged.AddListener(
                SetHapticsEnabled
            );

        RefreshUI();
    }

    private void OnDisable()
    {
        GameSettings.SettingsChanged -= RefreshUI;

        if (soundSlider != null)
            soundSlider.onValueChanged.RemoveListener(
                SetSoundVolume
            );

        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(
                SetMusicVolume
            );

        if (hapticsToggle != null)
            hapticsToggle.onValueChanged.RemoveListener(
                SetHapticsEnabled
            );

        GameSettings.Save();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
            GameSettings.Save();
    }


    // =====================================================
    // OPEN / CLOSE
    // =====================================================

    public void Open()
    {
        RefreshUI();

        if (popupRoot == null)
            return;

        if (popupAnimator != null)
            popupAnimator.Open();
        else
            popupRoot.SetActive(true);
    }

    public void Close()
    {
        GameSettings.Save();

        if (popupRoot == null)
            return;

        if (popupAnimator != null)
            popupAnimator.Close();
        else
            popupRoot.SetActive(false);
    }


    // =====================================================
    // CALLBACKS
    // =====================================================

    public void SetSoundVolume(float value)
    {
        GameSettings.SetSoundVolume(value);
    }

    public void SetMusicVolume(float value)
    {
        GameSettings.SetMusicVolume(value);
    }

    public void SetHapticsEnabled(bool enabled)
    {
        GameSettings.SetHapticsEnabled(enabled);
    }


    // =====================================================
    // REFRESH
    // =====================================================

    private void RefreshUI()
    {
        if (soundSlider != null)
        {
            soundSlider.SetValueWithoutNotify(
                GameSettings.SoundVolume
            );
        }

        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(
                GameSettings.MusicVolume
            );
        }

        if (hapticsToggle != null)
        {
            hapticsToggle.SetIsOnWithoutNotify(
                GameSettings.HapticsEnabled
            );
        }

        if (soundValueText != null)
        {
            soundValueText.text =
                $"{Mathf.RoundToInt(GameSettings.SoundVolume * 100)}%";
        }

        if (musicValueText != null)
        {
            musicValueText.text =
                $"{Mathf.RoundToInt(GameSettings.MusicVolume * 100)}%";
        }
    }
}