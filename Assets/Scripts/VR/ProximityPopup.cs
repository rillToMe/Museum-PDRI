using UnityEngine;

public class ProximityPopup : MonoBehaviour
{
    public GameObject popup;      
    public string playerTag = "Player";

    void Start()
    {
        if (popup != null)
            popup.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (popup != null)
                popup.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (popup != null)
                popup.SetActive(false);
        }
    }
}
