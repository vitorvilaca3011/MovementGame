using UnityEngine;

public class CameraFOV : MonoBehaviour
{
    
    public float baseFOV = 60f;
    public float maxFOV = 90f;
    public float fovSpeedFactor = 0.2f; // speed that fov changes
    public bool dynamicFov = true; // if true, fov changes based on speed
    [SerializeField]private Rigidbody playerRb;
    [SerializeField]private Camera cam;

    private void Start()
    {
        dynamicFov = PlayerPrefs.GetInt("DynamicFOV", 1) == 1; // Load dynamicFov preference from PlayerPrefs
    }

    void Update()
    {
        if(dynamicFov == true)
        {
        float speed = playerRb.velocity.magnitude;
        float targetFOV = baseFOV + (speed * fovSpeedFactor);
        targetFOV = Mathf.Clamp(targetFOV, baseFOV, maxFOV);

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * 5f);
        }
        else
        {
            cam.fieldOfView = baseFOV;
        }
    }
}
