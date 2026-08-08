using UnityEngine;
using UnityEngine.UI;

public class FilmUIController : MonoBehaviour
{
    [Header("Referensi")]
    public FilmTrigger filmTrigger;
    public Button playButton;
    public Button pauseButton;
    public Button resumeButton;
    public Button nextFilmButton;   
    public Toggle autoPlayToggle;

    [Header("Player Head (VR Camera)")]
    public Transform playerHead;
    public bool lockYRotation = true;

    void Start()
    {
        if (filmTrigger != null && autoPlayToggle != null)
        {
            autoPlayToggle.isOn = filmTrigger.autoPlayOnEnter;
        }

        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        if (pauseButton != null)
            pauseButton.onClick.AddListener(OnPauseClicked);

        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(OnResumeClicked);
            resumeButton.gameObject.SetActive(false);
        }

        if (nextFilmButton != null)
        {
            nextFilmButton.onClick.AddListener(OnNextFilmClicked);
            nextFilmButton.gameObject.SetActive(false);
        }

        if (autoPlayToggle != null)
            autoPlayToggle.onValueChanged.AddListener(OnAutoPlayChanged);
    }

    void LateUpdate()
    {
        if (playerHead != null)
        {
            Vector3 dir = playerHead.position - transform.position;
            if (lockYRotation) dir.y = 0f;

            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        }

        if (filmTrigger == null) return;

        if (resumeButton != null)
        {
            bool showResume = !filmTrigger.autoPlayOnEnter && filmTrigger.CanResume;
            if (resumeButton.gameObject.activeSelf != showResume)
                resumeButton.gameObject.SetActive(showResume);
        }

        if (nextFilmButton != null)
        {
            bool showNext = filmTrigger.HasMultipleFilms;
            if (nextFilmButton.gameObject.activeSelf != showNext)
                nextFilmButton.gameObject.SetActive(showNext);
        }
    }

    void OnPlayClicked()
    {
        if (filmTrigger == null) return;
        filmTrigger.TriggerPlay();
    }

    void OnPauseClicked()
    {
        if (filmTrigger == null) return;
        filmTrigger.TriggerPause(true);
    }

    void OnResumeClicked()
    {
        if (filmTrigger == null) return;
        filmTrigger.TriggerResume();
    }

    void OnNextFilmClicked()
    {
        if (filmTrigger == null) return;
        filmTrigger.NextFilm();
    }

    void OnAutoPlayChanged(bool value)
    {
        if (filmTrigger == null) return;
        filmTrigger.autoPlayOnEnter = value;
    }
}
