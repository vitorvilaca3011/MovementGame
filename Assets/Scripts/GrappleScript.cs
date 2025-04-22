using UnityEngine;

public class GrappleScript : MonoBehaviour
{
    private LineRenderer lr;
    private Vector3 grapplePoint;
    public LayerMask whatIsGrappleable;
    public Transform grappleTip, camera, player;
    private SpringJoint joint;

    

    [Header("Grapple Settings")]
    [SerializeField] private float maxGrappleDistance = 100f;
    [SerializeField] private float minGrappleDistance = 10f;
    [SerializeField] private float spring = 4.5f;
    [SerializeField] private float damper = 7f;
    [SerializeField] private float massScale = 4.5f;
    [SerializeField] private float maxGrappleTime = 1.5f; // tempo máximo de puxada
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
    }

    void LateUpdate() // put things that require to be drawn/rendered after everything else
    {
        DrawRope();
    }

    void StartGrapple()
    {
        RaycastHit hit;
        
        if(Physics.Raycast(camera.position, camera.forward, out hit, maxGrappleDistance, whatIsGrappleable)) 
        {
            if (joint != null) // prevent multiple joints
            {
                Destroy(joint);
                return;
            }

            grapplePoint = hit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;

            //Grapple timemark 
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

    void StopGrapple()
    {
        lr.positionCount = 0;
        Destroy(joint);
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
        //if not grapple , no draw rope
        if(joint == null) return;
        
        lr.SetPosition(0, grappleTip.position);
        lr.SetPosition(1, grapplePoint);
    }


} // Class
