using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class FilmTrigger : MonoBehaviour
{
    [Header("Referensi")]
    public VideoPlayer videoPlayer;
    public Canvas infoCanvas;
    public AudioSource videoAudio;
    public Renderer screenRenderer;

    [Header("Film List")]
    public VideoClip[] filmClips;
    public int currentFilmIndex = 0;

    [Header("Pengaturan")]
    public bool autoPlayOnEnter = true;
    public bool pauseOnExit = true;
    public bool resumeBGMOnExit = true;

    [Header("Durasi Fade")]
    public float videoFadeDuration = 2f;
    public float videoExitFadeDuration = 1.2f;

    [Header("State")]
    public bool CanResume = false;

    Color originalColor = Color.white;
    float originalVideoVolume = 1f;

    Coroutine fadeRoutine;

    public bool HasMultipleFilms
    {
        get { return filmClips != null && filmClips.Length > 1; }
    }

    void Start()
    {
        CanResume = false;

        if (infoCanvas != null)
            infoCanvas.enabled = false;

        if (screenRenderer != null)
        {
            originalColor = screenRenderer.material.color;
        }

        if (videoAudio != null)
        {
            originalVideoVolume = videoAudio.volume;
            videoAudio.volume = 0f;
        }

        if (videoPlayer != null && filmClips != null && filmClips.Length > 0)
        {
            currentFilmIndex = Mathf.Clamp(currentFilmIndex, 0, filmClips.Length - 1);
            videoPlayer.clip = filmClips[currentFilmIndex];
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (infoCanvas != null)
            infoCanvas.enabled = true;

        if (autoPlayOnEnter && videoPlayer != null)
        {
            TriggerPlay();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (infoCanvas != null)
            infoCanvas.enabled = false;

        if (videoPlayer == null) return;

        if (pauseOnExit && videoPlayer.isPlaying)
        {
            TriggerPause(resumeBGMOnExit);

            if (!autoPlayOnEnter &&
                videoPlayer.length > 0.1 &&
                videoPlayer.time > 0.1 &&
                videoPlayer.time < videoPlayer.length - 0.1)
            {
                CanResume = true;
            }
            else
            {
                CanResume = false;
            }
        }
        else
        {
            if (resumeBGMOnExit && EnhancedBGMPlayer.Instance != null)
            {
                EnhancedBGMPlayer.Instance.ResumeMusic();
            }

            CanResume = false;
        }
    }

    public void TriggerPlay()
    {
        if (videoPlayer == null) return;

        CanResume = false;

        videoPlayer.Stop();
        videoPlayer.time = 0;
        videoPlayer.frame = 0;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeInVideoAndAudio());
    }

    public void TriggerResume()
    {
        if (videoPlayer == null) return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeInVideoAndAudio());
        CanResume = false;
    }

    public void TriggerPause(bool resumeBGM)
    {
        if (videoPlayer == null) return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeOutVideoAndAudio(resumeBGM));
    }

    public void NextFilm()
    {
        if (!HasMultipleFilms || videoPlayer == null) return;

        currentFilmIndex = (currentFilmIndex + 1) % filmClips.Length;
        CanResume = false;

        videoPlayer.Stop();
        videoPlayer.time = 0;
        videoPlayer.frame = 0;
        videoPlayer.clip = filmClips[currentFilmIndex];

        if (autoPlayOnEnter)
        {
            if (fadeRoutine != null)
                StopCoroutine(fadeRoutine);

            fadeRoutine = StartCoroutine(FadeInVideoAndAudio());
        }
        else
        {
            if (screenRenderer != null)
                screenRenderer.material.color = originalColor;

            if (videoAudio != null)
                videoAudio.volume = 0f;
        }
    }

    IEnumerator FadeInVideoAndAudio()
    {
        if (EnhancedBGMPlayer.Instance != null)
            EnhancedBGMPlayer.Instance.PauseMusic();

        if (videoPlayer != null && !videoPlayer.isPlaying)
            videoPlayer.Play();

        float t = 0f;

        if (screenRenderer != null)
            screenRenderer.material.color = Color.black;

        if (videoAudio != null)
            videoAudio.volume = 0f;

        while (t < videoFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / videoFadeDuration);

            if (screenRenderer != null)
            {
                Color c = Color.Lerp(Color.black, originalColor, k);
                screenRenderer.material.color = c;
            }

            if (videoAudio != null)
            {
                videoAudio.volume = Mathf.Lerp(0f, originalVideoVolume, k);
            }

            yield return null;
        }

        if (screenRenderer != null)
            screenRenderer.material.color = originalColor;

        if (videoAudio != null)
            videoAudio.volume = originalVideoVolume;
    }

    IEnumerator FadeOutVideoAndAudio(bool resumeBGM)
    {
        float t = 0f;

        Color startColor = originalColor;
        if (screenRenderer != null)
            startColor = screenRenderer.material.color;

        float startVol = originalVideoVolume;
        if (videoAudio != null)
            startVol = videoAudio.volume;

        while (t < videoExitFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / videoExitFadeDuration);

            if (screenRenderer != null)
            {
                Color c = Color.Lerp(startColor, Color.black, k);
                screenRenderer.material.color = c;
            }

            if (videoAudio != null)
            {
                videoAudio.volume = Mathf.Lerp(startVol, 0f, k);
            }

            yield return null;
        }

        if (screenRenderer != null)
            screenRenderer.material.color = Color.black;

        if (videoAudio != null)
            videoAudio.volume = 0f;

        if (videoPlayer != null && videoPlayer.isPlaying)
            videoPlayer.Pause();

        if (resumeBGM && EnhancedBGMPlayer.Instance != null)
        {
            EnhancedBGMPlayer.Instance.ResumeMusic();
        }
    }
}
