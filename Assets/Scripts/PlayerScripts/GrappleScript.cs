using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class GrappleScript : MonoBehaviour
{
    private LineRenderer lr;
    private Vector3 grapplePoint;
    public Transform grappleTip, player;
    public Transform playerCamera;
    private SpringJoint joint;
    public GameObject grappleIndicator;

    [Header("Grapple Settings")]
    [SerializeField] private float maxGrappleDistance = 100f;
    [SerializeField] private float spring = 4.5f;
    [SerializeField] private float damper = 7f;
    [SerializeField] private float massScale = 4.5f;
    [SerializeField] private float maxGrappleTime = 1.5f;
    [SerializeField] private float grappleBoostForce = 15f;

    private float grappleStartTime;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    void Update() 
    {
        if (Input.GetMouseButtonDown(1)) 
        {
            StartGrapple();
        }
        else if (Input.GetMouseButtonUp(1) || (IsGrappling() && Time.time - grappleStartTime > maxGrappleTime))
        {
            StopGrapple();
        }

        if (IsGrappling() && Vector3.Distance(player.position, grapplePoint) <= joint.minDistance + 0.5f)
        {
            StopGrapple();
        }

        if (IsGrappling() && Input.GetKey(KeyCode.S))
        {
            StopGrapple();
        }
        
        // Aim grapple indicator configs
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, maxGrappleDistance))
        {
            if (hit.collider.CompareTag("Grappleable"))
            {
                grappleIndicator.gameObject.SetActive(true);
            }
            else
            {
                grappleIndicator.gameObject.SetActive(false);
            }
        }
        else
        {
            grappleIndicator.gameObject.SetActive(false);
        }

        if (IsGrappling())
        {
            grappleIndicator.gameObject.SetActive(false);
        }
    }

    void LateUpdate()
    {
        DrawRope();
    }

    void StartGrapple()
    {
        RaycastHit hit;
        
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, maxGrappleDistance)) 
        {
            if (hit.collider.CompareTag("Grappleable"))
            {
                if (joint != null)
                {
                    Destroy(joint);
                    return;
                }

                grapplePoint = hit.point;
                joint = player.gameObject.AddComponent<SpringJoint>();
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = grapplePoint;

                grappleStartTime = Time.time;

                float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);

                joint.maxDistance = distanceFromPoint * 0.8f;
                joint.minDistance = distanceFromPoint * 0.1f;

                joint.spring = spring;
                joint.damper = damper;
                joint.massScale = massScale;

                lr.positionCount = 2;
            }
        }
    }

    void StopGrapple()
    {
        if (joint != null)
        {
            float distanceToPoint = Vector3.Distance(player.position, grapplePoint);

            if (distanceToPoint <= joint.minDistance + 7f) // slingshot effect applies at near push end distance, and close to max range
            {
                Vector3 launchDir = grapplePoint - player.position;
                launchDir.y *= 0.1f; // reduce vertical component of the direction
                launchDir = launchDir.normalized;
                Rigidbody rb = player.GetComponent<Rigidbody>();
                rb.AddForce(launchDir * grappleBoostForce, ForceMode.VelocityChange);

                Debug.Log("<color=cyan>Grapple BOOST!</color>");
            }

            lr.positionCount = 0;
            Destroy(joint);
        }
    }

    public bool IsGrappling()
    {
        return joint != null;
    }

    public Vector3 GetGrapplePoint()
    {
        return grapplePoint;
    }

    void DrawRope() 
    {
        if (joint == null) return;
        
        lr.SetPosition(0, grappleTip.position);
        lr.SetPosition(1, grapplePoint);
    }
}