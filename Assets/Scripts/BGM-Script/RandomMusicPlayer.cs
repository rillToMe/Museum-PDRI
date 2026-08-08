using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnhancedBGMPlayer : MonoBehaviour
{
    public static EnhancedBGMPlayer Instance { get; private set; }
    [Header("Music Settings")]
    public AudioClip[] bgmClips;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float baseVolume = 0.25f;

    [Header("Transition Settings")]
    public float crossfadeDuration = 3f;

    public float pauseBetweenTracks = 1f;

    public float initialFadeIn = 2f;

    [Header("External Fade")]
    public float bgmFadeDuration = 1.5f;


    [Header("Playback Options")]
    public bool shufflePlaylist = true;
    public bool avoidRecentRepeats = true;
    public int recentTracksToAvoid = 3;

    private AudioSource currentSource;
    private AudioSource nextSource;
    private bool isTransitioning = false;
    private List<int> recentIndices = new List<int>();
    private int currentIndex = -1;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    void Start()
    {
        SetupAudioSources();
        DontDestroyOnLoad(gameObject);
        StartCoroutine(InitialPlayback());
    }

    void SetupAudioSources()
    {
        currentSource = gameObject.AddComponent<AudioSource>();
        nextSource = gameObject.AddComponent<AudioSource>();

        ConfigureAudioSource(currentSource);
        ConfigureAudioSource(nextSource);
    }

    void ConfigureAudioSource(AudioSource source)
    {
        source.loop = false;
        source.playOnAwake = false;
        source.volume = 0f;
        source.priority = 128;
        source.spatialBlend = 0f; 
    }

    IEnumerator InitialPlayback()
    {
        if (bgmClips.Length == 0)
        {
            Debug.LogWarning("No BGM clips assigned!");
            yield break;
        }

        currentIndex = GetNextTrackIndex();
        currentSource.clip = bgmClips[currentIndex];
        currentSource.Play();

        yield return StartCoroutine(FadeIn(currentSource, initialFadeIn));

        StartCoroutine(BGMLoop());
    }

    IEnumerator BGMLoop()
    {
        while (true)
        {
            float timeUntilEnd = currentSource.clip.length - currentSource.time;

            if (timeUntilEnd <= crossfadeDuration + pauseBetweenTracks && !isTransitioning)
            {
                yield return StartCoroutine(TransitionToNextTrack());
            }

            yield return null;
        }
    }

    IEnumerator TransitionToNextTrack()
    {
        isTransitioning = true;

        int nextIndex = GetNextTrackIndex();
        nextSource.clip = bgmClips[nextIndex];

        if (pauseBetweenTracks > 0)
        {
            yield return StartCoroutine(FadeOut(currentSource, pauseBetweenTracks));
            yield return new WaitForSeconds(pauseBetweenTracks);
        }

        nextSource.Play();

        yield return StartCoroutine(Crossfade(currentSource, nextSource, crossfadeDuration));

        AudioSource temp = currentSource;
        currentSource = nextSource;
        nextSource = temp;

        currentIndex = nextIndex;
        isTransitioning = false;
    }

    IEnumerator FadeIn(AudioSource source, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, baseVolume, elapsed / duration);
            yield return null;
        }

        source.volume = baseVolume;
    }

    IEnumerator FadeOut(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        source.volume = 0f;
    }

    IEnumerator Crossfade(AudioSource fadeOutSource, AudioSource fadeInSource, float duration)
    {
        float elapsed = 0f;
        float startVolumeOut = fadeOutSource.volume;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            fadeOutSource.volume = Mathf.Lerp(startVolumeOut, 0f, smoothT);
            fadeInSource.volume = Mathf.Lerp(0f, baseVolume, smoothT);

            yield return null;
        }

        fadeOutSource.volume = 0f;
        fadeInSource.volume = baseVolume;
        fadeOutSource.Stop();
    }

    IEnumerator FadeVolume(AudioSource source, float from, float to, float duration, bool pauseWhenDone)
    {
        if (source == null) yield break;

        float t = 0f;
        source.volume = from;

        if (!source.isPlaying && !pauseWhenDone)
            source.Play();

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            source.volume = Mathf.Lerp(from, to, k);
            yield return null;
        }

        source.volume = to;

        if (pauseWhenDone && to <= 0.001f)
            source.Pause();
    }


    int GetNextTrackIndex()
    {
        if (bgmClips.Length == 1) return 0;

        int nextIndex;

        if (shufflePlaylist)
        {
            int attempts = 0;
            do
            {
                nextIndex = Random.Range(0, bgmClips.Length);
                attempts++;

                if (attempts > bgmClips.Length * 2) break;

            } while (avoidRecentRepeats && recentIndices.Contains(nextIndex));

            recentIndices.Add(nextIndex);
            if (recentIndices.Count > Mathf.Min(recentTracksToAvoid, bgmClips.Length - 1))
            {
                recentIndices.RemoveAt(0);
            }
        }
        else
        {
            nextIndex = (currentIndex + 1) % bgmClips.Length;
        }

        return nextIndex;
    }

    public void SetVolume(float volume)
    {
        baseVolume = Mathf.Clamp01(volume);
        if (currentSource != null) currentSource.volume = baseVolume;
    }

    public void SkipToNextTrack()
    {
        if (!isTransitioning)
        {
            StopAllCoroutines();
            StartCoroutine(ForceTransition());
        }
    }

    IEnumerator ForceTransition()
    {
        yield return StartCoroutine(TransitionToNextTrack());
        StartCoroutine(BGMLoop());
    }


    public void PauseMusic()
    {
        if (currentSource == null) return;

        StopCoroutine(nameof(FadeVolume));
        StartCoroutine(FadeVolume(currentSource, currentSource.volume, 0f, bgmFadeDuration, true));
    }


    public void ResumeMusic()
    {
        if (currentSource == null) return;

        currentSource.UnPause();
        StopCoroutine(nameof(FadeVolume));
        StartCoroutine(FadeVolume(currentSource, currentSource.volume, baseVolume, bgmFadeDuration, false));
    }



    public string GetCurrentTrackName()
    {
        return currentSource != null && currentSource.clip != null
            ? currentSource.clip.name
            : "None";
    }
}