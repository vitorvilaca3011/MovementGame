using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerSpeedDisplay : MonoBehaviour
{
    public StrafeMovement strafeMovement;
    public Rigidbody controller; 
    public TMP_Text speedText;

    void Update()
    {
        Vector3 horizontalVelocity = controller.velocity;
        horizontalVelocity.y = 0f;

        float speed = horizontalVelocity.magnitude;
        speedText.text = $"Speed: {speed:F2}";
    }
}