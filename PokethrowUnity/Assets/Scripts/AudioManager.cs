using UnityEngine;

/// <summary>
/// Gerenciador de áudio do jogo
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource sfxSource;      // Para efeitos sonoros
    public AudioSource musicSource;    // Para música de fundo

    [Header("Sound Effects")]
    public AudioClip throwSound;       // Som ao arremessar
    public AudioClip hitSound;         // Som ao acertar
    public AudioClip captureSuccess;   // Som de captura bem-sucedida
    public AudioClip captureFail;      // Som de captura falhou

    [Header("Configurações")]
    [Range(0f, 1f)]
    public float sfxVolume = 0.7f;
    [Range(0f, 1f)]
    public float musicVolume = 0.3f;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Configura volumes
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;

        if (musicSource != null)
            musicSource.volume = musicVolume;
    }

    /// <summary>
    /// Toca som de arremesso
    /// </summary>
    public void PlayThrowSound()
    {
        if (throwSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(throwSound);
            Debug.Log("🔊 Som de arremesso!");
        }
    }

    /// <summary>
    /// Toca som de impacto/acerto
    /// </summary>
    public void PlayHitSound()
    {
        if (hitSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(hitSound);
            Debug.Log("🔊 Som de impacto!");
        }
    }

    /// <summary>
    /// Toca som de captura bem-sucedida
    /// </summary>
    public void PlayCaptureSuccessSound()
    {
        if (captureSuccess != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(captureSuccess);
            Debug.Log("🔊 Som de captura bem-sucedida!");
        }
    }

    /// <summary>
    /// Toca som de captura falhou
    /// </summary>
    public void PlayCaptureFailSound()
    {
        if (captureFail != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(captureFail);
            Debug.Log("🔊 Som de falha!");
        }
    }

    /// <summary>
    /// Toca um som customizado
    /// </summary>
    public void PlaySound(AudioClip clip, float volumeScale = 1f)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volumeScale);
        }
    }

    /// <summary>
    /// Para todos os sons
    /// </summary>
    public void StopAllSounds()
    {
        if (sfxSource != null)
            sfxSource.Stop();

        if (musicSource != null)
            musicSource.Stop();
    }
}