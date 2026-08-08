using System.Collections;
using UnityEngine;

public class NissanInfoTrigger : MonoBehaviour
{
    [Header("Target UI")]
    public CanvasGroup uiGroup;

    [Header("Settings")]
    public float fadeDuration = 0.5f;
    public string playerTag = "Player";

    Coroutine fadeRoutine;

    void Awake()
    {
        if (uiGroup != null)
        {
            uiGroup.alpha = 0f;
            uiGroup.interactable = false;
            uiGroup.blocksRaycasts = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        StartFade(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        StartFade(false);
    }

    void StartFade(bool show)
    {
        if (uiGroup == null) return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(Fade(show));
    }

    IEnumerator Fade(bool show)
    {
        float start = uiGroup.alpha;
        float end = show ? 1f : 0f;
        float t = 0f;

        if (show)
        {
            uiGroup.interactable = true;
            uiGroup.blocksRaycasts = true;
        }

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float k = t / fadeDuration;
            uiGroup.alpha = Mathf.Lerp(start, end, k);
            yield return null;
        }

        uiGroup.alpha = end;

        if (!show)
        {
            uiGroup.interactable = false;
            uiGroup.blocksRaycasts = false;
        }
    }
}
