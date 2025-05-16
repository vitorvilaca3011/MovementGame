using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class GrappleScriptV6 : MonoBehaviour
{
    [Header("References")]
    public Transform grappleTip;
    public Transform playerCamera;
    public Transform player;
    public GameObject grappleIndicator;
    [SerializeField] private StrafeMovement strafeMovement;

    private LineRenderer lr;
    private Rigidbody rb;

    [Header("Grapple Settings")]
    [SerializeField] private float maxGrappleDistance = 100f;
    [SerializeField] private float cableSpeed = 200f;
    [SerializeField] private float pullForce = 50f;
    [SerializeField] private float pullGravityRatio = 0.1f;
    [SerializeField] private float slingshotForce = 20f;
    [SerializeField] private float slingshotThreshold = 0.2f;
    [SerializeField] private float cooldownTime = 5f;
    [SerializeField] private float maxHoldTime = 1.7f;

    [Header("Jump Boost")]
    [SerializeField] private float jumpBoostMultiplier = 1.5f;
    [SerializeField] private float jumpBoostWindow = 0.25f;
    private bool applyJumpBoost = false;
    private float originalJumpForce;

    [Header("Break Check Delay")]
    [SerializeField] private float breakCheckDelay = 0.15f; // Time before checking if player passed grapple point
    private float grappleEngagedTime;

    private Vector3 grapplePoint;
    private bool isGrappling, isCooldown, isFiring;
    private float lastGrappleTime, grappleStartTime;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        rb = player.GetComponent<Rigidbody>();
        lr.positionCount = 0;

        if (strafeMovement != null)
            originalJumpForce = strafeMovement.jumpForce;
    }

    void Update()
    {
        // Detect grapple target anywhere (no tag restriction)
        RaycastHit hit;
        bool canGrapple = Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, maxGrappleDistance)
                          && !isCooldown;
        grappleIndicator.SetActive(canGrapple && !isGrappling);

        // Start grapple
        if (Input.GetMouseButtonDown(1) && canGrapple)
            StartCoroutine(FireGrapple(hit.point));

        // Manual release
        if (isGrappling && Input.GetMouseButtonUp(1))
            StopGrapple();

        // Auto release after max hold time
        if (isGrappling && Time.time - grappleStartTime >= maxHoldTime)
            StopGrapple();

        // Jump boost logic
        if (isGrappling && applyJumpBoost && Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * strafeMovement.jumpForce * (jumpBoostMultiplier - 1f), ForceMode.VelocityChange);
            applyJumpBoost = false;
        }
    }

    void LateUpdate()
    {
        if (isGrappling)
        {
            // Check if player passed grapple point after a small delay
            if (Time.time - grappleEngagedTime > breakCheckDelay)
            {
                Vector3 dirToHook = (grapplePoint - player.position).normalized;
                float dot = Vector3.Dot(rb.velocity.normalized, dirToHook);
                if (dot < 0.2f)
                {
                    StopGrapple();
                    return;
                }
            }

            // Draw grapple cable
            lr.SetPosition(0, grappleTip.position);
            lr.SetPosition(1, grapplePoint);

            // Apply pulling force with gravity factor
            Vector3 dir = (grapplePoint - player.position).normalized;
            Vector3 pull = dir * pullForce;
            Vector3 gravity = Physics.gravity * pullGravityRatio;
            rb.AddForce(pull + gravity, ForceMode.Acceleration);
        }
    }

    private IEnumerator FireGrapple(Vector3 targetPoint)
    {
        isFiring = true;
        grapplePoint = targetPoint;
        float distance = Vector3.Distance(grappleTip.position, grapplePoint);
        float delay = distance / cableSpeed;

        lr.positionCount = 2;
        float t = 0f;
        while (t < delay)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / delay);
            Vector3 mid = Vector3.Lerp(grappleTip.position, grapplePoint, lerp);
            lr.SetPosition(0, grappleTip.position);
            lr.SetPosition(1, mid);
            yield return null;
        }

        // Engage grapple and initialize timers
        isGrappling = true;
        isFiring = false;
        grappleStartTime = Time.time;
        lastGrappleTime = Time.time;
        grappleEngagedTime = Time.time;

        // Enable jump boost window and modify jump force
        if (strafeMovement != null)
        {
            applyJumpBoost = true;
            strafeMovement.jumpForce = originalJumpForce * jumpBoostMultiplier;
            Invoke(nameof(DisableJumpBoost), jumpBoostWindow);
        }
    }

    private void DisableJumpBoost()
    {
        applyJumpBoost = false;
        if (strafeMovement != null)
            strafeMovement.jumpForce = originalJumpForce;
    }

    private void StopGrapple()
    {
        if (!isGrappling) return;

        float dist = Vector3.Distance(player.position, grapplePoint);
        if (dist <= slingshotThreshold)
        {
            Vector3 launchDir = (grapplePoint - player.position).normalized;
            rb.AddForce(launchDir * slingshotForce, ForceMode.VelocityChange);
        }

        // Reset grapple visuals and flags
        lr.positionCount = 0;
        isGrappling = false;
        applyJumpBoost = false;

        if (strafeMovement != null)
            strafeMovement.jumpForce = originalJumpForce;

        StartCoroutine(StartCooldown());
    }

    private IEnumerator StartCooldown()
    {
        isCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        isCooldown = false;
    }

    // Public getters for other scripts
    public bool IsGrappling() => isGrappling || isFiring;
    public bool IsOnCooldown() => isCooldown;
}
