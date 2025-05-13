using UnityEngine;

public class StrafeMovement : MonoBehaviour
{
    #region referencias

    [Header("References")]
    [SerializeField]
    private GrappleScript grappleScript;
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private Transform playerTransform;

    #endregion

    #region Movement

    [Header("Movement")]
    [SerializeField]
    private float accel = 200f;
    [SerializeField]
    public float airAccel = 200f;
    [SerializeField]
    private float maxSpeed = 6.4f;
    [SerializeField]
    private float maxAirSpeed = 0.6f;
    [SerializeField]
    private float friction = 8f;
    [SerializeField]
    private float jumpForce = 5f;
    [SerializeField]
    private float grappleMovementMultiplier = 0.4f;

    [SerializeField]
    private GameObject camObj;
    private float lastJumpPress = -1f;
    private float jumpPressDuration = 0.1f;
    private bool onGround = false;

    [Header("CapeGears")]

    public int gearEngaged = 0;

    [Header("")]
    // Straight gear
    [SerializeField]
    private float firstGearAirAccel = 100f; //original 200f
    [SerializeField]
    private float firstGearMaxSpeed = 50f; //original 6.4f
    [SerializeField]
    private float firstGearMaxAirSpeed = 2f; //original 0.6f
    [SerializeField]
    private float firstGearForwardAccel = 0.5f;

    [Header("")]

    // Curve gear
    [SerializeField]
    private float secondGearAirAccel = 350f; //original 200f
    [SerializeField]
    private float secondGearMaxSpeed = 5.0f; //original 6.4f
    [SerializeField]
    private float secondGearMaxAirSpeed = 0.4f; //original 0.6f

    #endregion

    #region Crouch

    [Header("Crouch Settings")]
    [SerializeField]
    private float crouchHeight = 1f;
    [SerializeField]
    private float standHeight = 2f;

    [SerializeField]
    private KeyCode crouchKey = KeyCode.LeftControl;

    private CapsuleCollider col;
    private bool isCrouching = false;

    [Header("Crouch Visual Settings")]
    [SerializeField]
    private Transform camTransform;
    [SerializeField]
    private float standingCamY = 1.6f;
    [SerializeField]
    private float crouchingCamY = 1.0f;
    [SerializeField]
    private float crouchLerpSpeed = 8f;

    #endregion

    #region SFX

    [Header("SFXs")]

    [Header("Wind Sound Settings")]
    [SerializeField]
    private AudioSource windAudio;
    [SerializeField]
    private float windStartSpeed = 15f;
    [SerializeField]
    private float windMaxSpeed = 40f;
    [SerializeField]
    private float maxWindVolume = 0.7f;
    [SerializeField]
    private float windLerpSpeed = 3f;
    [SerializeField]
    private float minAirHeightForWind = 2f;

    [Header("Landing Sound")]
    [SerializeField]
    private AudioSource landAudio;
    [SerializeField]
    private AudioClip[] landClips;
    [SerializeField]
    private float minLandPitch = 0.85f;
    [SerializeField]
    private float maxLandPitch = 1.15f;
    [SerializeField]
    private float minLandVolume = 0.8f;
    [SerializeField]
    private float maxLandVolume = 1.0f;
    [SerializeField]
    private bool wasGrounded = true;

    #endregion

    private void Awake()
    {
        wasGrounded = CheckGround();
        rb = GetComponent<Rigidbody>();
        playerTransform = transform;
        col = GetComponent<CapsuleCollider>();
    }

    private void Update()
    {
        if (Input.GetButton("Jump"))
        {
            lastJumpPress = Time.time;
        }

        if (grappleScript.IsGrappling())
        {
            Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            Vector3 move = playerTransform.TransformDirection(input) * grappleMovementMultiplier;
            rb.AddForce(move, ForceMode.Acceleration);
        }

        Crouch();

        float speed = rb.velocity.magnitude;
        bool inAir = !CheckGround() && GetAirHeight() > minAirHeightForWind;

        if (inAir)
        {
            float t = Mathf.InverseLerp(windStartSpeed, windMaxSpeed, speed);
            float targetVolume = Mathf.Lerp(0f, maxWindVolume, t);
            windAudio.volume = Mathf.Lerp(windAudio.volume, targetVolume, Time.deltaTime * windLerpSpeed);
        }
        else
        {
            windAudio.volume = Mathf.Lerp(windAudio.volume, 0f, Time.deltaTime * windLerpSpeed);
        }
    }

    public void FixedUpdate()
    {
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        Vector3 playerVelocity = rb.velocity;
        playerVelocity = CalculateFriction(playerVelocity);
        playerVelocity += CalculateMovement(input, playerVelocity);
        rb.velocity = playerVelocity;

        if (gearEngaged == 1) // Straight Gear
        {
            Vector3 cameraForward = camObj.transform.forward;
            cameraForward.y = 0; 
            cameraForward.Normalize();

            Vector3 forwardForce = cameraForward * firstGearForwardAccel * Time.fixedDeltaTime;
            rb.AddForce(forwardForce, ForceMode.Acceleration);
        }

        bool nearGround = CheckNearGround();

        if (!wasGrounded && nearGround)
        {
            PlayLandingSound();
        }

        wasGrounded = nearGround;

        CapeGears();
    }

    #region Movement

    private Vector3 CalculateFriction(Vector3 currentVelocity)
    {
        onGround = CheckGround();
        float speed = currentVelocity.magnitude;

        if (!onGround || Input.GetButton("Jump") || speed == 0f)
            return currentVelocity;

        float drop = speed * friction * Time.deltaTime;
        return currentVelocity * (Mathf.Max(speed - drop, 0f) / speed);
    }

    private Vector3 CalculateMovement(Vector2 input, Vector3 velocity)
    {
        onGround = CheckGround();

        float curAccel = onGround ? accel : airAccel;
        float curMaxSpeed = onGround ? maxSpeed : maxAirSpeed;

        Vector3 camRotation = new Vector3(0f, camObj.transform.rotation.eulerAngles.y, 0f);
        Vector3 inputVelocity = Quaternion.Euler(camRotation) *
                                new Vector3(input.x * curAccel, 0f, input.y * curAccel);

        Vector3 alignedInputVelocity = new Vector3(inputVelocity.x, 0f, inputVelocity.z) * Time.deltaTime;
        Vector3 currentVelocity = new Vector3(velocity.x, 0f, velocity.z);

        float max = Mathf.Max(0f, 1 - (currentVelocity.magnitude / curMaxSpeed));
        float velocityDot = Vector3.Dot(currentVelocity, alignedInputVelocity);

        Vector3 modifiedVelocity = alignedInputVelocity * max;
        Vector3 correctVelocity = Vector3.Lerp(alignedInputVelocity, modifiedVelocity, velocityDot);

        correctVelocity += GetJumpVelocity(velocity.y);

        return correctVelocity;
    }

    private Vector3 GetJumpVelocity(float yVelocity)
    {
        Vector3 jumpVelocity = Vector3.zero;

        if (Time.time < lastJumpPress + jumpPressDuration && yVelocity < jumpForce && CheckGround())
        {
            lastJumpPress = -1f;
            jumpVelocity = new Vector3(0f, jumpForce - yVelocity, 0f);
        }

        return jumpVelocity;
    }

    private bool CheckGround()
    {
        float rayDistance = GetComponent<Collider>().bounds.extents.y + 0.05f;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;

        return Physics.SphereCast(rayOrigin, 0.3f, Vector3.down, out RaycastHit hit, rayDistance) &&
               hit.collider.CompareTag("Ground");
    }

    #endregion

    private void Crouch()
    {
        if (Input.GetKeyDown(crouchKey))
        {
            col.height = crouchHeight;
            isCrouching = true;
            float targetY = isCrouching ? crouchingCamY : standingCamY;
            Vector3 camPosition = camTransform.localPosition;
            camPosition.y = Mathf.Lerp(camPosition.y, targetY, Time.deltaTime * crouchLerpSpeed);
            camTransform.localPosition = camPosition;
        }
        else if (Input.GetKeyUp(crouchKey))
        {
            col.height = standHeight;
            isCrouching = false;
        }
    }

    private float GetAirHeight()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            return hit.distance;
        }

        return Mathf.Infinity;
    }

    //! Landing sound effect
    private void PlayLandingSound()
    {
        if (landClips.Length == 0 || rb.velocity.y > -1f) return;

        landAudio.pitch = Random.Range(minLandPitch, maxLandPitch);
        landAudio.volume = Random.Range(minLandVolume, maxLandVolume);
        landAudio.PlayOneShot(landClips[Random.Range(0, landClips.Length)]);
    }

    private bool CheckNearGround()
    {
        float extraDistance = 0.3f;
        float rayDistance = GetComponent<Collider>().bounds.extents.y + extraDistance;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;

        return Physics.SphereCast(rayOrigin, 0.3f, Vector3.down, out RaycastHit hit, rayDistance) &&
               hit.collider.CompareTag("Ground");
    }
    //!

    void CapeGears()
    {
        // manual selection of gears
        if (Input.GetKey(KeyCode.Z))
        {
            airAccel = 200f;
            maxSpeed = 6.4f;
            maxAirSpeed = 0.6f;
        }

        if (Input.GetKey(KeyCode.X))
        {
            // Select gear 1
            gearEngaged = 1;
            airAccel = firstGearAirAccel;
            maxSpeed = firstGearMaxSpeed;
            maxAirSpeed = firstGearMaxAirSpeed;
        }
        else if (Input.GetKey(KeyCode.C))
        {
            // Select gear 2
            gearEngaged = 2;
            airAccel = secondGearAirAccel;
            maxSpeed = secondGearMaxSpeed;
            maxAirSpeed = secondGearMaxAirSpeed;
        }
    }
}