using UnityEngine;


public class AudioManager :
    MonoBehaviour
{
    // =====================================================
    // SINGLETON
    // =====================================================

    public static AudioManager Instance
    {
        get;
        private set;
    }


    // =====================================================
    // SOURCES
    // =====================================================

    [Header("Audio Sources")]

    [SerializeField]
    private AudioSource musicSource;


    [SerializeField]
    private AudioSource sfxSource;


    [Header("Library")]

[SerializeField]
private GameAudioLibrary audioLibrary;


    [Header("Volume")]

    [SerializeField]
    [Range(0f, 1f)]
    private float musicVolume =
        0.65f;


    [SerializeField]
    [Range(0f, 1f)]
    private float sfxVolume =
        1f;


    // =====================================================
    // UNITY
    // =====================================================

    private void Awake()
    {
        if (
            Instance != null &&
            Instance != this
        )
        {
            Destroy(
                gameObject
            );

            return;
        }


        Instance =
            this;


        DontDestroyOnLoad(
            gameObject
        );


        ApplySettings();
    }


    private void OnEnable()
    {
        GameSettings.SettingsChanged +=
            ApplySettings;
    }


    private void OnDisable()
    {
        GameSettings.SettingsChanged -=
            ApplySettings;
    }


    // =====================================================
    // SETTINGS
    // =====================================================

    private void ApplySettings()
{
    if (musicSource != null)
    {
        musicSource.volume =
            musicVolume * GameSettings.MusicVolume;

        musicSource.mute =
            !GameSettings.MusicEnabled;

        if (
            GameSettings.MusicEnabled &&
            musicSource.clip != null &&
            !musicSource.isPlaying
        )
        {
            musicSource.Play();
        }
    }

    if (sfxSource != null)
    {
        sfxSource.volume =
            sfxVolume * GameSettings.SoundVolume;

        sfxSource.mute =
            !GameSettings.SoundEnabled;
    }
}

    // =====================================================
    // SFX
    // =====================================================

    public void PlaySfx(
        AudioClip clip
    )
    {
        PlaySfx(
            clip,
            1f
        );
    }


    public void PlaySfx(
    GameSfx type
)
{
    PlaySfx(
        type,
        1f
    );
}


public void PlaySfx(
    GameSfx type,
    float volumeScale
)
{
    if (audioLibrary == null)
    {
        return;
    }


    AudioClip clip =
        audioLibrary.GetClip(
            type
        );


    PlaySfx(
        clip,
        volumeScale
    );
}


    public void PlaySfx(
        AudioClip clip,
        float volumeScale
    )
    {
        if (
            clip == null ||
            sfxSource == null ||
            !GameSettings.SoundEnabled
        )
        {
            return;
        }


        sfxSource.PlayOneShot(
            clip,
            Mathf.Clamp01(
                volumeScale
            )
        );
    }


    // =====================================================
    // MUSIC
    // =====================================================

    public void PlayMusic(
        AudioClip clip,
        bool restart = false
    )
    {
        if (
            clip == null ||
            musicSource == null
        )
        {
            return;
        }


        if (
            musicSource.clip == clip &&
            musicSource.isPlaying &&
            !restart
        )
        {
            return;
        }


        musicSource.clip =
            clip;


        musicSource.loop =
            true;


        if (
            GameSettings.MusicEnabled
        )
        {
            musicSource.Play();
        }
    }


    public void StopMusic()
    {
        if (musicSource == null)
        {
            return;
        }


        musicSource.Stop();


        musicSource.clip =
            null;
    }
}