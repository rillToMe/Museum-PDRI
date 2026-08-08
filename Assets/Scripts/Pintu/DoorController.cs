using System.Collections;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;
    public string openParameterName = "Open";

    [Header("Settings")]
    public bool autoClose = false;
    public float autoCloseDelay = 3f;

    bool isOpen = false;
    Coroutine autoCloseRoutine;

    public void ToggleDoor()
    {
        if (isOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    public void Open()
    {
        if (isOpen) return;

        isOpen = true;
        if (animator != null)
        {
            animator.SetBool(openParameterName, true);
        }

        if (autoClose)
        {
            if (autoCloseRoutine != null)
                StopCoroutine(autoCloseRoutine);

            autoCloseRoutine = StartCoroutine(AutoCloseAfterDelay());
        }
    }

    public void Close()
    {
        if (!isOpen) return;

        isOpen = false;
        if (animator != null)
        {
            animator.SetBool(openParameterName, false);
        }

        if (autoCloseRoutine != null)
        {
            StopCoroutine(autoCloseRoutine);
            autoCloseRoutine = null;
        }
    }

    IEnumerator AutoCloseAfterDelay()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        Close();
    }
}
