using UnityEngine;
using UnityEngine.UIElements;

public class StrafeMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private GameObject camObj;

    [Header("Movement Settings")]
    [SerializeField] private float groundAccel = 60f;
    [SerializeField] private float airAccel = 100f;
    [SerializeField] public float groundFriction = 8f;
    [SerializeField] private float maxGroundSpeed = 7f;
    [SerializeField] private float maxAirSpeed = 0.8f;
    [SerializeField] public float jumpForce;
    public bool canMove = true; // Public variable to control movement      
    public bool JumpBufferMode = false; // Public variable to control jump mode
    [Header("Crouch Settings")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] private Transform camTransform;
    [SerializeField] private float standingCamY = 1.6f;
    [SerializeField] private float crouchingCamY = 1.0f;
    [SerializeField] private float crouchLerpSpeed = 8f;

    [Header("Landing Sound")]
    [SerializeField] private AudioSource landAudio;
    [SerializeField] private AudioClip[] landClips;
    [SerializeField] private float minLandPitch = 0.95f;
    [SerializeField] private float maxLandPitch = 1.05f;
    [SerializeField] private float minLandVolume = 0.8f;
    [SerializeField] private float maxLandVolume = 1.0f;

    private BoxCollider col;
    private bool isCrouching = false;
    private bool isGrounded = false;
    private float lastJumpPress = -1f;
    private float jumpPressDuration = 0.1f;
    private bool wasGrounded = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerTransform = transform;
        col = GetComponent<BoxCollider>();
    }

    void Update()
    {
        if (!canMove) return; // check if movement is allowed

        // switch mode to jump mode
        if (JumpBufferMode == true) // queued mode/ double tap
        {
            if (Input.GetButtonDown("Jump"))
                lastJumpPress = Time.time;

        }
        else // autobhop mode/ hold
        {
            if (Input.GetButton("Jump"))
                lastJumpPress = Time.time;

        }


        HandleCrouch();
    }

    void FixedUpdate()
    {
        if (!canMove) return; // Check if movement is allowed

        isGrounded = CheckGround();

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector3 wishDir = GetWishDirection(input);

        if (isGrounded)
        {
            ApplyFriction();
            Accelerate(wishDir, maxGroundSpeed, groundAccel);

            if (Time.time < lastJumpPress + jumpPressDuration)
            {
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // Reset vertical velocity
                rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
                lastJumpPress = -1f;
            }
        }
        else
        {
            AirAccelerate(wishDir, maxAirSpeed, airAccel);
        }

        // Landing sound effect
        if (!wasGrounded && (isGrounded || IsNearGround()))
        {
            PlayLandingSound();
        }
        wasGrounded = isGrounded || IsNearGround();
    }

    // Calculates the movement direction based on camera orientation and input
    Vector3 GetWishDirection(Vector2 input)
    {
        Vector3 camForward = camObj.transform.forward;
        Vector3 camRight = camObj.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();
        return (camForward * input.y + camRight * input.x).normalized;
    }

    // Ground acceleration (CS-style: low accel, strong friction)
    void Accelerate(Vector3 wishDir, float maxSpeed, float accel)
    {
        float projVel = Vector3.Dot(rb.velocity, wishDir);
        float addSpeed = maxSpeed - projVel;
        if (addSpeed <= 0) return;

        float accelSpeed = accel * Time.fixedDeltaTime;
        if (accelSpeed > addSpeed)
            accelSpeed = addSpeed;

        rb.AddForce(wishDir * accelSpeed, ForceMode.VelocityChange);
    }

    // Air acceleration (CS-style: very limited)
    void AirAccelerate(Vector3 wishDir, float maxSpeed, float accel)
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        float projVel = Vector3.Dot(flatVel, wishDir);
        float addSpeed = maxSpeed - projVel;
        if (addSpeed <= 0) return;

        float accelSpeed = accel * Time.fixedDeltaTime;
        if (accelSpeed > addSpeed)
            accelSpeed = addSpeed;

        rb.AddForce(wishDir * accelSpeed, ForceMode.VelocityChange);
    }

    // Strong friction for quick stops (CS feel)
    void ApplyFriction()
    {
        Vector3 vel = rb.velocity;
        vel.y = 0f;
        float speed = vel.magnitude;
        if (speed < 0.001f) return;

        float drop = speed * groundFriction * Time.fixedDeltaTime;
        float newSpeed = Mathf.Max(speed - drop, 0f);
        if (newSpeed != speed)
        {
            rb.velocity = vel.normalized * newSpeed + Vector3.up * rb.velocity.y;
        }
    }

    // Simple ground check using SphereCast
    bool CheckGround()
    {
        float rayDistance = col.bounds.extents.y + 0.05f;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        return Physics.SphereCast(rayOrigin, 0.3f, Vector3.down, out RaycastHit hit, rayDistance) &&
               hit.collider.CompareTag("Ground");
    }

    // Handles crouch logic and camera lerp
    void HandleCrouch()
    {
        if (Input.GetKeyDown(crouchKey))
        {
            Vector3 size = col.size;
            size.y = crouchHeight;
            col.size = size;
            isCrouching = true;
        }
        else if (Input.GetKeyUp(crouchKey))
        {
            Vector3 size = col.size;
            size.y = standHeight;
            col.size = size;
            isCrouching = false;
        }

        float targetY = isCrouching ? crouchingCamY : standingCamY;
        Vector3 camPosition = camTransform.localPosition;
        camPosition.y = Mathf.Lerp(camPosition.y, targetY, Time.deltaTime * crouchLerpSpeed);
        camTransform.localPosition = camPosition;
    }

    // Plays a random landing sound effect when landing
    void PlayLandingSound()
    {
        if (landClips.Length == 0 || rb.velocity.y > -1f) return;

        landAudio.pitch = Random.Range(minLandPitch, maxLandPitch);
        landAudio.volume = Random.Range(minLandVolume, maxLandVolume);
        landAudio.PlayOneShot(landClips[Random.Range(0, landClips.Length)]);
    }

    bool IsNearGround()
    {
        float checkDistance = 1.5f; // Small distance to check below the player
        Vector3 origin = transform.position + Vector3.up * 0.05f;

        // Draw the ray in the Scene view for debugging
        Debug.DrawRay(origin, Vector3.down * checkDistance, Color.green);

        return Physics.Raycast(origin, Vector3.down, checkDistance);
    }
}