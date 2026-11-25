using System;
using UnityEngine;

public static class GameSettings
{
    private const string SoundKey =
        "Puzzle_Settings_Sound";

    private const string MusicKey =
        "Puzzle_Settings_Music";

    private const string HapticsKey =
        "Puzzle_Settings_Haptics";

    private const string SoundVolumeKey =
        "Puzzle_Settings_SoundVolume";

    private const string MusicVolumeKey =
        "Puzzle_Settings_MusicVolume";

    public static event Action SettingsChanged;


    // =====================================================
    // SOUND
    // =====================================================

    public static float SoundVolume =>
        GetVolume(SoundVolumeKey, SoundKey);

    public static bool SoundEnabled =>
        SoundVolume > 0f;

    public static void SetSoundVolume(float value)
    {
        SetVolume(SoundVolumeKey, value);
    }

    public static void SetSoundEnabled(bool enabled)
    {
        SetSoundVolume(enabled ? 1f : 0f);
        Save();
    }


    // =====================================================
    // MUSIC
    // =====================================================

    public static float MusicVolume =>
        GetVolume(MusicVolumeKey, MusicKey);

    public static bool MusicEnabled =>
        MusicVolume > 0f;

    public static void SetMusicVolume(float value)
    {
        SetVolume(MusicVolumeKey, value);
    }

    public static void SetMusicEnabled(bool enabled)
    {
        SetMusicVolume(enabled ? 1f : 0f);
        Save();
    }


    // =====================================================
    // HAPTICS
    // =====================================================

    public static bool HapticsEnabled =>
        PlayerPrefs.GetInt(HapticsKey, 1) == 1;

    public static void SetHapticsEnabled(bool enabled)
    {
        if (HapticsEnabled == enabled)
            return;

        PlayerPrefs.SetInt(
            HapticsKey,
            enabled ? 1 : 0
        );

        Save();

        SettingsChanged?.Invoke();
    }


    // =====================================================
    // INTERNAL
    // =====================================================

    private static float GetVolume(
        string volumeKey,
        string legacyKey
    )
    {
        float defaultValue =
            PlayerPrefs.GetInt(legacyKey, 1) == 1
                ? 1f
                : 0f;

        return Mathf.Clamp01(
            PlayerPrefs.GetFloat(
                volumeKey,
                defaultValue
            )
        );
    }

    private static void SetVolume(
        string key,
        float value
    )
    {
        value = Mathf.Clamp01(value);

        PlayerPrefs.SetFloat(key, value);

        SettingsChanged?.Invoke();
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }


    // =====================================================
    // TESTING
    // =====================================================

    public static void ResetForTesting()
    {
        PlayerPrefs.DeleteKey(SoundKey);
        PlayerPrefs.DeleteKey(MusicKey);
        PlayerPrefs.DeleteKey(HapticsKey);
        PlayerPrefs.DeleteKey(SoundVolumeKey);
        PlayerPrefs.DeleteKey(MusicVolumeKey);

        Save();

        SettingsChanged?.Invoke();
    }
}