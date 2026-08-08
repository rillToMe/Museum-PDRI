using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class WelcomeGate : MonoBehaviour
{
    [Header("UI")]
    public GameObject welcomePanel;         
    public GameObject continueButton;       

    [Header("Locomotion")]
    public ActionBasedContinuousMoveProvider moveProvider;
    public ActionBasedContinuousTurnProvider turnProvider;
    public TeleportationProvider teleportProvider;

    [Header("Config")]
    public bool onlyFirstTime = true;
    public string playerPrefsKey = "MuseumWelcomeSeen";

    void Start()
    {
        bool alreadySeen = onlyFirstTime && PlayerPrefs.GetInt(playerPrefsKey, 0) == 1;

        if (alreadySeen)
        {
            SetUI(false);
            SetLocomotionEnabled(true);
        }
        else
        {
            SetUI(true);
            SetLocomotionEnabled(false);
        }
    }

    public void OnClickContinue()
    {
        if (onlyFirstTime)
            PlayerPrefs.SetInt(playerPrefsKey, 1);

        SetUI(false);
        SetLocomotionEnabled(true);
    }

    void SetUI(bool state)
    {
        if (welcomePanel != null)
            welcomePanel.SetActive(state);

        if (continueButton != null)
            continueButton.SetActive(state);
    }

    void SetLocomotionEnabled(bool value)
    {
        if (moveProvider != null)
            moveProvider.enabled = value;

        if (turnProvider != null)
            turnProvider.enabled = value;

        if (teleportProvider != null)
            teleportProvider.enabled = value;
    }
}
