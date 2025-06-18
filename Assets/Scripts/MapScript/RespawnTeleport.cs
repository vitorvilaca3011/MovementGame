using System.Collections;
#if UNITY_EDITOR
using UnityEditor.Callbacks;
#endif
using UnityEngine;

public class RespawnTeleport : MonoBehaviour
{
    public GameObject player;

    private StrafeMovement strafeMovement;
    public Rigidbody rb;
    public GameObject respawnPoint;
    public GrappleScriptV6 grappleScript;
    void Start()
    {
        strafeMovement = player.GetComponent<StrafeMovement>();
        player = GameObject.FindGameObjectWithTag("Player");
        respawnPoint = GameObject.FindGameObjectWithTag("Respawn");
        rb = player.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Optional: You can add logic here if needed
        if (Input.GetKeyDown(KeyCode.R))
        {
            TeleportPlayer();
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Teleport the player to the respawn point
            TeleportPlayer();
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("KillerObject") && other.gameObject == player)
        {
            // If the player touches an object with the "KillerObject" layer, teleport the player
            TeleportPlayer();
        }
    }

    public void TeleportPlayer()
    {
        rb.velocity = Vector3.zero; // Reset player velocity
        StartCoroutine(DelayToMove());
        player.transform.position = new Vector3(respawnPoint.transform.position.x, respawnPoint.transform.position.y + 1f, respawnPoint.transform.position.z);
    }
    
    private IEnumerator DelayToMove()
    {
        rb.velocity = Vector3.zero; // Reset player velocity
        strafeMovement.canMove = false;
        yield return new WaitForSeconds(0.3f);
        strafeMovement.canMove = true;
    }

    
}
