using UnityEngine;
using UnityEngine.Video;

public class VideoBGMController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public bool pauseBGMOnPlay = true;
    public bool resumeBGMOnEnd = true;

    void Reset()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();
    }

    void OnEnable()
    {
        if (videoPlayer == null) return;

        videoPlayer.started += OnVideoStarted;
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.errorReceived += OnVideoError;
    }

    void OnDisable()
    {
        if (videoPlayer == null) return;

        videoPlayer.started -= OnVideoStarted;
        videoPlayer.loopPointReached -= OnVideoFinished;
        videoPlayer.errorReceived -= OnVideoError;
    }

    void OnVideoStarted(VideoPlayer vp)
    {
        if (!pauseBGMOnPlay) return;
        if (EnhancedBGMPlayer.Instance != null)
        {
            EnhancedBGMPlayer.Instance.PauseMusic();
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        if (!resumeBGMOnEnd) return;
        if (EnhancedBGMPlayer.Instance != null)
        {
            EnhancedBGMPlayer.Instance.ResumeMusic();
        }
    }

    void OnVideoError(VideoPlayer vp, string message)
    {
        if (resumeBGMOnEnd && EnhancedBGMPlayer.Instance != null)
        {
            EnhancedBGMPlayer.Instance.ResumeMusic();
        }
    }
}
