using System;


public static class GameSettings
{
    private const string SoundKey =
        "Puzzle_Settings_Sound";


    private const string MusicKey =
        "Puzzle_Settings_Music";


    private const string HapticsKey =
        "Puzzle_Settings_Haptics";


    public static event Action
        SettingsChanged;


    // =====================================================
    // SOUND
    // =====================================================

    public static bool SoundEnabled =>
        GetBool(
            SoundKey,
            true
        );


    public static void SetSoundEnabled(
        bool enabled
    )
    {
        SetBool(
            SoundKey,
            enabled
        );
    }


    // =====================================================
    // MUSIC
    // =====================================================

    public static bool MusicEnabled =>
        GetBool(
            MusicKey,
            true
        );


    public static void SetMusicEnabled(
        bool enabled
    )
    {
        SetBool(
            MusicKey,
            enabled
        );
    }


    // =====================================================
    // HAPTICS
    // =====================================================

    public static bool HapticsEnabled =>
        GetBool(
            HapticsKey,
            true
        );


    public static void SetHapticsEnabled(
        bool enabled
    )
    {
        SetBool(
            HapticsKey,
            enabled
        );
    }


    // =====================================================
    // INTERNAL
    // =====================================================

    private static bool GetBool(
        string key,
        bool defaultValue
    )
    {
        return
            UnityEngine.PlayerPrefs.GetInt(
                key,
                defaultValue
                    ? 1
                    : 0
            )
            == 1;
    }


    private static void SetBool(
        string key,
        bool value
    )
    {
        UnityEngine.PlayerPrefs.SetInt(
            key,
            value
                ? 1
                : 0
        );


        UnityEngine.PlayerPrefs.Save();


        SettingsChanged?.Invoke();
    }


    // =====================================================
    // TESTING
    // =====================================================

    public static void ResetForTesting()
    {
        UnityEngine.PlayerPrefs.DeleteKey(
            SoundKey
        );


        UnityEngine.PlayerPrefs.DeleteKey(
            MusicKey
        );


        UnityEngine.PlayerPrefs.DeleteKey(
            HapticsKey
        );


        UnityEngine.PlayerPrefs.Save();


        SettingsChanged?.Invoke();
    }
}