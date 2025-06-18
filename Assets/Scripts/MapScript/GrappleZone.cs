using UnityEngine;

public class GrappleZone : MonoBehaviour
{
    public GrappleScriptV6 grappleScript;
    public GameObject grappleIndicator;
    void Start()
    {
        grappleIndicator.SetActive(false);
        // Ensure the GrappleScriptV6 instance is initialized
        if (GrappleScriptV6.Instance == null)
        {
            Debug.LogError("GrappleScriptV6 instance is not initialized. Please ensure it is set up in the scene.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GrappleScriptV6.Instance.SetGrappleEnabled(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GrappleScriptV6.Instance.SetGrappleEnabled(false);
            grappleIndicator.SetActive(false);
        }

    }
}