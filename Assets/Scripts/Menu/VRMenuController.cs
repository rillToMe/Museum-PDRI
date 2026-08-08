using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class VRMenuController : MonoBehaviour
{
    [Header("Referensi utama")]
    public GameObject menuCanvas;
    public Transform playerHead;
    public float distanceFromHead = 1.2f;
    public float heightOffset = -0.1f;

    [Header("Input VR")]
    public InputActionProperty menuAction;   // ganti openMenuAction + showButton jadi satu

    [Header("Objek main menu")]
    public GameObject mainMenuTitle;
    public GameObject btnResumeObj;
    public GameObject btnQuitObj;

    [Header("Haptic / getaran")]
    public XRBaseController leftController;
    public XRBaseController rightController;
    public float hoverAmplitude = 0.2f;
    public float hoverDuration = 0.04f;
    public float clickAmplitude = 0.5f;
    public float clickDuration = 0.08f;

    [Header("Audio UI")]
    public AudioSource uiAudioSource;
    public AudioClip hoverClip;
    public AudioClip clickClip;

    private bool isOpen = false;

    void Start()
    {
        if (menuCanvas != null)
            menuCanvas.SetActive(false);

        ShowMainMenu();
    }

    void Update()
    {
        // Keyboard ESC
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleMenu();
        }

        // Tombol menu di controller
        if (menuAction.action != null && menuAction.action.WasPressedThisFrame())
        {
            ToggleMenu();
        }

        // Update posisi dan rotasi menu biar selalu hadap ke kepala
        if (isOpen)
        {
            UpdateMenuTransform();
        }
    }

    public void ToggleMenu()
    {
        isOpen = !isOpen;

        if (menuCanvas != null)
            menuCanvas.SetActive(isOpen);

        if (isOpen)
        {
            ShowMainMenu();
            UpdateMenuTransform();
        }
    }

    private void UpdateMenuTransform()
    {
        if (playerHead == null || menuCanvas == null) return;

        // Posisi di depan kepala, rata di sumbu Y tapi ada offset tinggi
        Vector3 flatForward = new Vector3(playerHead.forward.x, 0f, playerHead.forward.z).normalized;
        Vector3 targetPos = playerHead.position + flatForward * distanceFromHead + new Vector3(0f, heightOffset, 0f);

        menuCanvas.transform.position = targetPos;

        // Hadap ke player
        Vector3 lookPos = new Vector3(playerHead.position.x, menuCanvas.transform.position.y, playerHead.position.z);
        menuCanvas.transform.LookAt(lookPos);
        menuCanvas.transform.forward *= -1f;
    }

    public void ShowMainMenu()
    {
        if (mainMenuTitle != null) mainMenuTitle.SetActive(true);

        if (btnResumeObj != null) btnResumeObj.SetActive(true);
        if (btnQuitObj != null) btnQuitObj.SetActive(true);
    }

    public void ResumeGame()
    {
        PlayClickFeedback();
        ToggleMenu();
    }

    public void QuitGame()
    {
        PlayClickFeedback();
        Application.Quit();
    }

    public void OnButtonHover(GameObject buttonObj)
    {
        PlayHoverFeedback();

        if (buttonObj != null)
            buttonObj.transform.localScale = Vector3.one * 1.05f;
    }

    public void OnButtonExit(GameObject buttonObj)
    {
        if (buttonObj != null)
            buttonObj.transform.localScale = Vector3.one;
    }

    public void OnButtonHover()
    {
        PlayHoverFeedback();
    }

    private void PlayHoverFeedback()
    {
        if (uiAudioSource != null && hoverClip != null)
            uiAudioSource.PlayOneShot(hoverClip);

        SendHaptics(hoverAmplitude, hoverDuration);
    }

    private void PlayClickFeedback()
    {
        if (uiAudioSource != null && clickClip != null)
            uiAudioSource.PlayOneShot(clickClip);

        SendHaptics(clickAmplitude, clickDuration);
    }

    private void SendHaptics(float amp, float dur)
    {
        if (leftController != null)
            leftController.SendHapticImpulse(amp, dur);

        if (rightController != null)
            rightController.SendHapticImpulse(amp, dur);
    }
}
