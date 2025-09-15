using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Sources Audio (à assigner dans l'Inspector)")]
    [Tooltip("Source Audio pour la musique de fond")]
    public AudioSource musicSource;
    [Tooltip("Source Audio pour les effets sonores")]
    public AudioSource sfxSource;

    [Header("Listes de Clips (à remplir dans l'Inspector)")]
    [Tooltip("Liste des AudioClip pour les musiques (leur 'name' sera utilisé pour les jouer)")]
    public List<AudioClip> musicClips = new List<AudioClip>();
    [Tooltip("Liste des AudioClip pour les effets sonores (leur 'name' sera utilisé pour les jouer)")]
    public List<AudioClip> sfxClips = new List<AudioClip>();

    public UnityEngine.UI.Slider musicSlider;
    public UnityEngine.UI.Slider sfxSlider;

    [Header("Volumes (0.0 à 1.0)")]
    [Range(0f, 1f)] public float musicVolume = 1.0f;
    [Range(0f, 1f)] public float sfxVolume = 1.0f;

    // --- Paramètre optionnel : petite marge après lecture avant destruction (évite cut trop tôt) ---
    private const float TempSourceTailSeconds = 0.05f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (musicSource == null)
                musicSource = gameObject.AddComponent<AudioSource>();
            if (sfxSource == null)
                sfxSource = gameObject.AddComponent<AudioSource>();

            musicSource.loop = true;
            musicSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume;
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }

    private void Update()
    {
        if (musicSlider != null)
        {
            musicVolume = musicSlider.value;
            if (musicSource != null) musicSource.volume = musicVolume;
        }

        if (sfxSlider != null)
        {
            sfxVolume = sfxSlider.value;
            if (sfxSource != null) sfxSource.volume = sfxVolume;
        }
    }

    public void PlayMusic(string name)
    {
        AudioClip clip = musicClips.Find(c => c != null && c.name == name);

        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.volume = musicVolume;
            musicSource.loop = true;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning($"[SoundManager] PlayMusic : impossible de trouver une musique nommée « {name} » dans musicClips.");
        }
    }

    public void PlaySFX(string name)
    {
        AudioClip clip = sfxClips.Find(c => c != null && c.name == name);

        if (clip != null)
        {
            // >>> Au lieu de jouer sur sfxSource directement, on crée une source temporaire
            StartCoroutine(PlayClipOnTempSfxSource(clip, 1f));
        }
        else
        {
            Debug.LogWarning($"[SoundManager] PlaySFX : impossible de trouver un effet nommé « {name} » dans sfxClips.");
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }

    public void PlayRandomSFX(List<string> clipNames, float minPitch, float maxPitch)
    {
        if (clipNames == null || clipNames.Count == 0)
        {
            Debug.LogWarning("[SoundManager] PlayRandomSFX : la liste de noms est vide ou nulle.");
            return;
        }
        if (minPitch < 0f || maxPitch < minPitch)
        {
            Debug.LogWarning($"[SoundManager] PlayRandomSFX : bornes de pitch invalides (minPitch={minPitch}, maxPitch={maxPitch}).");
            return;
        }

        int randomIndex = Random.Range(0, clipNames.Count);
        string randomName = clipNames[randomIndex];

        AudioClip clip = sfxClips.Find(c => c != null && c.name == randomName);
        if (clip == null)
        {
            Debug.LogWarning($"[SoundManager] PlayRandomSFX : impossible de trouver le clip nommé « {randomName} » dans sfxClips.");
            return;
        }

        float randomPitch = Random.Range(minPitch, maxPitch);

        // >>> Chaque SFX aléatoire joue sur sa source temporaire avec son pitch dédié
        StartCoroutine(PlayClipOnTempSfxSource(clip, randomPitch));
    }

    public void FadeMusic(float duration)
    {
        if (musicSource == null || !musicSource.isPlaying)
            return;

        StartCoroutine(FadeMusicCoroutine(duration));
    }

    private IEnumerator FadeMusicCoroutine(float duration)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        musicSource.volume = 0f;
        musicSource.Stop();
        musicSource.clip = null;
        musicSource.volume = musicVolume;
    }

    // ============================
    //     Helpers privés SFX
    // ============================

    private IEnumerator PlayClipOnTempSfxSource(AudioClip clip, float pitch)
    {
        if (clip == null) yield break;

        AudioSource temp = CreateTempSfxSource(pitch);

        // On fixe le volume au moment de la création (cohérent avec slider/SetSFXVolume)
        temp.volume = sfxSlider != null ? sfxSlider.value : sfxVolume;

        // Utiliser clip + Play() (plutôt que PlayOneShot) pour un contrôle total de la durée/pitch
        temp.clip = clip;
        temp.loop = false;
        temp.Play();

        float dur = clip.length / Mathf.Max(0.01f, pitch);
        yield return new WaitForSeconds(dur + TempSourceTailSeconds);

        if (temp != null) Destroy(temp.gameObject);
    }

    private AudioSource CreateTempSfxSource(float pitch)
    {
        // Parenté sous la sfxSource si possible, sinon sous ce SoundManager
        Transform parent = (sfxSource != null) ? sfxSource.transform : transform;

        GameObject go = new GameObject("SFX_Temp");
        go.transform.SetParent(parent, false);

        AudioSource src = go.AddComponent<AudioSource>();

        // Copie les réglages importants depuis sfxSource (mixeur, 2D/3D, distances…)
        if (sfxSource != null)
        {
            src.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;
            src.spatialBlend = sfxSource.spatialBlend;
            src.dopplerLevel = sfxSource.dopplerLevel;
            src.rolloffMode = sfxSource.rolloffMode;
            src.minDistance = sfxSource.minDistance;
            src.maxDistance = sfxSource.maxDistance;
            src.reverbZoneMix = sfxSource.reverbZoneMix;
            src.bypassEffects = sfxSource.bypassEffects;
            src.bypassListenerEffects = sfxSource.bypassListenerEffects;
            src.bypassReverbZones = sfxSource.bypassReverbZones;
            src.priority = sfxSource.priority;
            src.panStereo = sfxSource.panStereo;
            src.spatialize = sfxSource.spatialize;
            src.spread = sfxSource.spread;
            src.ignoreListenerPause = sfxSource.ignoreListenerPause;
            src.ignoreListenerVolume = sfxSource.ignoreListenerVolume;
            src.mute = sfxSource.mute;
        }

        src.playOnAwake = false;
        src.loop = false;
        src.pitch = pitch;

        return src;
    }
}
