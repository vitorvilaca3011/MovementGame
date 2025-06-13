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
    [SerializeField] private float grapplePullSpeed = 60f; // Pathfinder: rápido!
    [SerializeField] private float grappleSpringiness = 0.2f; // Quanto mais baixo, mais "elástico"
    [SerializeField] private float grappleDamping = 0.8f; // Damping para não oscilar demais
    [SerializeField] private float cooldownTime = 5f;
    [SerializeField] private float maxHoldTime = 2.0f;
    public static GrappleScriptV6 Instance { get; private set; }
    
    [SerializeField]private bool grappleEnabled = false; // Flag to enable/disable grapple
   
    // Public getters for other scripts, dont know if ill use it
    public bool IsGrappling() => isGrappling || isFiring;
    public bool IsOnCooldown() => isCooldown;


    [Header("Jump Boost")]
    [SerializeField] private float jumpBoostMultiplier = 1.5f;
    [SerializeField] private float jumpBoostWindow = 0.25f;
    private bool applyJumpBoost = false;
    private float originalJumpForce;

    private Vector3 grapplePoint;
    private bool isGrappling, isCooldown, isFiring;
    private float lastGrappleTime, grappleStartTime;

    void Awake()
    {
        Instance = this;
        lr = GetComponent<LineRenderer>();
        rb = player.GetComponent<Rigidbody>();
        lr.positionCount = 0;

        if (strafeMovement != null)
            originalJumpForce = strafeMovement.jumpForce;
    }

    void Update()
    {
        if (!grappleEnabled || isCooldown) return; // Check if grapple is enabled
        
        RaycastHit hit;
        bool canGrapple = Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, maxGrappleDistance)
                          && !isCooldown
                          && hit.collider != null
                  && hit.collider.gameObject != player.gameObject;


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
            // Draw grapple cable
            lr.SetPosition(0, grappleTip.position);
            lr.SetPosition(1, grapplePoint);

            //spring force + direct move
            Vector3 toTarget = grapplePoint - player.position;
            float distance = toTarget.magnitude;

            // Spring force (Pathfinder style)
            Vector3 springForce = toTarget.normalized * (distance * grappleSpringiness);
            rb.velocity = Vector3.Lerp(rb.velocity, springForce + toTarget.normalized * grapplePullSpeed, grappleDamping * Time.deltaTime);

            // Guarantee arrival: if very close, snap to point and stop
            float arriveThreshold = 1.2f;
            if (distance < arriveThreshold)
            {
                player.position = grapplePoint;
                StopGrapple();
            }
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

    
    public void SetGrappleEnabled(bool enabled)
    {
        grappleEnabled = enabled;

        if (!enabled && isGrappling)
        {
            StopGrapple(); // Se o player sair da zona, quebra o hook
        }
    }
}