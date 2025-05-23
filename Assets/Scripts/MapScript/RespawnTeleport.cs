using System.Collections;
using UnityEditor.Callbacks;
using UnityEngine;

public class RespawnTeleport : MonoBehaviour
{
    public GameObject player;

    private StrafeMovement strafeMovement;
    public Rigidbody rb;
    public GameObject respawnPoint;
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

    }

    void TeleportPlayer()
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
