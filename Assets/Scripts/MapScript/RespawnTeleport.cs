using UnityEngine;

public class RespawnTeleport : MonoBehaviour
{
    public GameObject player;
    public GameObject respawnPoint;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        respawnPoint = GameObject.FindGameObjectWithTag("Respawn");
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player.transform.position = respawnPoint.transform.position;
        }
    }
}
