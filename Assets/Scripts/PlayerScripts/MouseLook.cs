using UnityEngine;
using System.Collections;

/// MouseLook rotates the transform based on the mouse delta.
/// Minimum and Maximum values can be used to constrain the possible rotation

/// To make an FPS style character:
/// - Create a capsule.
/// - Add the MouseLook script to the capsule.
///   -> Set the mouse look to use LookX. (You want to only turn character but not tilt it)
/// - Add FPSInputController script to the capsule
///   -> A CharacterMotor and a CharacterController component will be automatically added.

/// - Create a camera. Make the camera a child of the capsule. Reset it's transform.
/// - Add a MouseLook script to the camera.
///   -> Set the mouse look to use LookY. (You want the camera to tilt up and down like a head. The character already turns.)
[AddComponentMenu("Camera-Control/Mouse Look")]
public class MouseLook : MonoBehaviour {

	public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
	public RotationAxes axes = RotationAxes.MouseXAndY;
	private float sensitivityX = 15F;
	private float sensitivityY = 15F;

	public float minimumX = -360F;
	public float maximumX = 360F;

	public float minimumY = -60F;
	public float maximumY = 60F;

	public bool invertY = false;

	float rotationY = 0F;
    
	private float sensitivity;

	public bool isMouseLookEnabled = true;

    void Update ()
	{
		if (!isMouseLookEnabled) return; // Exit if mouse look is disabled

        float ySens = sensitivityY;
		if(invertY) { ySens *= -1f; }

		if (axes == RotationAxes.MouseXAndY)
		{
			float rotationX = transform.localEulerAngles.y + GetMouseX() * sensitivityX;
			
			rotationY += GetMouseY() * ySens;
			rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
			
			transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
		}
		else if (axes == RotationAxes.MouseX)
		{
			transform.Rotate(0, GetMouseX() * sensitivityX, 0);
		}
		else
		{
			rotationY += GetMouseY() * ySens;
			rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
			
			transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
		}

        sensitivityX = PlayerPrefs.GetFloat("SensitivityX", 15f);
        sensitivityY = PlayerPrefs.GetFloat("SensitivityY", 15f);
    }
	
	void Start ()
	{
        sensitivityX = PlayerPrefs.GetFloat("MouseSensitivityX", sensitivityX);
        sensitivityY = PlayerPrefs.GetFloat("MouseSensitivityY", sensitivityY);

        Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		// Make the rigid body not change rotation
		if (GetComponent<Rigidbody>())
			GetComponent<Rigidbody>().freezeRotation = true;
	}

    float GetMouseX()
    {
        return Input.GetAxis("Mouse X");
    }

    float GetMouseY()
    {
        return Input.GetAxis("Mouse Y");
    }
}