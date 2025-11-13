using UnityEngine;

public sealed class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip throwClip;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip captureSuccessClip;
    [SerializeField] private AudioClip captureFailClip;

    [Header("Volumes")]
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 0.7f;
    [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.3f;

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeSingleton();
        ConfigureAudioSources();
    }

    #endregion

    #region Initialization

    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void ConfigureAudioSources()
    {
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;

        if (musicSource != null)
            musicSource.volume = musicVolume;
    }

    #endregion

    #region Public API - Sound Effects

    public void PlayThrowSound() => PlaySfx(throwClip, "Som de arremesso");
    public void PlayHitSound() => PlaySfx(hitClip, "Som de impacto");
    public void PlayCaptureSuccessSound() => PlaySfx(captureSuccessClip, "Som de captura bem-sucedida");
    public void PlayCaptureFailSound() => PlaySfx(captureFailClip, "Som de falha");

    public void PlaySound(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, volumeScale);
    }

    public void StopAllSounds()
    {
        sfxSource?.Stop();
        musicSource?.Stop();
    }

    #endregion

    #region Private Helpers

    private void PlaySfx(AudioClip clip, string logMessage)
    {
        if (clip == null || sfxSource == null) return;

        sfxSource.PlayOneShot(clip);
        #if UNITY_EDITOR
            Debug.Log($"🔊 {logMessage}");
        #endif
    }

    #endregion
}
