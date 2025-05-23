using UnityEngine;

public class WallJump : MonoBehaviour
{
    [Header("Settings")]
    public float bounceForce = 12f;
    public float verticalForce = 6f;
    public float detectionDistance = 1f;
    public float wallJumpCooldown = 0.5f;

    [Header("References")]
    public Rigidbody rb;
    public Transform playerCam;

    private bool isTouchingWall;
    private Vector3 lastWallNormal;

    private float lastWallJumpTime = -10f; // Inicializa para garantir que pode pular no início

    [Header("SfX")]
    [SerializeField] private AudioSource wallJumpAudio;
    [SerializeField] private AudioClip[] wallJumpAudioClips;
    [SerializeField] private float minLandPitch = 0.95f;
    [SerializeField] private float maxLandPitch = 1.05f;
    [SerializeField] private float minLandVolume = 0.8f;
    [SerializeField] private float maxLandVolume = 1.0f;
    void Update()
    {
        CheckWall();

        if (isTouchingWall && Input.GetButton("Jump"))
        {
            if (Input.GetKey(KeyCode.W)) // Only perform wall bounce if W is pressed
            {
                return;
            }
            else
            {
                if (Time.time - lastWallJumpTime > wallJumpCooldown)
                {
                    lastWallJumpTime = Time.time;
                    PerformWallBounce();
                }
            }
        }
    }

    void CheckWall()
    {
        //RaycastHit hit;

        Vector3 origin = new Vector3(playerCam.position.x, playerCam.position.y - 0.5f, playerCam.position.z);  
        Vector3 direction = playerCam.forward;

        // Draw the ray in the Scene view for debugging
        //Debug.DrawRay(origin, direction * detectionDistance, Color.red); //!Debug walljump ray

        // Use RaycastAll and ignore self-collisions
        RaycastHit[] hits = Physics.RaycastAll(origin, direction, detectionDistance);
        isTouchingWall = false;

        foreach (var h in hits)
        {
            if (h.collider != null && h.collider.gameObject != gameObject)
            {
                isTouchingWall = true;
                lastWallNormal = h.normal;
                break;
            }
        }
    }


    void PerformWallBounce()
    {
        // Reflect the player's current velocity on the wall normal
        Vector3 incomingVelocity = rb.velocity;

        // Zero out vertical velocity before wall jump to avoid stacking forces
        incomingVelocity.y = 0;

        Vector3 reflectedVelocity = Vector3.Reflect(incomingVelocity, lastWallNormal);

        // Set the new velocity to the reflected vector, keeping the same magnitude or bounceForce (whichever is greater)
        Vector3 finalVelocity = reflectedVelocity.normalized * Mathf.Max(incomingVelocity.magnitude, bounceForce);

        // Ensure minimum vertical force
        finalVelocity.y = Mathf.Max(verticalForce, rb.velocity.y);

        rb.velocity = finalVelocity;

        PlaySound(); // Play sound on wall jump
    }

    void PlaySound()
    {
        if (wallJumpAudioClips.Length == 0) return;

        wallJumpAudio.pitch = Random.Range(minLandPitch, maxLandPitch);
        wallJumpAudio.volume = Random.Range(minLandVolume, maxLandVolume);
        wallJumpAudio.PlayOneShot(wallJumpAudioClips[Random.Range(0, wallJumpAudioClips.Length)]);
    }
}