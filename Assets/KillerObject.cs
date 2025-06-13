using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillerObject : MonoBehaviour
{
    public Rigidbody rb;
    public GameObject respawnPoint;
    public GameObject player;
    public RespawnTeleport respawnTeleport;

    void Start()
    {
        // Find the player and respawn point in the scene
        player = GameObject.FindGameObjectWithTag("Player");
        respawnPoint = GameObject.FindGameObjectWithTag("Respawn");
        rb = player.GetComponent<Rigidbody>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            // Teleport the player to the respawn point
            respawnTeleport.TeleportPlayer();
        }
    }
}
