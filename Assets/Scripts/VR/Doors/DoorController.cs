using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRDoubleDoorAdvanced : MonoBehaviour
{
    [Header("Animator")]
    public Animator doorAnimator;

    public string openParamName = "IsOpen";

    [Header("State awal")]
    public bool startOpen = false;

    public float openAnimDuration = 0.8f;

    public float closeAnimDuration = 0.8f;

    [Header("Interaksi XR")]
    public XRBaseInteractable xrInteractable;

    public bool useSelectEvent = true;

    [Header("Auto close")]
    public bool autoClose = false;

    public float autoCloseDelay = 3f;

    [Header("Lock system")]
    public bool isLocked = false;

    public string lockedDebugMessage = "Door is locked.";

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip openClip;
    public AudioClip closeClip;
    public AudioClip lockedClip;

    bool isOpen;
    bool isBusy;
    Coroutine autoCloseRoutine;

    void Reset()
    {
        if (doorAnimator == null)
            doorAnimator = GetComponent<Animator>();

        if (xrInteractable == null)
            xrInteractable = GetComponent<XRBaseInteractable>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Awake()
    {
        if (doorAnimator == null)
            doorAnimator = GetComponent<Animator>();

        if (xrInteractable == null)
            xrInteractable = GetComponent<XRBaseInteractable>();

        SetupXRCallbacks();
    }

    void Start()
    {
        isOpen = startOpen;

        if (doorAnimator != null && !string.IsNullOrEmpty(openParamName))
            doorAnimator.SetBool(openParamName, isOpen);

        if (isOpen && autoClose && autoCloseDelay > 0f)
        {
            autoCloseRoutine = StartCoroutine(AutoCloseAfterDelay());
        }
    }

    void SetupXRCallbacks()
    {
        if (xrInteractable == null)
            return;

        xrInteractable.selectEntered.RemoveListener(OnSelectEntered);
        xrInteractable.activated.RemoveListener(OnActivated);

        if (useSelectEvent)
        {
            xrInteractable.selectEntered.AddListener(OnSelectEntered);
        }
        else
        {
            xrInteractable.activated.AddListener(OnActivated);
        }
    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        TryInteract();
    }

    void OnActivated(ActivateEventArgs args)
    {
        TryInteract();
    }


    void TryInteract()
    {
        if (isLocked)
        {
            if (lockedClip != null && audioSource != null)
                audioSource.PlayOneShot(lockedClip);

            if (!string.IsNullOrEmpty(lockedDebugMessage))
                Debug.Log(lockedDebugMessage);

            return;
        }

        if (isBusy)
        {
            return;
        }

        if (isOpen)
            StartCoroutine(CloseRoutine());
        else
            StartCoroutine(OpenRoutine());
    }

    IEnumerator OpenRoutine()
    {
        isBusy = true;

        if (autoCloseRoutine != null)
        {
            StopCoroutine(autoCloseRoutine);
            autoCloseRoutine = null;
        }

        SetAnimatorOpen(true);
        PlayOneShotSafe(openClip);

        yield return new WaitForSeconds(openAnimDuration);

        isOpen = true;
        isBusy = false;

        if (autoClose && autoCloseDelay > 0f)
        {
            autoCloseRoutine = StartCoroutine(AutoCloseAfterDelay());
        }
    }

    IEnumerator CloseRoutine()
    {
        isBusy = true;

        if (autoCloseRoutine != null)
        {
            StopCoroutine(autoCloseRoutine);
            autoCloseRoutine = null;
        }

        SetAnimatorOpen(false);
        PlayOneShotSafe(closeClip);

        yield return new WaitForSeconds(closeAnimDuration);

        isOpen = false;
        isBusy = false;
    }

    IEnumerator AutoCloseAfterDelay()
    {
        yield return new WaitForSeconds(autoCloseDelay);

        if (isOpen && !isBusy && !isLocked)
        {
            yield return CloseRoutine();
        }

        autoCloseRoutine = null;
    }

    void SetAnimatorOpen(bool open)
    {
        if (doorAnimator != null && !string.IsNullOrEmpty(openParamName))
            doorAnimator.SetBool(openParamName, open);
    }

    void PlayOneShotSafe(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    public void LockDoor()
    {
        isLocked = true;
    }

    public void UnlockDoor()
    {
        isLocked = false;
    }

    public void ForceOpen()
    {
        if (isBusy)
            return;

        StartCoroutine(OpenRoutine());
    }

    public void ForceClose()
    {
        if (isBusy)
            return;

        StartCoroutine(CloseRoutine());
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isOpen ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.25f);
    }
}
